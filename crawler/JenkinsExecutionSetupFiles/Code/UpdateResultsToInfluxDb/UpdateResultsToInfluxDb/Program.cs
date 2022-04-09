using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace UpdateResultsToInfluxDb
{
    class Program
    {
        // Lists for soapui
        static List<TestCaseDetails> tcDetailList = new List<TestCaseDetails>();
        static List<ProjectDetails> projDetailList = new List<ProjectDetails>();
        // Lists for junit
        static List<JUnitTCDetails> jTCDetailslList = new List<JUnitTCDetails>();
        static List<JUnitSummaryDetails> jSummaryDetailsList = new List<JUnitSummaryDetails>();
        // Lists for nunit
        static List<NUnitTCDetails> nTCDetailslList = new List<NUnitTCDetails>();
        static List<NUnitSummaryDetails> nSummaryDetailsList = new List<NUnitSummaryDetails>();
        // Lists for jmeter
        static List<JMeterResultRow> jResultRowList = new List<JMeterResultRow>();

        // This program accepts 2 mandatory parameter and an optionl parameters to add additional attribute to each datapoint
        // Assumes that only jmeter-compact (used from production_jmeter) will have 2 files passed in as arguments
        // Check CommandLineObjects.cs for details
        static void Main(string[] args)
        {
            CommandLineObject cl = new CommandLineObject(args);
            try
            {
                cl.PerformCompleteParameterCheck();
            }catch(Exception e)
            {
                Console.WriteLine($"ERROR: UpdateResultsToInfluxDb: Results NOT updated to InfluxDB: {e.Message}");
                return;
            }
            try
            {
                InfluxDb.Initialize(cl);
                switch (cl.reportType)
                {
                    case "soapuilog":
                        UpdateSoapUILists(cl.file);
                        // Write to influx db
                        InfluxDb.WriteInternalTestCasesSummaryToInfluxDb(tcDetailList);
                        InfluxDb.WriteInternalProjectsSummaryToInfluxDb(projDetailList);
                        break;
                    case "junit":
                        UpdateJUnitLists(cl.file);
                        InfluxDb.WriteJUnitTestCasesSummaryToInfluxDb(jTCDetailslList);
                        InfluxDb.WriteJUnitTestSuitesSummaryToInfluxDb(jSummaryDetailsList);
                        break;
                    case "nunit":
                        UpdateNUnitLists(cl.file);
                        InfluxDb.WriteNUnitTestCasesSummaryToInfluxDb(nTCDetailslList, cl.additionalRowAttributes);
                        InfluxDb.WriteNUnitTestSuitesSummaryToInfluxDb(nSummaryDetailsList, cl.additionalRowAttributes);
                        break;
                    case "jmeter":
                        UpdateJMeterLists(cl.file);
                        InfluxDb.WriteJMeterResultsToInfluxDb(jResultRowList, cl.additionalRowAttributes);
                        break;
                    case "jmeter-compact":
                        UpdateJMeterLists(cl.file.Split(';')[0]);
                        UpdateJMeterListsBuildTimings(cl.file.Split(';')[1]);
                        InfluxDb.WriteJMeterCompactResultsToInfluxDb(jResultRowList, cl.additionalRowAttributes);
                        break;
                    default:
                        // Do nothing because user will not be able to enter anything other than junit, soapuilog, nunit.
                        // Values spefified in CommandLineObjects.cs
                        break;
                };
            } catch (Exception e)
            {
                Console.WriteLine($"UpdateResultsToInfluxDb encountered exception when running for [{cl.reportType}][{cl.file}]:");
                Console.WriteLine(e.Message);
                if (InfluxDb.client != null)
                    InfluxDb.client.Dispose();
                return;
            }
            Console.WriteLine($"UpdateResultsToInfluxDb ran to completion for [{cl.reportType}][{cl.file}]");
        }

        // SOAPUI: updates values to the 2 lists: tcDetailList && projDetailList
        private static void UpdateSoapUILists(string file)
        {
            // Initialize
            string projectName = "";
            //Read the line
            try
            {   // Open the text file using a stream reader.
                StreamReader sr = new StreamReader(file);
                string line;
                // Loop through each line 
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    // Get details for the TestCaseDetailsSummaryMeasurements
                    if (line.Contains("Running tests in the project"))
                    {
                        projectName = line.Split('[')[2].Split(']')[0];
                    }
                    else if (line.Contains("Finished running TestCase"))
                    {
                        tcDetailList.Add(CreateTestCaseDetailObject(line, projectName));
                    }
                    // Get details for the TestCaseDetailsSummaryMeasurements
                    else if (line.Contains("TestCaseRunner Summary"))
                    {
                        projDetailList.Add(CreateProjectDetailObject(ref sr, projectName));
                    }
                }
                sr.Close();
            }
            catch (Exception e)
            {
                Debug.Write("Issue with file operation or string operations: " + e.Message);
            }
        }

        // SOAPUI: performing string operations to get the details for ProjectDetails object
        private static ProjectDetails CreateProjectDetailObject(ref StreamReader sr, string projectName)
        {
            int duration;
            int testSuites;
            int totalTestCases;
            int failedTestCases;
            int testSteps;
            int totalAssertions;
            int failedAssertions;
            string line;
            // Fast forward 2 lines
            line = sr.ReadLine();
            line = sr.ReadLine();
            // Start getting details
            duration = Int32.Parse(line.Split(':')[1].Trim().Substring(0, line.Split(':')[1].Trim().Length - 2));
            line = sr.ReadLine(); // move to next line
            testSuites = Int32.Parse(line.Split(':')[1].Trim());
            line = sr.ReadLine(); // move to next line
            totalTestCases = Int32.Parse(line.Split(':')[1].Split('(')[0].Trim());
            failedTestCases = Int32.Parse(line.Split('(')[1].Split('f')[0].Trim());
            line = sr.ReadLine(); // move to next line
            testSteps = Int32.Parse(line.Split(':')[1].Trim());
            line = sr.ReadLine(); // move to next line
            totalAssertions = Int32.Parse(line.Split(':')[1].Trim());
            line = sr.ReadLine(); // move to next line
            failedAssertions = Int32.Parse(line.Split(':')[1].Trim());
            //Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", projectName, duration, testSuites, totalTestCases, failedTestCases, testSteps, totalAssertions, failedAssertions); // To be removed
            return new ProjectDetails(projectName, duration, testSuites, totalTestCases, failedTestCases, testSteps, totalAssertions, failedAssertions);
        }

        // SOAPUI: performing string operations to get the details for TestCaseDetails object
        private static TestCaseDetails CreateTestCaseDetailObject(string line, string projectName)
        {
            string testCase;
            string status;
            string timeStr;
            int duration;
            testCase = line.Split('[')[2].Split(']')[0];
            timeStr = line.Split(new string[] { "time taken:" }, StringSplitOptions.None)[1].Split(',')[0].Trim();
            duration = Int32.Parse(timeStr.Substring(0, timeStr.Length - 2));
            status = line.Split(new string[] { "status:" }, StringSplitOptions.None)[1].Trim();
            //Console.WriteLine("{0} , {1}, {2}, {3}", projectName, testCase, status, duration); // To be removed
            return new TestCaseDetails(projectName, testCase, status, duration);
        }

        // JUnit: 
        private static void UpdateJUnitLists(string file)
        {
            var xml = XDocument.Load(file);
            // update jSummaryDetailsList
            var testSuites = xml.Descendants("testsuite");
            foreach (var testSuite in testSuites)
            {
                jSummaryDetailsList.Add(new JUnitSummaryDetails(testSuite));
            }
            // update jTCDetailslList
            foreach (var testSuite in testSuites)
            {
                var testCases = testSuite.Descendants("testcase");
                foreach (var testCase in testCases)
                {
                    jTCDetailslList.Add(new JUnitTCDetails((string)testSuite.Attribute("name"), testCase));
                }
            }
        }

        // Nunit: 
        private static void UpdateNUnitLists(string file)
        {
            var xml = XDocument.Load(file);
            // update nSummaryDetailsList
            var testSuites = xml.Descendants("test-suite").Where(x => x.Attribute("type").Value.Equals("TestFixture"));
            foreach (var testSuite in testSuites)
            {
                nSummaryDetailsList.Add(new NUnitSummaryDetails(testSuite));
            }
            // update nTCDetailslList
            foreach (var testSuite in testSuites)
            {
                var testCases = testSuite.Descendants("test-case");
                foreach (var testCase in testCases)
                {
                    nTCDetailslList.Add(new NUnitTCDetails((string)testSuite.Attribute("name").Value, testCase));
                }
            }
        }

        // JMeter: 
        // Pass in only one file (*.csv)
        private static void UpdateJMeterLists(string file)
        {
            //string file = files.Split(';')[0];
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (!(line.Contains("Label,# Samples") || line.StartsWith("TOTAL")))
                    {
                        jResultRowList.Add(new JMeterResultRow(line));
                    }
                }
                reader.Close();
            }
        }

        // JMeter
        // Pass in only one file (buildTimeList.txt)
        private static void UpdateJMeterListsBuildTimings(string file)
        {
            //string file = files.Split(';')[0];
            List<int> buildTimes = new List<int>();
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    buildTimes.Add(Convert.ToInt32(line.Split(',')[2]));
                }
                reader.Close();
            }
            int avg = Convert.ToInt32(buildTimes.Average());
            int median = GetMedian(buildTimes);
            int count = buildTimes.Count();
            jResultRowList.Add(new JMeterResultRow
            {
                sampler_label = "Build Start-Finished",
                aggregate_report_count = count,
                average = avg,
                aggregate_report_median = median
            });
        }

        static int GetMedian(IEnumerable<int> source)
        {
            // Create a copy of the input, and sort the copy
            int[] temp = source.ToArray();
            Array.Sort(temp);
            int count = temp.Length;
            if (count == 0)
            {
                throw new InvalidOperationException("Empty collection");
            }
            else if (count % 2 == 0)
            {
                // count is even, average two middle elements
                int a = temp[count / 2 - 1];
                int b = temp[count / 2];
                return Convert.ToInt32((a + b) / 2m);
            }
            else
            {
                // count is odd, return the middle element
                return temp[count / 2];
            }
        }
    }
}
