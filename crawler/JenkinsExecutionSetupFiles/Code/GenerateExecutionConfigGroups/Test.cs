namespace GenerateExecutionConfigGroups
{
    public class Test
    {
        public string Name { get; }
        public string Dependency { get; }
        public string ConfigLine { get; }
        public string Group { get; set; }
        public string ExecutionGroup { get; set; }

        public Test(string name, string dependency, string line)
        {
            Dependency = dependency;
            Name = name;
            ConfigLine = line;
            Group = null;
            ExecutionGroup = null;
        }

        public void SetGroup(string group)
        {
            Group = group;
        }
    }
}
