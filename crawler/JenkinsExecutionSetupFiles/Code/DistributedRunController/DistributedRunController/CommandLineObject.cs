using ConsoleCommon;
using System.IO;

namespace DistributedRunController
{
    class CommandLineObject : ParamsObject
    {
        public CommandLineObject(string[] args) : base(args)
        {

        }

        [Switch("s3uri")]
        public string s3uri { get; set; }

        [Switch("artifactFolderName")]
        public string artifactFolderName { get; set; }

        [Switch("s3resultFolder")]
        public string s3resultFolder { get; set; }

        [Switch("relativePathOfSolution")]
        public string relativePathOfSolution { get; set; }

        [Switch("configGrpFolder")]
        public string configGrpFolder { get; set; }

        [Switch("browser")]
        public string browser { get; set; }

        [Switch("environment")]
        public string environment { get; set; }

        [Switch("threads")]
        public string threads { get; set; }

        [Switch("additionalData")]
        public string additionalData { get; set; }

        [Switch("testDataFileName")]
        public string testDataFileName { get; set; }

        [Switch("instancesPrivateIPs")]
        public string instancesPrivateIPs { get; set; }

        public string configexecutionConfigFolderRelPath => relativePathOfSolution + configGrpFolder + "/";

        public void CompleteParameterCheck()
        {
            CheckParams();
            if (!new DirectoryInfo(configexecutionConfigFolderRelPath).Exists)
            {
                throw new DirectoryNotFoundException($"Failed: unable to find config files directory in master job [{configexecutionConfigFolderRelPath}]");
            }
        }
    }
}
