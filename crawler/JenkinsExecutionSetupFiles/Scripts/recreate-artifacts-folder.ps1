# Create final solution artifact folder and the folder for the results
param (
	[string]$artifactFolderName, # assumes the folder will be placed at workspace root
    [string]$relativePathOfSolution, # assumes the folder will be placed at workspace root
    [string]$relativePathOfJenkinsSupportFiles, # assumes the folder will be placed at workspace root
	[string]$configGrpFolder, # assumes that the folder is placed in $relativePathOfSolution
	[string]$s3resultFolder, # Folder to house all results (adjacent to artifactFolderName) at workspace root
    [string]$testDataSharepointUrl, # If testDataSharepointUrl is provided then the following need to be provided as well: cipherDllPath, sharepointUsername, sharepointEncryptedPassword
    [string]$cipherDllPath,
    [string]$sharepointUsername,
    [string]$sharepointEncryptedPassword,
	[string]$testDataOneDriveUrl, # If testDataOneDriveUrl is provided then the following need to be provided as well: testDataFileName
	[string]$testDataFileName='TestData.xlsx' # default testData filename
)

# Function to download data from sharepoint
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SharePoint.Client") | Out-Null
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SharePoint.Client.Runtime") | Out-Null
Function Download-SharePointFile([string]$UserName, [string]$Password,[string]$FileUrl,[string]$DownloadPath)
 {
    if([string]::IsNullOrEmpty($Password)) {
      $SecurePassword = Read-Host -Prompt "Enter the password" -AsSecureString 
    }
    else {
      $SecurePassword = $Password | ConvertTo-SecureString -AsPlainText -Force
    }
    $fileName = [System.IO.Path]::GetFileName($FileUrl)
    $downloadFilePath = [System.IO.Path]::Combine($DownloadPath,$fileName)


    $client = New-Object System.Net.WebClient 
    $client.Credentials = New-Object Microsoft.SharePoint.Client.SharePointOnlineCredentials($UserName, $SecurePassword)
    $client.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f")
    $client.DownloadFile($FileUrl, $downloadFilePath)
    $client.Dispose()
}

# AWS Delivery Folder && Result Folder Cleanup
Remove-Item $artifactFolderName -Recurse -ErrorAction Ignore
Remove-Item "$artifactFolderName.zip" -Recurse -ErrorAction Ignore
mkdir $artifactFolderName|out-null
Write-Host "Completed: [$artifactFolderName] folder re-create"
Remove-Item $s3resultFolder -Recurse -ErrorAction Ignore
mkdir $s3resultFolder|out-null
Write-Host "Completed: [$s3resultFolder] folder re-create"

# Download TestDataFile and place it in the root of $artifactFolderName
if (!([string]::IsNullOrEmpty($testDataSharepointUrl))){
	# Copy from sharepoint
	Add-Type -Path $cipherDllPath
	Download-SharePointFile -UserName $sharepointUsername -Password $([Cipher.cipher]::DecryptString($sharepointEncryptedPassword)) -FileUrl $testDataSharepointUrl -DownloadPath .\$artifactFolderName\
	Write-Host "Completed: Copy of data file from Sharepoint: [$testDataSharepointUrl]"
} elseif (!([string]::IsNullOrEmpty($testDataOneDriveUrl))){
	# Copy from OneDrive
	Invoke-WebRequest -Uri $testDataOneDriveUrl -OutFile .\$artifactFolderName\$testDataFileName
	Write-Host "Completed: Copy of data file from OneDrive: [$testDataOneDriveUrl]"
} else {
	# Copy default testData file from git
	if(Test-Path "$relativePathOfSolution\$testDataFileName" -PathType Leaf){
		Copy-Item "$relativePathOfSolution\$testDataFileName" -Destination .\$artifactFolderName\
	} else {
		Write-Host "Error: Unable to find and copy default Test Data File from [$relativePathOfSolution\$testDataFileName]"
		exit 1
	}
}

# Specify all folders to copy here
$allSrcFolder = @("$relativePathOfSolution\bin\Debug",
"$relativePathOfSolution\DownloadedFiles",
"$relativePathOfSolution\UploadFiles",
"$relativePathOfJenkinsSupportFiles\Scripts",
"$relativePathOfSolution\$configGrpFolder")

# Copy Folder
foreach ($src in $allSrcFolder){
    $dest = ".\$artifactFolderName"+$src.TrimStart('.')
    Copy-Item $src $dest -Recurse
    if (Test-Path($dest)){
        Write-Host "Completed: Copy of [$dest] folder"
    } else{
        Write-Host "Error: Copy of [$src] to [$dest] folder failed"
		exit 1
    }
}

# Create Archive (archive created in workspace with the same name as localFolder)
try{
	Compress-Archive -Path .\$artifactFolderName\* -DestinationPath ".\$artifactFolderName.zip"
	Write-Host "Completed: Created zip file for artifacts [.\$artifactFolderName]"
}catch {
	Write-Host "Error: Unable to create zip file for artifacts  at [.\$artifactFolderName]"
    Write-Host $_.Exception.Message
    exit 1
}
