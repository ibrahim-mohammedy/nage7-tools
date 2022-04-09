# Creates EC2 instance and then connects RDP Sessions
param (
	[int]$count, # number of instances
	[string]$requestSpotInstances,
	[string]$awsImage, # image id for creating the slave nodes
	[string]$awsInstanceType,
	[string]$awsSecurityGroup,
	[string]$awsSubnet,
    [string]$awsIamProfileArn,
    [string]$awsProductTag,
    [string]$awsJobNameTag,
	[string]$ndLoginUsername, # credentials to rdp into the node
	[string]$ndLoginEncryptedPassword,
	[string]$ndScreenWidth='1920',
	[string]$ndScreenHeight='1080',
    [string]$cipherDllPath # path for cipher dll stored typically in the libraries folder
)

# Define function/script block to initalize VM
$InitializeVM = {
    param(
        [string] $server,
        [string] $username,
        [string] $password,
        [int] $width,
        [int] $height
    )
    # Establish the RDP session
    # parameters to poll for rdp process to complete
    [int]$timeout = 120
    [int]$qInterval = 10
    [int]$consecutiveMatches = 3
    [int]$maxRDPattempts = 3
    [double]$minCPUForSuccessfulConnection = .2
    [int]$currentRDPattempt = 0
    do{
		$currentRDPattempt += 1
	    #Write-Host "DEBUG: RDP Connection to [$server] attempt [$currentRDPattempt]"
	    [string]$errorMessge = $null
	    try {
		    cmdkey /generic:TERMSRV/$server /user:$username /pass:$password | Out-Null
		    $appId = (Start-Process -FilePath "mstsc.exe" -ArgumentList "/v:$server /w:$width /h:$height" -PassThru).Id
		    #Write-Host "Debug: AppId is [$appId]"
	    } catch {
		    $message = $_.Exception.Message
		    throw "ERROR: Establishing RDP connection with [$server]. ErrorMessage: $message"
	    }
	    ## Wait for RDP connection to Succeed and CPU to Stabalize
	    $counter = 0
	    $matchCounter = 0
	    $previousCPU = 0
	    $counterLimit = $timeout/$qInterval
	    while (($counter -le $counterLimit) -and ($matchCounter -lt $consecutiveMatches))
	    {
		    $currentCPU = (Get-Process -Id $appId | Select-Object CPU).CPU
		    #Write-Host "CurrentCPU=$currentCPU && PreviousCPU=$previousCPU"
		    if ($currentCPU -eq $previousCPU){
			    $matchCounter += 1
		    } else {
			    $previousCPU = $currentCPU
			    $matchCounter = 0
		    }
		    Start-Sleep $qInterval
		    $counter += 1
	    }
	    #Write-Host "DEBUG: PreviousCPU=$previousCPU"
	    if ($counter -ge $counterLimit ){
		    $errorMessge = "ERROR: Unable to start a RDP session with [$server]. RDP Session did NOT Stabalized till timeout ($timeout seconds)"
		    Write-Host $errorMessge
	    } elseif ($previousCPU -le $minCPUForSuccessfulConnection){
		    $errorMessge = "ERROR: Unable to start a RDP session with [$server]. RDP Session CPU was [$previousCPU] which is less than the threshold [$minCPUForSuccessfulConnection]"
		    Write-Host $errorMessge
	    }
	    ## Try close process
	    try {
		    Stop-Process -Id $appId
	    } catch { 
		    # do nothing
	    }
    } while (($currentRDPattempt -le $maxRDPattempts) -and -Not([string]::IsNullOrEmpty($errorMessge)))
    ## Return error/success
    if (-Not([string]::IsNullOrEmpty($errorMessge))){
	    throw "ERROR: Unable to establish RDP Connection to [$server] after [$currentRDPattempt] attempts"
    }
    return "SUCCESS: Establishing RDP session with [$server] has been successful after [$currentRDPattempt] attempts"
}

# Create EC2 instance
$awsTagSpecification = "ResourceType=instance,Tags=[{Key=project,Value=qa-automation},{Key=product,Value=$awsProductTag},{Key=jobName,Value=$awsJobNameTag}]"
$awsIamProfile = "Arn=$awsIamProfileArn"
if ([System.Convert]::ToBoolean($requestSpotInstances) -eq $true){
    $instanceObj = aws ec2 run-instances `
                        --image-id $awsImage `
                        --count $count `
                        --instance-type $awsInstanceType `
                        --security-group-ids $awsSecurityGroup `
                        --subnet-id $awsSubnet `
                        --iam-instance-profile $awsIamProfile `
                        --tag-specifications $awsTagSpecification `
                        --instance-market-options 'MarketType=spot,SpotOptions={SpotInstanceType=one-time}' `
                        | ConvertFrom-Json
} else {
    $instanceObj = aws ec2 run-instances `
                        --image-id $awsImage `
                        --count $count `
                        --instance-type $awsInstanceType `
                        --security-group-ids $awsSecurityGroup `
                        --subnet-id $awsSubnet `
                        --iam-instance-profile $awsIamProfile `
                        --tag-specifications $awsTagSpecification `
                        | ConvertFrom-Json
}
if ($instanceObj -eq $null) {
    Write-Host "Error: Creating instances" -ForegroundColor Red
    exit 1
}
$instanceIds = $instanceObj.Instances.InstanceId
Write-Host "Completed: Instances created [$instanceIds]"
Write-Host "Waiting for instances [$instanceIds] to complete status check"
aws ec2 wait instance-status-ok --instance-ids $instanceIds
# Get instance private Ips
$servers = (aws ec2 describe-instances --instance-ids $instanceIds | ConvertFrom-Json).Reservations.Instances.PrivateIpAddress

Write-Host "SUCCESS: VM [$servers] has been created and have completed status check" -ForegroundColor Green 

## Establish RDP Connection
#Write-Host "Waiting 90 seconds before attempting to start RDP"
#Start-Sleep 90
$jobs = @()
Add-Type -Path $cipherDllPath
# Start the initialization on seprate threads
Write-Host "Starting server initializations"
foreach($server in $servers){
    $jobs += Start-Job -Name VMInit -Scriptblock $InitializeVM -ArgumentList $server,$ndLoginUsername,$([Cipher.cipher]::DecryptString($ndLoginEncryptedPassword)),$ndScreenWidth,$ndScreenHeight
}
# Wait for all server initialization to complete
Write-Host "Waiting for server initializations to complete"
$allJobsCount = Wait-Job -Name VMInit
# Print status of initialization for each VM
$failedServers = 0
foreach ($job in $jobs) {
    if ($job.State -eq 'Failed') {
        Write-Host ($job.ChildJobs[0].JobStateInfo.Reason.Message) -ForegroundColor Red
        $failedServers++
    } else {
        Write-Host (Receive-Job $job) -ForegroundColor Green
    }
}
# Terminate Job/Threads
Remove-Job -Name VMInit
# Summarize result
if ($failedServers -ge 1) {
    throw "ERROR: Few VMs failed to initialize. Count = [$($failedServers)]"
} else {
    Write-Host "SUCCESS: All VMs initialized. Count = [$($jobs.Count)]" -ForegroundColor Green 
}