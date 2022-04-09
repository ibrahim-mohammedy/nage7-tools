using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nge7_Helper
{
    public class Utilities
    {
        public static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                if (dirPath.EndsWith(@"data\e-books")) continue;
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            foreach (string file in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                if (file.Contains(@"\data\e-books")) continue;

                if (file.Contains(@"\data\media\images\english"))
                {
                    string targetFolder = Path.GetDirectoryName(file.Replace(sourcePath, targetPath));
                    if (Directory.GetFiles(targetFolder).Length >= 1) continue;
                }

                File.Copy(file, file.Replace(sourcePath, targetPath), true);
            }
        }
    }
}