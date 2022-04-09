# SendResultEmail

This is a basic console app which is used for creating a mail with a summary of information (which it reads from the "*BuildInfo.txt*" ) and adds an attachment with the a compressed zip file for the results.

## **Commandline Parameters:** 
The following are the commandline paramaters that need to be passed in when calling this solution

 1. `toEmail`: The list of recipients email separated by a comma "," (e.g. toEmail="sbanerjee@uplandsoftware.com,twiseman@uplandsoftware.com")
 2. `share`: This is the root folder for the shared path where all the results are stored. (e.g. share=c:\shared_nunit_results)
 3. `config`: This is the name of the config file used for a specific execution run. (e.g. config=RegressionTest)
 4. `buildLog`: The url for console log for the master job build. (e.g. buildLog=http://ord1qapp08qv.qvidiancorp.com:8080/job/NUnitDistributedRun_master/888/consoleFull)
 5. `emailSubject`: Subject of the email. (e.g. emailSubject="RO Automation Regression Test Run")

## **SMTP Configuration:**
The configuration file *App.config* allows you to set some basic smptp configuration (see below example)

```
  <appSettings>
    <add key="smtpHost" value="smtp.upland.local"/>
    <add key="smtpPort" value="25"/>
    <add key="fromEmail" value="qvautomation@uplandsoftware.com"/>
  </appSettings>
```