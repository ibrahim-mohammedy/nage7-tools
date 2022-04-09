using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ConsolidateResults
{
    class Program
    {
        const string configGroupFoldersStartWith = "ExecutionGroup_";
        const string mappingReferenceFile = "GroupMappings.txt";

        static void Main(string[] args)
        {
            // Initialize
            CommandLineObject cl = new CommandLineObject(args);
            cl.CompleteParamsCheck();
            var configGrpFolders = Directory.GetDirectories(cl.resultFoldersPath, configGroupFoldersStartWith + "*");
            TestResults tr = new TestResults();

            #region copyFiles
            // Copy logMapping file
            string mappingFilePath = cl.configGrpFolderPath + "/" + mappingReferenceFile;
            if (!File.Exists(mappingFilePath))
                throw new FileNotFoundException($"ERROR: Mapping file [{mappingFilePath}] NOT found");
            else
                File.Copy(mappingFilePath, Path.Combine(cl.consolidatedLogsFolderPath, mappingReferenceFile));
            Console.WriteLine($"SUCCESS: Log Mapping Refrence file [{mappingReferenceFile}] is written to [{cl.consolidatedLogsFolderPath}]");

            // Iterate over each configGroupFolder and consolidate Logs and ScreenShots
            foreach (var folder in configGrpFolders)
            {
                // Get the TestResult.xml from the folder
                string resultXml = $"{folder}\\TestResult.xml";
                if (!new FileInfo(resultXml).Exists)
                {
                    throw new Exception($"Test results file [{resultXml}] doesn't exist. Was group run correctly?");
                }
                //Copy screenshots and Log files
                foreach (var sourceFilePath in Directory.GetFiles(folder))
                {
                    string targetFile = null;
                    string fileName = Path.GetFileName(sourceFilePath);
                    if (fileName.Equals("Log.log", StringComparison.InvariantCultureIgnoreCase)) // Rename and copy log files
                    {
                        targetFile = cl.consolidatedLogsFolderPath + "\\" + new DirectoryInfo(Path.GetDirectoryName(sourceFilePath)).Name + "_Log.log";
                    }
                    else if (!fileName.EndsWith(".xml")) // Skip if TestResult.xml
                    {
                        targetFile = cl.consolidatedScreenshotFolderPath + "\\" + fileName;
                    }
                    else
                    {
                        continue;
                    }
                    File.Copy(sourceFilePath, targetFile);
                    //Console.WriteLine($"File {fileName} copied to {cl.consolidatedResultFolderPath}");
                }
            }
            Console.WriteLine($"SUCCESS: Consolidated Log files are written to [{cl.consolidatedLogsFolderPath}]");
            Console.WriteLine($"SUCCESS: Consolidated Screenshots files are written to [{cl.consolidatedScreenshotFolderPath}]");
            #endregion

            // Write file overall result file
            #region generateResultXml
            string res = cl.resultFoldersPath + "\\TestResult.xml";
            XmlDocument combinedResults = new XmlDocument();
            XmlNode testRun = combinedResults.CreateElement("test-run");
            
            // Iterate over each configGroupFolder and consolidate TestResult XMLs
            foreach (var folder in configGrpFolders)
            {
                // Get the TestResult.xml from the folder
                string resultXml = $"{folder}\\TestResult.xml";
                if (!new FileInfo(resultXml).Exists)
                {
                    throw new Exception($"Test results file [{resultXml}] doesn't exist. Was group run correctly?");
                }
                XmlDocument xd = new XmlDocument();
                xd.Load(resultXml);
                foreach (XmlNode testSuite in xd.SelectNodes("//test-suite"))
                {
                    if (!testSuite.GetAttributeValue("type").Equals("TestFixture")) continue;
                    XmlNode duplicatedTestSuite = combinedResults.ImportNode(testSuite, false);
                    foreach (XmlNode testCase in testSuite.SelectNodes("test-case"))
                    {
                        if (testCase.GetAttributeValue("result").Equals("Skipped")) continue;
                        XmlNode duplicatedTestCase = combinedResults.ImportNode(testCase, true);
                        duplicatedTestSuite.AppendChild(duplicatedTestCase);
                        tr.TotalTestCases++;

                        if (testCase.GetAttributeValue("result") == "Failed") tr.TotalFailed++;
                        else tr.TotalPassed++;

                        tr.TotalDuration += Convert.ToDouble(testCase.GetAttributeValue("duration"));
                    }

                    if (duplicatedTestSuite.ChildNodes.Count > 0) testRun.AppendChild(duplicatedTestSuite);
                }
            }
            testRun.SetAttributeValue("testcasecount", tr.TotalTestCases.ToString());
            testRun.SetAttributeValue("total", tr.TotalTestCases.ToString());
            testRun.SetAttributeValue("passed", tr.TotalPassed.ToString());
            testRun.SetAttributeValue("failed", tr.TotalFailed.ToString());
            testRun.SetAttributeValue("duration", tr.TotalDuration.ToString());
            combinedResults.AppendChild(testRun);
            combinedResults.Save(res);
            Console.WriteLine($"SUCCESS: Consolidate results are written to [{res}] written");
            #endregion

            // Write BuildInfo file
            #region generateBuildInfoFile
            string detailsFile = $"{cl.resultFoldersPath}\\BuildInfo.txt";
            TimeSpan durationTime = TimeSpan.FromSeconds(tr.TotalDuration);
            
            FileInfo details = new FileInfo(detailsFile);
            StreamWriter write_text = details.AppendText();
            write_text.WriteLine($"CumulativeExecutionTime={durationTime.ToString(@"hh\:mm\:ss")}");
            write_text.WriteLine($"NumberOfNodes={cl.numberOfNodes}");
            write_text.WriteLine($"TotalTests={tr.TotalTestCases}");
            write_text.WriteLine($"PassedTests={tr.TotalPassed}");
            write_text.WriteLine($"FailedTests={tr.TotalFailed}");
            write_text.WriteLine($"Browser={cl.browser}");
            write_text.WriteLine($"Environment={cl.environment}");
            write_text.WriteLine($"Threads={cl.threads}");
            write_text.Close();
            Console.WriteLine($"SUCCESS: BuildIfo is written to [{detailsFile}]");
            #endregion
        }
    }
}
