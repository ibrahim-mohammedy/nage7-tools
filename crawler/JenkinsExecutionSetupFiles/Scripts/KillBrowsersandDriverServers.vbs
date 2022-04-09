'Kill Everything

'Get Computer name
Set wshShell = CreateObject( "WScript.Shell" )
strComputerName = wshShell.ExpandEnvironmentStrings( "%COMPUTERNAME%" )

'Set WMI Service
Dim objWMIService
Set objWMIService = GetObject("winmgmts:" & "{impersonationLevel=impersonate}!\\" & strComputerName & "\root\cimv2")

'Terminate DriverServers
Dim colProcessIEList, colProcessChromeList
Set colProcessIEList = objWMIService.ExecQuery("SELECT * FROM Win32_Process WHERE Name = 'IEDriverServer.exe'")
Set colProcessChromeList = objWMIService.ExecQuery("SELECT * FROM Win32_Process WHERE Name = 'ChromeDriver.exe'")
Set colProcessFirefoxList = objWMIService.ExecQuery("SELECT * FROM Win32_Process WHERE Name = 'geckodriver.exe'")
For Each objProcess in colProcessIEList
  objProcess.Terminate() 
Next 
For Each objProcess in colProcessChromeList
  objProcess.Terminate() 
Next 
For Each objProcess in colProcessFirefoxList
  objProcess.Terminate() 
Next 

'Terminate Browser
Dim colProcessIE, colProcessChrome
Set colProcessIE = objWMIService.ExecQuery("SELECT * FROM Win32_Process WHERE Name = 'iexplore.exe'")
Set colProcessChrome = objWMIService.ExecQuery("SELECT * FROM Win32_Process WHERE Name = 'chrome.exe'")
Set colProcessFirefox = objWMIService.ExecQuery("SELECT * FROM Win32_Process WHERE Name = 'firefox.exe'")
Do While colProcessIE.Count <> 0
	For Each objProcess in colProcessIE
	  On Error Resume Next
	  objProcess.Terminate() 
	  Exit For
	Next
	WScript.Sleep 500
	Set colProcessIE = objWMIService.ExecQuery("SELECT * FROM Win32_Process WHERE Name = 'iexplore.exe'")
Loop
Do While colProcessChrome.Count <> 0
	For Each objProcess in colProcessChrome
	  On Error Resume Next
	  objProcess.Terminate() 
	  Exit For
	Next 
	WScript.Sleep 500
	Set colProcessChrome = objWMIService.ExecQuery("SELECT * FROM Win32_Process WHERE Name = 'chrome.exe'")
Loop
Do While colProcessFirefox.Count <> 0
	For Each objProcess in colProcessFirefox
	  On Error Resume Next
	  objProcess.Terminate() 
	  Exit For
	Next 
	WScript.Sleep 500
	Set colProcessFirefox = objWMIService.ExecQuery("SELECT * FROM Win32_Process WHERE Name = 'firefox.exe'")
Loop