# Upload the contents to s3 bucket

param (
	[string]$s3uri,
    [string]$artifactFolderName # assumes the folder will be placed at workspace root
)

# Empty s3 bucket
try{
	aws s3 rm $s3uri --recursive --quiet
    Write-Host "Completed: Emptied s3 location [$s3uri]"
} catch {
	Write-Host "Error: Unable to empty s3 location [$s3uri]"
    Write-Host $_.Exception.Message
    exit 1
}

# Copy folder to s3 bucket
try{
	aws s3 cp .\$artifactFolderName.zip $s3uri --quiet
    Write-Host "Completed: Copied zip file [.\$artifactFolderName.zip] to s3 bucket [$s3uri]"
} catch {
	Write-Host "Error: Unable copy zip file [.\$artifactFolderName.zip] to s3 bucket [$s3uri]"
    Write-Host $_.Exception.Message
    exit 1
}