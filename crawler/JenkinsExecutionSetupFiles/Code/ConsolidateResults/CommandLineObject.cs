using ConsoleCommon;
using System;
using System.IO;

namespace ConsolidateResults
{
    class CommandLineObject : ParamsObject
    {
        public CommandLineObject(string[] args) : base(args)
        {

        }

        [Switch("resultFoldersPath", true)]
        public string resultFoldersPath { get; set; }

        [Switch("configGrpFolderPath", true)]
        public string configGrpFolderPath { get; set; }

        [Switch("consolidatedLogsFolder", true)]
        public string consolidatedLogsFolder { get; set; }

        [Switch("consolidatedScreenshotFolder", true)]
        public string consolidatedScreenshotFolder { get; set; }

        [Switch("environment", true)]
        public string environment { get; set; }

        [Switch("browser", true)]
        public string browser { get; set; }

        [Switch("threads", true)]
        public string threads { get; set; }

        [Switch("numberOfNodes", true)]
        public string numberOfNodes { get; set; }

        public string consolidatedLogsFolderPath => Path.GetFullPath(resultFoldersPath) + "\\" + consolidatedLogsFolder;
        public string consolidatedScreenshotFolderPath => Path.GetFullPath(resultFoldersPath) + "\\" + consolidatedScreenshotFolder;

        internal void CompleteParamsCheck()
        {
            CheckParams();
            // Validate resultFoldersPath is accessible
            if (!Directory.Exists(resultFoldersPath))
            {
                Console.WriteLine($"ERROR: Parameter/Path passed for resultFoldersPath [{resultFoldersPath}] does not exist");
                Environment.Exit(1);
            }
            // Validate configGrpFolderPath is accessible
            if (!Directory.Exists(configGrpFolderPath))
            {
                Console.WriteLine($"ERROR: Parameter/Path passed for configGrpFolderPath [{configGrpFolderPath}] does not exist");
                Environment.Exit(1);
            }
            // Initialize consolidatedLogsFolderPath
            if (!Directory.Exists(consolidatedLogsFolderPath))
                Directory.CreateDirectory(consolidatedLogsFolderPath);
            else
                DeleteAllFilesAndFolders(consolidatedLogsFolderPath);
            // Initialize consolidatedScreenshotFolderPath
            if (!Directory.Exists(consolidatedScreenshotFolderPath))
                Directory.CreateDirectory(consolidatedScreenshotFolderPath);
            else
                DeleteAllFilesAndFolders(consolidatedScreenshotFolderPath);
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
