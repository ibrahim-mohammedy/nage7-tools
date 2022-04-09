using ConsoleCommon;
using System;
using System.IO;

namespace UpdateResultsToInfluxDb
{
    class CommandLineObject : ParamsObject
    {
        public CommandLineObject(string[] args) : base(args)
        {

        }

        [ConsoleCommon.Switch("TYPE", true, 1, switchValues: new string[] { "junit", "soapuilog", "nunit", "jmeter", "jmeter-compact" })]
        public string reportType { get; set; }

        // Assumes that only jmeter-compact will have 2 files
        [ConsoleCommon.Switch("FILE", true, 2)]
        public string file { get; set; }

        // Format: key1:value1;key2:value2....
        [ConsoleCommon.Switch("ADDITIONALATTRIBUTES", defaultOrdinal: 3)]
        public string additionalRowAttributes { get; set; }

        public void PerformCompleteParameterCheck()
        {
            CheckParams();
            string[] files = file.Split(';');
            foreach(var f in files)
            {
                if (!File.Exists(f))
                {
                    throw new FileNotFoundException($"File/FilePath [{f}] does not exist! Current working directory is [{Directory.GetCurrentDirectory()}]");
                }
            }
        }
    }
}
