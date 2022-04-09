using ConsoleCommon;
using System;
using System.IO;

namespace GenerateExecutionConfigGroups
{
    class CommandLineObject : ParamsObject
    {
        public CommandLineObject(string[] args) : base(args)
        {

        }

        [Switch("configPath", true)]
        public string configPath { get; set; }

        [Switch("threads", true)]
        public string threads { get; set; }

        [Switch("configGrpFolder", true)]
        public string configGrpFolder { get; set; }

        public string configGrpFolderPath => new FileInfo(configPath).Directory.FullName + "\\" + configGrpFolder;

        internal void CompleteParamsCheck()
        {
            CheckParams();
            // Validate configPath
            if (!File.Exists(configPath))
            {
                Console.WriteLine($"Error: Parameter passed for full config path [{configPath}] does not exist");
                Environment.Exit(1);
            }
            // Initialize configGrpFolder
            if (!Directory.Exists(configGrpFolderPath))
            {
                Directory.CreateDirectory(configGrpFolderPath);
            }
            else
            {
                DeleteAllFilesAndFolders(configGrpFolderPath);
            }
        }

        internal void DeleteAllFilesAndFolders(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }
    }
}
