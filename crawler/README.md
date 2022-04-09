# UI Automation

This is a NUnit Selemium faramework designed for UI testing leveraging webservice calls for quicker script execution times.

TestData is maintained in Excel File
Environment Information and automation script meta data is maintained in different ExecutionConfig xml files.

Tests are to be grouped into personas to form test suites. Additionally, tests are run based on the type of test (i.e. regression, acceptance, smoke)

The framework is ideally targeted to run on multiple VMs and run multiple threads on each VM.

## UI Automation Key Configuration & Output

* ```App.Config```: Set the following:
	* *Browser* :Chrome, Firefox, IE 
	* *Environment*: Pick from the environment values available in the ExecutionConfig. This must match a column in the TestData as well
	* *ExecutionConfig*: Pick the xml file name the script you want to run is categorized as.
	* *TestData*: If only the file name is mentioned, the solution looks for the file in the root folder (UIAutoamtion). If an absolute path is mentioned, it looks up the value accordingly
* ```AssemblyInfo.cs```: Set the number of threads you want to run by updating the LevelOfParallelism assembly parameter
* ```ExecutionConfig```: SmokeTest.xml, RegressionTest.xml, etc. are called as ExecutionConfig and define which scripts run when the entire solution is run
	* *EnvironmentInformation*: Enter the different environments as sub tags to this section and enter information such as `url`, `API`(external webservices url and key), `LoginInfo` (username and password for each persona)
	* *ExecutionList*: List all the scripts that pertain to that test type in this as sub tags. Additional attributes include:
		* *cleanup*: which cleanup method to run after running a script. Multiple methods can be mentioned separated with a comma. If any cleanup method requires specific data, enter the same as `cleanup="<method>=<dataParameter as mentioned in TestData>"`
		* *ui* (optional): specifies which url to use from the EnvironmentInformation section
		* *persona*: specifies which Test Suite persona to use.  Depending on the value in this attribute the framework will look for the test in "relevant cs file in the Tests folder" (e.g. Admin.cs if attribute is admin) and the "relevant sheet in the TestData excel file"
		* *dependentScripts* (optional): if any script is specified in this attribute. The script will not run as long as the script mentioned in this attribute is run and passes
* ```TestData.xlsx```: The workbook is devided into different sheets based on personas. Each script needs to have a entry in this file (even if no data is used from this file). All data parameters can be accessed by `Data.Get("<paramterName>")` from within the script
	* *column A*: script name
	* *column B*: parameter name
	* *column C,D,etc*: data value for different environments
* ```Results```: Results are stored in the ~\ResultFolders\Results. The "Results" folder is re-created every run and contains:
	* *log file*
	* *screenshots with date timestamp appended*


## Adding a new script

This framework is based on a page object model with page factory initialization. i.e. for a new script that automates a new page the user has to create the following:

* ```PageObject```: A new page object class file which contains all the objects for a particular page
	* The class must inherit from `BasePage`
	* The class name must end with a prefix Page as standard coding practice
	* All objects are defined as `WebElement` which implements `IWebElement`
	* All objects are defined as an expression-bodied member (i.e. `=>`) which evaluates the objects only when being called.
	* Elements can be defined using `driver` (`CustomDriver`) which is an implementation of `IWebDriver`
	* Object names must begin with the 1st 3 letters of the dom element as standard coding practice
* ```Page```: A new page class file which contains all the methods related to a particular page
	* The class must inherit from `Base`
	* Initialize a static page object variable in `Base` like so: `protected static LoginPage loginPage => GetPageObject("LoginPage");`
	* Using the above initialized page object, create methods for performing the different actions. Keep the functions as modular as possible to increase re-use.
	* All methods will be `static` and most `void`
	* As a standard practice please ensure `LogStart()` and `LogEnd()` are appropriately used
* ```Tests```: Tests are categorized into test suites based on persona. If the particular test needs to fall in a new persona create a class file for the same.
	* The class must inherit from `BaseTest`	
	* Add a `public static void` method for the new test with the following attributes `[Test, Parallelizable, TestOf("<TestRailTestCase#>"), Description("<TestDescription>")]`
	* Start adding in calls from the methods created in `Page`
* ```ExecutionConfig```: Add a new xml tag under `ExecutionList` with the **same name** as the method created in `Test`
* ```TestData.xlsx```: Add a section with the **same name** as the method in `Tests`, in the same sheet as the name of the class in `Tests`
	* Add in the parameters to the tests in the same section
	* Data in these parameters can be accessed in the code using `Data.Get("<paramterName>")`
	