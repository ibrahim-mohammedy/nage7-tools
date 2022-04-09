# Download s3 bucket content
param (
	[string]$s3uri,
    [string]$artifactFolderName # assumes the folder will be placed at root of $s3uri
)

# Copy folder from s3 bucket
try{
    aws s3 cp $s3uri$artifactFolderName.zip .\
    Write-Host "Completed: Copied s3 bucket [$s3uri$artifactFolderName.zip] into local folder [.\]"
} catch {
	Write-Host "Error: Unable copy s3 bucket [$s3uri$artifactFolderName.zip] into local folder [.\]"
    Write-Host $_.Exception.Message
    exit 1
}

# Unzip file
if(Test-Path ".\$artifactFolderName.zip" -PathType Leaf){
	Expand-Archive -Path ".\$artifactFolderName.zip" -DestinationPath .\	
    Write-Host "Completed: Extracting [$artifactFolderName.zip] into local folder [.\]"
} else {
	Write-Host "Error: Unable to find downloaded zip file [$artifactFolderName] in workspace root"
    exit 1
}

# Delete zip file
Remove-Item "$artifactFolderName.zip" -ErrorAction Ignore
Write-Host "Completed: Cleanup: Deleting [$artifactFolderName.zip] into local folder [.\]"
