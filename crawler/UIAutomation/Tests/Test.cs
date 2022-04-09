using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using UIAutomation.Utils_Misc;
using UIAutomation.Utils_Selenium;
using Environment = UIAutomation.Utils_Misc.Environment;
using UIAutomation.API;
using log4net;
using System.Security.Cryptography;
using System.Text;

namespace UIAutomation.Tests
{
    public class SynchronizeAttribute : PropertyAttribute
    {
        public const string IntegratedApplicationFileBound = "IntegratedApplicationFileBound";
        public const string IntegratedApplicationInterFAX = "IntegratedApplicationInterFAX";

        public SynchronizeAttribute(string method) : base("Synchronize", method)
        {
        }
    }

    [NonParallelizable]
    public class Test
    {
        public string TenantAdminEmail => Data.Get("config:username");
        public string TenantAdminPassword => Data.Get("config:password");

        public string TestingEmailDomain => "automated-testing.intelligentcapture.com";

        public string TestingUserName => SanitizedTestName;
        public string TestingUserEmail => $"{SanitizedTestName}@{TestingEmailDomain}".ToLower();
        public string TestingUserPassword = "password123";

        public string TestingRoleName => SanitizedTestName;

        public string TestingMetaDataName => SanitizedTestName;

        public string TestingDataSourceName => SanitizedTestName;

        public string TestingImporterName => SanitizedTestName;

        public string TestingIntegratedApplicationName => SanitizedTestName;

        public string SanitizedTestName
        {
            get
            {
                int index = TestContext.CurrentContext.Test.Name.IndexOf("(");
                if (index == -1) return TestContext.CurrentContext.Test.Name;

                return TestContext.CurrentContext.Test.Name.Substring(0, index);
            }
        }

        public string HashedTestName
        {
            get
            {
                return HashString(SanitizedTestName);
            }
        }

        public static string HashString(string text)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            byte[] bytes = Encoding.UTF8.GetBytes(text);

            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);

            char[] hash2 = new char[16];

            // Note that here we are wasting bits of hash!
            // But it isn't really important, because hash.Length == 32
            for (int i = 0; i < hash2.Length; i++)
            {
                hash2[i] = chars[hash[i] % chars.Length];
            }

