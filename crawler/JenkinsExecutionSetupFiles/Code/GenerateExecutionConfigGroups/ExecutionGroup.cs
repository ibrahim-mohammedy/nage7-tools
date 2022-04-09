using System.Collections.Generic;

namespace GenerateExecutionConfigGroups
{
    public class ExecutionGroup
    {
        public string Name { get; set; }
        public List<Test> Tests { get; set; }
        public List<string> Groups { get; set; }
        public bool Separate { get; set; }

        public ExecutionGroup(string name)
        {
            Name = name;
            Tests = new List<Test>();
            Groups = new List<string>();
            Separate = false;
        }
    }
}
