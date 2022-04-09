using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UpdateResultsToDb
{
    class Program
    {
        // All the sql server connection information is stored in the config file
        // Main method accepts the following commandline parameter: ResultsFolder Path, build url (optional)
        // Note: the result folder is expected to contain TestResult.xml
        // The date time is entered as the current date time
        static void Main(string[] args)
        {
            // Define datetime entered in the databse
            DateTime dt = DateTime.Now;

            // Ensure all arguments are given
            if (args.Length < 1)
            {
                Console.WriteLine("Invalid command line parameter. Exiting WITHOUT updating database");
                return;
            }

            string buildUrl = args.Length == 2 ? args[1] : "";
            string resultFolderPath = args[0];
            string resultFile = resultFolderPath + @"\TestResult.xml";

            // Verify that results folder path exists
            if (!Directory.Exists(resultFolderPath))
            {
                Console.WriteLine("Invalid result folder path. Exiting WITHOUT updating database");
                return;
            }

            // Verify that results file exists
            if (!File.Exists(resultFile))
            {
                Console.WriteLine("TestResult.xml does not exist in :" + resultFolderPath);
                return;
            }

            //Dictionary<string, string[]> executionResults = new Dictionary<string, string[]>();
            XElement rootElem = XElement.Load(resultFile);
            // Get env and browser information
            XElement testPramDictionary = rootElem.Descendants("setting").Where(y => (string)y.Attribute("name").Value == "TestParametersDictionary").ToArray()[0];
            string env = testPramDictionary.Descendants("item").Where(y => (string)y.Attribute("key").Value == "environment").ToArray()[0].Attribute("value").Value;
            string browser = testPramDictionary.Descendants("item").Where(y => (string)y.Attribute("key").Value == "browser").ToArray()[0].Attribute("value").Value;
            // Go through each of the results
            IEnumerable<XElement> testNames = rootElem.Descendants("test-case");
            foreach (XElement x in testNames)
            {
                using (var db = new MetricsModel())
                {
                    var row = new RO_Results()
                    {
                        DateTimeStamp = dt,
                        BuildUrl = buildUrl,
                        Environment = env,
                        Browser = browser,
                        Persona = x.Attribute("classname").Value.Substring(x.Attribute("classname").Value.LastIndexOf('.') + 1),
                        TestName = x.Attribute("name").Value,
                        TestCaseId = x.Descendants("property").Where(y => (string)y.Attribute("name").Value == "TestOf").ToArray()[0].Attribute("value").Value,
                        TestDescription = x.Descendants("property").Where(y => (string)y.Attribute("name").Value == "Description").ToArray()[0].Attribute("value").Value,
                        ExecutionTime = Convert.ToDouble(x.Attribute("duration").Value),
                        Status = x.Attribute("result").Value,
                        Error = x.Descendants("failure").ToList().Count > 0 ? x.Descendants("message").ToArray()[0].Value : "",
                    };
                    db.RO_Results.Add(row);
                    db.SaveChanges();
                }
            }

            Console.WriteLine("Results Table successfully updated in the database !!");
        }
    }
}
