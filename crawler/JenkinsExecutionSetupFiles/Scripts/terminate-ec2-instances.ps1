# Terminates ec2-instances which are in 1) running state 2) have the relevant job tag name
param (
	[string]$awsJobNameTag
)

$instanceIds = aws ec2 describe-instances `
    --filters "Name=tag:project,Values=qa-automation" "Name=tag-key,Values=product" "Name=instance-state-name,Values=pending,running" "Name=tag:jobName,Values=$awsJobNameTag" `
	--query Reservations[*].Instances[*].[InstanceId] `
    --output text
	
if ($instanceIds.Count -eq 0){
	Write-Host "WARNING: No instance found to shut down"
} else {
	# Terminate the instance
	$instanceObj = aws ec2 terminate-instances --instance-ids $instanceIds | ConvertFrom-Json
	$currentStateList = $instanceObj.TerminatingInstances.CurrentState.Name
	# Check if the shutdown actually took effect
	foreach($currentState in $currentStateList){
		if ($currentState -ne "shutting-down"){
			Write-Host "Error: One or more instance have not shut down"
			exit 1
		}
	}
	Write-Host "Completed: All instances [$instanceIds] are set to shutting down state"
}

