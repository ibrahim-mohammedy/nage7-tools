# Upload the contents to s3 bucket

param (
	[string]$s3uri,
    [string]$s3resultFolder, # assumes the folder will be at root of $s3uri
    [string]$resultFolder, # default folder where results are stored
    [string]$executionConfigName # name of the folder in which results will be uploaded
)

# Check if resultsfolder is empty
# No need to check if directory exists because the preceednig steps should create the folder
$resultFiles = (Get-ChildItem -Path $resultFolder -Recurse | Measure-Object).Count
if ($resultFiles -eq 0){
    Write-Host "ERROR: No files exist in results folder [.\$resultFolder]"
    exit 1
}

# Copy folder to s3 bucket
try{
	aws s3 cp "$($resultFolder)/" "$($s3uri)$($s3resultFolder)/$($executionConfigName)/" --recursive
    Write-Host "Completed: Copied files from [$resultFolder] to s3 [$s3uri/$s3resultFolder/$executionConfigName]"
} catch {
	Write-Host "Error: Files NOT copied from [$resultFolder] to s3 [$s3uri/$s3resultFolder/$executionConfigName]"
    Write-Host $_.Exception.Message
    exit 1
}