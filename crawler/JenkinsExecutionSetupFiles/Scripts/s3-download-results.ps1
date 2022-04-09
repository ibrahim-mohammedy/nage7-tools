# Download s3 bucket content
param (
	[string]$s3uri,
    [string]$s3resultFolder # assumes the folder will be placed at root of workspace
)

# Copy folder from s3 bucket
try{
    aws s3 cp $s3uri$s3resultFolder/ .\$s3resultFolder --recursive --quiet
    Write-Host "Completed: Copied s3 bucket [$s3uri$s3resultFolder/] into local folder [.\$s3resultFolder]"
} catch {
	Write-Host "Error: Unable copy s3 bucket [$s3uri$s3resultFolder/] into local folder [.\$s3resultFolder]"
    Write-Host $_.Exception.Message
    exit 1
}