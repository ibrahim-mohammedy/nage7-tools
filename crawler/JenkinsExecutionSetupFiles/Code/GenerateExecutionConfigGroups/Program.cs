using System.Collections.Generic;

namespace GenerateExecutionConfigGroups
{
    class Program
    {
        const string xmlNameStartsWith = "ExecutionGroup_";
        const string mappingReferenceFile = "GroupMappings.txt";
        static void Main(string[] args)
        {
            CommandLineObject cl = new CommandLineObject(args);
            cl.CompleteParamsCheck();

            Dictionary<string, Test> testsList = Utilities.GenerateTestGroups(cl.configPath);
            Dictionary<string, int> groupsTestsCounter = new Dictionary<string, int>();

            //Counting number of tests by each group. Will be used for generating execution groups by THREAD_NUMBER
            foreach (var test in testsList)
            {
                var t = test.Value;
                if (!groupsTestsCounter.ContainsKey(t.Group))
                {
                    groupsTestsCounter.Add(t.Group, 1);
                }
                else
                {
                    groupsTestsCounter[t.Group] = groupsTestsCounter[t.Group] + 1;
                }
            }

            var EXECUTION_GROUPS = Utilities.UniteGroupsByThreadsCount(groupsTestsCounter, int.Parse(cl.threads), testsList, xmlNameStartsWith);
            //Utilities.RemoveExistingConfigXMLs(cl.configPath, cl.configGrpFolder);
            Utilities.WriteExecutionConfigs(cl.configPath, EXECUTION_GROUPS, cl.configGrpFolder); //Write configs
            Utilities.WriteMappingReferenceFile(EXECUTION_GROUPS, cl.configGrpFolderPath, mappingReferenceFile);
        }



        

        

        
    }
}
