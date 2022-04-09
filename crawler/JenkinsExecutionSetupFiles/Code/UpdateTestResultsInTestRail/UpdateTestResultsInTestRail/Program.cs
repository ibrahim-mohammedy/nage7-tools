using Gurock.TestRail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Configuration;
using Cipher;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.Threading;

namespace UpdateTestResultsInTestRail
{
    class Program
    {
        // This program requires 2 arguments in the following order: result xml, planName
        static void Main(string[] args)
        {
            // Ensure all arguments are given
            if (args.Length == 1)
            {
                TerminateApplication("No Plan Name Specified. Abort updating results in TestRail.");
            }
            if (args.Length != 2)
            {
                TerminateApplication("Invalid command line paramaters");
            }

            string resultsFile = args[0];
            string planName = args[1].Trim();

            if (String.IsNullOrEmpty(planName) || String.IsNullOrWhiteSpace(planName))
            {
                TerminateApplication("No Plan Name Specified. Abort updating results in TestRail.");
            }

            #region getInformationFromXML
            // Get the result file
            if (!File.Exists(resultsFile))
            {
                TerminateApplication("Invalid result file path: " + resultsFile);
            }
            Dictionary<string, string[]> executionResults = new Dictionary<string, string[]>();
            XElement rootElem = XElement.Load(resultsFile);
            IEnumerable<XElement> testNames = rootElem.Descendants("test-case");
            foreach (XElement x in testNames)
            {
                string name = x.Attribute("name").Value;
                string id = x.Descendants("property").Where(y => (string)y.Attribute("name").Value == "TestOf").ToArray()[0].Attribute("value").Value;
                string result = x.Attribute("result").Value;
                executionResults.Add(name, new string[] { id, result });
            }
            #endregion getInformationFromXML

            #region TestRailConnectMapping
            List<string> testsNotUpdated = new List<string>();
            List<string> testsWithoutIds = new List<string>();
            // Initialize API Information
            APIClient client = new APIClient(ConfigurationManager.AppSettings["TestRailURL"]);
            client.User = ConfigurationManager.AppSettings["Username"];
            client.Password = cipher.DecryptString(ConfigurationManager.AppSettings["EncryptedPassword"]);
            // Initialize variables
            string projectId = ConfigurationManager.AppSettings["ProjectId"];
            string planId = "";
            Dictionary<string, string> suiteIdToRunId = new Dictionary<string, string>();
            List<ResultRow> resultList = new List<ResultRow>();
            // Get Test Plan Id from name
            try
            {
                // get_plans will return a paginated response with pageSize = 250 items
                // We assumes that we will never have more than 250 incomplete test runs in the project. Hence, we search only on the first page
                JObject plansObj = (JObject)client.SendGet("get_plans/" + projectId + "&is_completed=0");
                JArray plansArr = (JArray)plansObj["plans"];
                foreach (JObject o in plansArr)
                {
                    if (String.Equals(o["name"].ToString(), planName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        planId = o["id"].ToString();
                        break;
                    }
                }
                if (planId == "")
                {
                    TerminateApplication("Plan name not found.Tests NOT Updated");
                }
            }
            catch (APIException)
            {
                TerminateApplication("Error in communicating with test rail. Check URL, username, password and project id");
            }
            // Get Suite Id and Run Id mapping form the plan
            JObject planObj = (JObject)client.SendGet("get_plan/" + planId);
            IEnumerable<JToken> runsArr = planObj.SelectTokens("$..runs");
            foreach (JToken jt in runsArr)
            {
                suiteIdToRunId.Add(jt.SelectToken("$..suite_id").ToString(), jt.SelectToken("$..id").ToString());
            }
            //Form ResultRow
            foreach (KeyValuePair<string, string[]> kv in executionResults)
            {
                string testName = kv.Key;
                string testResult = kv.Value[1];
                string testCaseId = kv.Value[0] ==""? "": kv.Value[0].Replace("C","");
                try
                {
                    // Get suite Id for test case
                    if (testCaseId != "")
                    {
                        Thread.Sleep(250); // hard coded wait without which the get request fails on multiple subsequent requests
                        JObject caseObj = (JObject)client.SendGet("get_case/" + testCaseId);
                        string testSuiteId = caseObj["suite_id"].ToString();
                        string testRunId = suiteIdToRunId[testSuiteId];
                        resultList.Add(new ResultRow(testName, testCaseId, testSuiteId, testRunId, testResult));
                    }
                    else
                    {
                        testsWithoutIds.Add(testName);
                    }
                }
                catch (Exception e)
                {
                    if (e is APIException || e is KeyNotFoundException)
                    {
                        testsNotUpdated.Add(testName + $" (C{testCaseId})");
                    }
                    else
                    {
                        throw e;
                    }
                }
            }

            // Display test that have not been updated
            Console.WriteLine("****Test cases not found OR not present in the test plan:");
            foreach (string s in testsNotUpdated)
            {
                Console.WriteLine(s);
            }
            // Display test that have not been updated
            Console.WriteLine("****Test cases without test case Ids:");
            foreach (string s in testsWithoutIds)
            {
                Console.WriteLine(s);
            }
            #endregion TestRailConnectMapping

            #region writeToTestRail
            // Write result row in testrail
            testsNotUpdated = null;
            testsNotUpdated = new List<string>();
            Console.WriteLine("****Tests updated in testrail: ");
            foreach (ResultRow r in resultList)
            {
                try
                {
                    Thread.Sleep(250); // hard coded wait without which the get request fails on multiple subsequent requests
                    client.SendPost("add_result_for_case/" + r.runId + "/" + r.caseId, createPostJson(r));
                    Console.WriteLine(r.scriptName + " (C" + r.caseId + ")");
                }
                catch (APIException)
                {
                    testsNotUpdated.Add(r.scriptName);
                }
            }
            Console.WriteLine("****Test cases to which updates failed:");
            foreach (string s in testsNotUpdated)
            {
                Console.WriteLine(s);
            }
            #endregion writeToTestRail

            // Closing message
            Console.WriteLine("****UpdateTestinTestRail Completed!!!****");
        }

        public static void TerminateApplication(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
            Environment.Exit(0);
        }

        // For reporting result
        public static JObject createPostJson(ResultRow r)
        {
            int status = r.resultStatus == "Passed" ? 1 : 4;
            string comment = "Run via automation";
            JObject obj = new JObject(
                new JProperty("status_id", status),
                new JProperty("comment", comment)
                );
            return obj;
        }
    }
}
