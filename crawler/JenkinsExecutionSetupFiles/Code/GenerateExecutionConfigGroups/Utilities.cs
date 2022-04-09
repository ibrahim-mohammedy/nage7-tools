using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace GenerateExecutionConfigGroups
{
    class Utilities
    {
        //Reads execution config and creates list of tests distributed by groups 
        public static Dictionary<string, Test> GenerateTestGroups(string configPath)
        {
            if (!new FileInfo(configPath).Exists) throw new Exception("Config file doesn't exist");

            //Read execution config
            Dictionary<string, Test> testsList = new Dictionary<string, Test>();
            var config = XDocument.Load(configPath);
            var tests = config.Descendants("ExecutionList").Elements();
            foreach (var test in tests)
            {
                testsList.Add(test.Name.ToString(), new Test(test.Name.ToString(),
                    test.Attribute("dependentScripts")?.Value,
                    test.ToString()));
            }
            if (testsList.Count < 1) throw new Exception($"No tests were found in config [{configPath}]");

            //Group tests
            int groupN = 1;

            //Fill groups for tests with dependencies
            foreach (var test in testsList)
            {
                var t = test.Value;
                if (!string.IsNullOrEmpty(t.Dependency?.Trim()))
                {
                    foreach (var dependency in t.Dependency.Trim().Split(','))
                    {
                        if (!testsList.ContainsKey(dependency)) throw new Exception($"Parent script [{dependency}] not found in execution list.");
                        if (testsList[dependency].Group != null)
                        {
                            t.Group = testsList[dependency].Group;
                        }
                        else
                        {
                            testsList[dependency].Group = groupN.ToString();
                            t.SetGroup(groupN.ToString());
                            groupN++;
                        }
                    }
                }
            }

            //Fill groups for tests without dependencies
            foreach (var test in testsList)
            {
                var t = test.Value;
                if (t.Group == null)
                {
                    t.Group = groupN.ToString();
                    groupN++;
                }
            }
            Console.WriteLine($"Config parsed. Total {testsList.Count} found.");
            return testsList;
        }

        public static List<ExecutionGroup> UniteGroupsByThreadsCount(Dictionary<string, int> groupsTestsCounter, int THREADS_COUNT, Dictionary<string, Test> testsList, string xmlNameStartsWith)
        {
            List<ExecutionGroup> EXECUTION_GROUPS = new List<ExecutionGroup>();

            //Creating execution groups
            List<string> groupsAddedToExecutionGroups = new List<string>();
            int executionGroupN = 1;

            //Iterate until all groups added to execution groups
            while (groupsAddedToExecutionGroups.Count != groupsTestsCounter.Count)
            {
                string name = xmlNameStartsWith + executionGroupN.ToString("000");
                ExecutionGroup executionGroup = new ExecutionGroup(name);
                foreach (var group in groupsTestsCounter)
                {
                    if (groupsAddedToExecutionGroups.Contains(group.Key)) continue; //Skip group if it was added already

                    //If executiongroup doesn't contain any tests yet and current group contains more or equals tests than THREADS_COUNT - will create a separate execution group
                    if (executionGroup.Groups.Count == 0 && group.Value >= THREADS_COUNT)
                    {
                        executionGroup.Tests = MarkGroupWithExecutionGroup(group.Key, name, testsList);
                        executionGroup.Groups.Add(group.Key);
                        groupsAddedToExecutionGroups.Add(group.Key);
                        break;
                    }
                    if (executionGroup.Tests.Count + group.Value <= THREADS_COUNT)
                    {
                        foreach (var test in MarkGroupWithExecutionGroup(group.Key, name, testsList))
                        {
                            executionGroup.Tests.Add(test);
                        }
                        executionGroup.Groups.Add(group.Key);
                        groupsAddedToExecutionGroups.Add(group.Key);
                    }
                }
                EXECUTION_GROUPS.Add(executionGroup);
                executionGroupN++;
            }
            Console.WriteLine($"Groups united. Total {EXECUTION_GROUPS.Count} execution groups generated.");
            return EXECUTION_GROUPS;
        }

        //Creates list of tests by group name and marks tests with execution group name 
        private static List<Test> MarkGroupWithExecutionGroup(string groupName, string executionGroupName, Dictionary<string, Test> testsList)
        {
            List<Test> tests = new List<Test>();
            foreach (var test in testsList)
            {
                var t = test.Value;
                if (t.Group.Equals(groupName))
                {
                    tests.Add(t);
                    t.ExecutionGroup = executionGroupName;
                }
            }
            return tests;
        }

        public static void WriteExecutionConfigs(string executionConfigPath, List<ExecutionGroup> executionGroups, string configGrpFolder)
        {
            string path = new FileInfo(executionConfigPath).Directory.FullName;
            foreach (var executionGroup in executionGroups)
            {
                FileInfo newConfig = new FileInfo(path + "\\" + configGrpFolder + "\\" + executionGroup.Name + ".xml");
                StreamWriter write_text;
                write_text = newConfig.AppendText();
                StreamReader streamReader = new StreamReader(executionConfigPath);
                while (!streamReader.EndOfStream)
                {
                    string str = streamReader.ReadLine();
                    if (str.Contains("<ExecutionList>"))
                    {
                        write_text.WriteLine(str);
                        break;
                    }
                    write_text.WriteLine(str);
                }
                foreach (var test in executionGroup.Tests)
                {
                    write_text.WriteLine(test.ConfigLine);
                }
                write_text.WriteLine("</ExecutionList>");
                write_text.WriteLine("</UIAutomation>");
                streamReader.Close();
                write_text.Close();
            }
            Console.WriteLine($"Completed: [{MethodBase.GetCurrentMethod().DeclaringType}]: Configs written");
        }

        public static void WriteMappingReferenceFile(List<ExecutionGroup> executionGroups, string configGrpFolderPath, string mappingReferenceFile)
        {
            FileInfo mappingFile = new FileInfo(configGrpFolderPath + "\\" + mappingReferenceFile);
            StreamWriter write_text;
            write_text = mappingFile.AppendText();
            foreach (var executionGroup in executionGroups)
            {
                string line = executionGroup.Name + "=>";
                foreach(var test in executionGroup.Tests)
                {
                    line += test.Name + ";";
                }
                line = line.TrimEnd(';');
                write_text.WriteLine(line);
            }
            write_text.Close();
            Console.WriteLine($"Completed: [{MethodBase.GetCurrentMethod().DeclaringType}]: Mapping File written [{mappingFile.FullName}]");
        }

        public static void RemoveExistingConfigXMLs(string configPath, string folder)
        {
            string path = new FileInfo(configPath).Directory.FullName;
            string folderPath = path + "\\" + folder;
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
                Console.WriteLine($"[{MethodBase.GetCurrentMethod().DeclaringType}] [pre-run-cleanup]: removed directory [{folderPath}]");
            }
            Directory.CreateDirectory(folderPath);
            Console.WriteLine($"[{MethodBase.GetCurrentMethod().DeclaringType}] [pre-run-setup]: created directory [{folderPath}]");
        }
    }
}