            return new string(hash2);
        }

        readonly object lockThis = new object();

        API.RESTAPI _api;

        protected API.RESTAPI IC
        {
            get
            {
                lock (lockThis)
                {
                    if (_api == null)
                    {
                        _api = new API.RESTAPI(Data.Get("config:url"));
                        _api.Authenticate(TenantAdminEmail, TenantAdminPassword);
                    }

                    return _api;
                }
            }
        }

        API.Setup _setup;

        protected API.Setup Setup
        {
            get
            {
                lock (lockThis)
                {
                    if (_setup == null) _setup = new API.Setup(IC, this);

                    return _setup;
                }
            }
        }

        Dictionary<string, API.IFX> _ifxes = new Dictionary<string, IFX>();

        protected API.IFX IFX
        {
            get
            {
                lock (_ifxes)
                {
                    if (!_ifxes.ContainsKey(SanitizedTestName))
                    {
                        _ifxes[SanitizedTestName] = new API.IFX(this);
                    }

                    return _ifxes[SanitizedTestName];
                }
            }
        }

        [TearDown]
        public void ShutDown()
        {
            var testResult = TestContext.CurrentContext.Result.Outcome;
            TestExecution.addTestResult(TestContext.CurrentContext.Test.Name, testResult);

            CleanUp();

            if (testResult == ResultState.Error || testResult == ResultState.Failure) CreateScreenshot();
            if (DriverOperations.Get() != null) DriverOperations.Get().Close();
        }

        private void CreateScreenshot()
        {
            if (!TestExecutionContext.CurrentContext.CurrentTest.Properties.ContainsKey("driver")) return;

            string screensLocation = Environment.RootPath + @"ResultFolders\Results\";
            var screenshot = ((ITakesScreenshot)DriverOperations.GetIWebDriver()).GetScreenshot();
            screenshot.SaveAsFile(screensLocation + TestContext.CurrentContext.Test.Name + Data.dateTimeStamp + ".png");
        }

        public void AutomaticCleanup(IDocument d)
        {
            if (d == null) return;

            lock (AutomaticCleanupItems)
            {
                AutomaticCleanupItems[SanitizedTestName].Add(d);
            }
        }

        Dictionary<string, List<IDocument>> AutomaticCleanupItems { get; } = new Dictionary<string, List<IDocument>>();

        public Type[] CleanupOrder = { typeof(User), typeof(MetaData), typeof(Role), typeof(Importer), typeof(Workflow) };

        protected virtual void CleanUp()
        {
            if (!TestExecutionContext.CurrentContext.CurrentTest.Properties.ContainsKey("TestData")) return;

            foreach (Type t in CleanupOrder)
            {
                if (t == typeof(User))
                {
                    Setup.CleanupDefaultTestingUser();
                    Cleanup(typeof(User));
                }

                if (t == typeof(MetaData))
                {
                    Setup.CleanupDefaultTestingMetaData();
                    Cleanup(typeof(MetaData));
                }

                if (t == typeof(Role))
                {
                    Setup.CleanupDefaultTestingRole();
                    Cleanup(typeof(Role));
                }

                if (t == typeof(Importer))
                {
                    Setup.CleanupDefaultTestingImporter();
                    Cleanup(typeof(Importer));
                }

                if (t == typeof(Workflow))
                {
                    Setup.CleanupDefaultTestingWorkflow();
                    Cleanup(typeof(Workflow));
                }
            }

            if (string.IsNullOrEmpty(Data.Get("config:cleanup")))
            {
                return;
            }

            Console.WriteLine("Cleanup:");
            foreach (var cleanUp in Data.Get("config:cleanup").Split(','))
            {
                var cleanUpParams = cleanUp.Split('=');
                var cleanupMethod = cleanUpParams.First().Trim();
                var param = cleanUpParams.Length > 1 ? cleanUpParams.Last().Trim() : null;
                try
                {
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        void Cleanup(Type t)
        {
            IEnumerable<IDocument> items = null;

            lock (AutomaticCleanupItems)
            {
                items = AutomaticCleanupItems[SanitizedTestName].Where(w => w.GetType() == t);
            }

            foreach (IDocument d in items)
            {
                IC.Cleanup(d);
            }
        }

        private Dictionary<string, string> PrepareTestData(Dictionary<string, string> envParams, Dictionary<string, string> rawTestData, Dictionary<string, string> testParams)
        {
            var testData = new Dictionary<string, string>();
            foreach (var data in rawTestData)
            {
                testData.Add(data.Key.ToLower(), data.Value.ToString());
            }
            testData.Add("config:env", Environment.Env);
            testData.Add("config:browser", Environment.Browser);
            testData.Add("config:fileuploadpath", Environment.RootPath + @"UploadFiles\");
            testData.Add("config:filedownloadpath", Environment.RootPath + @"DownloadedFiles\");
            testData.Add("config:resultpath", Environment.RootPath + @"Results");
            // add all envParams
            foreach (KeyValuePair<string, string> item in envParams)
            {
                testData.Add(item.Key, item.Value ?? "");
            }
            testData.Add("config:url", envParams["config_" + testParams["ui"].ToLower() + ":url"]);
            testData.Add("config:username", envParams["config_" + testParams["persona"].ToLower() + ":username"]);
            testData.Add("config:password", envParams["config_" + testParams["persona"].ToLower() + ":password"]);
            testData.Add("config:testname", testParams["testName"]);
            testData.Add("config:cleanup", testParams["cleanup"]);
            return testData;
        }

        private string WaitForeDependencies(string dependencies)
        {
            if (string.IsNullOrEmpty(dependencies)) return "";

            foreach (var test in dependencies.Split(','))
            {
                Console.WriteLine($"Wait for dependency [{test}] finished.");

                int counter = 0;
                while (TestExecution.FINISHED == null || !TestExecution.FINISHED.Contains(test.Trim()))
                {
                    Thread.Sleep(500);

                    if (counter++ > 60 * 5 * 2) throw new Exception($"WaitForDependency '{dependencies}' exceeded time limit");
                }

                Console.WriteLine($"Dependency [{test}] finished.");
                if (TestExecution.FAILED.Contains(test.Trim())) return "Dependent script failure: [" + test.Trim() + "]";
                if (TestExecution.SKIPPED.Contains(test.Trim())) return "Dependent script skipped: [" + test.Trim() + "]";
            }

            return "";
        }

        private Dictionary<string, string> ReadConfig(string configFile, string testName)
        {
            var config = XDocument.Load(configFile);
            var test = config.Descendants(testName);

            if (test.Count() > 1)
            {
                throw new Exception("More than 1 test with name [" + testName + "] declared in config file.");
            }
            if (!test.Any())
            {
                return null;
            }

            Dictionary<string, string> testParams = new Dictionary<string, string>
            {
                {"ui", test.First().Attribute("ui") == null ? "" : test.First().Attribute("ui").Value},
                {"persona", test.First().Attribute("persona").Value},
                {"dependentScripts", test.First().Attribute("dependentScripts")?.Value},
                {"testName", testName},
                {"cleanup", test.First().Attribute("cleanup") == null ? "" : test.First().Attribute("cleanup").Value}
            };

            return testParams;
        }

        private Dictionary<string, string> ReadTestData(string testDataFile, string testName, string persona, string environment)
        {
            ExlParser excel = new ExlParser(testDataFile);
            return excel.LoadDataDictionay(testName, persona, environment);
        }

        protected void WaitForFile(string filename)
        {
            int retry = 30 * 4; //30s

            while (retry-- > 0)
            {
                try
                {
                    using (System.IO.File.Open(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None))
                    {
                        return;
                    }
                }
                catch
                {
                }

                if (System.IO.File.Exists(filename)) return;

                Thread.Sleep(250);
            }

            throw new Exception($"Failed to wait for {filename}");
        }

        protected void CompareFiles(string expectedContent, string actualContentPath)
        {
            if (System.IO.File.ReadAllBytes(expectedContent).SequenceEqual(System.IO.File.ReadAllBytes(actualContentPath))) return;

            throw new Exception("File contents differ");
        }
    }
}