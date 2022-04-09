# Gets ec2-instances which are in 1) running state 2) have the relevant job tag name
# Validates that jenkins service is running on these instances
# Stores ipaddresses in server.txt file in workspace root
param (
	[string] $awsJobNameTag,
    [int] $count, # number of instances that are required
    [string] $s3resultFolder,
    [int] $timeoutForJenkinsAvailability
)

# Define function/script block to confirm availability of Jenkins Server
$serverAvailabilityCheck = {
    param ( 
        [string] $ipAddress,
        [int] $timeout = 30 # in seconds
        )
    $jenkinsUrl = 'http://'+$ipAddress+':8080/login'
    $stopwatch =  [system.diagnostics.stopwatch]::StartNew()
    while ($stopwatch.Elapsed.TotalSeconds -le $timeout){
        try{
            $HTTP_Request = [System.Net.WebRequest]::Create($jenkinsUrl)
            $HTTP_Request.Timeout = 30000 # web-request timeout hard coded to 30 seconds
            $HTTP_Response = $HTTP_Request.GetResponse()
            $HTTP_Status = [int]$HTTP_Response.StatusCode
        } catch {
            $HTTP_Status = -1
        }
        If ($HTTP_Status -eq 200){
            break
        }
        Start-Sleep -Seconds 10 # interval between queries hard coded to 10 seconds
    }
	$stopwatch.Stop()
    If (($HTTP_Status -eq $null) -Or ($HTTP_Status -ne 200)){
        throw "ERROR: Jenkins on [$ipAddress] is NOT operational after [$($stopwatch.Elapsed.TotalSeconds)] seconds"
    }
    If ($HTTP_Response -ne $null) {
        $HTTP_Response.Close() 
    }
    return "SUCCESS: Jenkins on [$ipAddress] is operational"
}

$allipaddresses = aws ec2 describe-instances `
    --filters "Name=tag:project,Values=qa-automation" "Name=tag-key,Values=product" "Name=instance-state-name,Values=pending,running" "Name=tag:jobName,Values=$awsJobNameTag" `
	--query Reservations[*].Instances[*].[PrivateIpAddress] `
    --output text
	
if ($allipaddresses.Count -lt $count){
	throw "ERROR: Available # instnaces [$($ipaddresses.Count)] is less than requested number of instances [$count]"
}

# If more servers are available than the requested then filter out servers that are not required
$ipaddresses = @()
for($i=0; $i -lt $count; $i++){
    if(($i -eq 0) -and ($allipaddresses.Count -eq 1)) {
        $ipaddresses += $allipaddresses
    } else {
        $ipaddresses += $allipaddresses[$i]
    }
}

# Cycle through and confirm all the instances have Jenkins running
$jobs = @()
Write-Host "Starting ping all Jenkins server nodes"
foreach($ipaddress in $ipaddresses){
    $jobs += Start-Job -Name ServerCheck -Scriptblock $serverAvailabilityCheck -ArgumentList $ipaddress, $timeoutForJenkinsAvailability
}
Write-Host "Waiting for ping operations to complete"
$allJobsCount = Wait-Job -Name ServerCheck
# Print status of Server Check
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
Remove-Job -Name ServerCheck
# Summarize result
if ($failedServers -ge 1) {
    throw "ERROR: Few servers do NOT have Jenkins services running. Count = [$($failedServers)]"
} else {
    Write-Host "SUCCESS: All servers/instances have Jenkins running. Count = [$($jobs.Count)]" -ForegroundColor Green 
}

# Create comma seperated string of all the instances and add to env valiable
foreach($ipaddress in $ipaddresses){
    $serverString = $serverString + "," + $ipaddress
}
$serverString = $serverString.TrimStart(',')
"servers=$serverString" | Out-File -Encoding "ASCII" "$s3resultFolder\servers.txt"

Write-Host "Complete: Recording values of ip addresses in env file [$s3resultFolder\servers.txt]"

