# SendResultEmail

This is a basic console app which is used to update testrail *"Test Plan"* with results from an automation run (i.e. from TestResult.xml)

## Pre-requisite:
The tests specified in UIAutomation must have only one testrail test case id associated with each test. i.e. *TestOf* attribute is required on the test method

## **Commandline Parameters:** 
The following are the commandline paramaters that need to be passed in when calling this solution

 1. `resultFilePath`: The complete path for the NUnit result file (e.g. "c:\shared_nunit_results\Results\TestResult.xml")
 2. `testrailPlanName`: Exact name of the test rail plan. (e.g. "2019 Sept Release US Deploy Day Test")

## **TestRail Configuration:**
The configuration file *App.config* has all the relevant information for logging into test rail (see below example)
**Note:** all parameters in the below snippet are required

```
  <appSettings>
    <add key="TestRailURL" value="https://upland.testrail.com/"/>
    <add key="Username" value="sbanerjee@uplandsoftware.com"/>
    <add key="EncryptedPassword" value="AwrxJ/DtyLh42lRV/v449A=="/>
    <add key="ProjectId" value="22"/>
  </appSettings>
```