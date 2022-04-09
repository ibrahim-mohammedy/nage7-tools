using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SendResultEmail
{
    class Program
    {
        static void Main(string[] args)
        {
            // Getting the command line arguments
            string toEmailList = GetCMDParam(args, "toEmail");
            string resultFolderPath = GetCMDParam(args, "resultFolder");
            string configFile = GetCMDParam(args, "config");
            string buildUrl = GetCMDParam(args, "buildUrl");
            string buildArtifact = buildUrl + "artifact/" + resultFolderPath + "/";
            string emailSubject = GetCMDParam(args, "emailSubject");
            string buildInfo = resultFolderPath + "\\BuildInfo.txt";
            string resultFilePath = null;

            // Verify that results folder path exists
            if (!Directory.Exists(resultFolderPath))
            {
                throw new DirectoryNotFoundException("Invalid result folder path");
            }
            if (!File.Exists(resultFolderPath + @"\TestResult.html"))
            {
                Console.WriteLine($"WARNING: Result file [TestResult.html] not found in [{resultFolderPath}] path");
            }
            else
            {
                resultFilePath = new FileInfo(resultFolderPath + @"\TestResult.html").FullName;
            }

            // Get Data from buildinfo file
            Dictionary<string, string> mailContent = new Dictionary<string, string>
            {
                {"buildArtifacts", buildArtifact},
                {"config", configFile},
                {"numberOfNodes", GetBuildInfo(buildInfo, "NumberOfNodes")},
                {"browser", GetBuildInfo(buildInfo, "Browser")},
                {"env", GetBuildInfo(buildInfo, "Environment")},
                {"passed", GetBuildInfo(buildInfo, "PassedTests")},
                {"failed", GetBuildInfo(buildInfo, "FailedTests")},
                {"total", GetBuildInfo(buildInfo, "TotalTests")},
                {"totalDuration", GetBuildInfo(buildInfo, "CumulativeExecutionTime")},
                {"threads", GetBuildInfo(buildInfo, "Threads")}
            };

            // Send email
            bool mailSentFlag = false;
            try
            {
                Email.InvokeSendMail(emailSubject, resultFilePath, toEmailList, mailContent, resultFolderPath);
                mailSentFlag = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Sending Email Failed: " + e.Message + "\n" + e.StackTrace);
                if (e.InnerException != null)
                {
                    Console.WriteLine(e.InnerException.Message);
                    Console.WriteLine(e.InnerException.StackTrace);
                }
            }

            // Write to console success
            if (mailSentFlag)
            {
                Console.WriteLine("Email for results sent successfully!!");
            }
        }

        static string GetBuildInfo(string path, string param)
        {
            if (!new FileInfo(path).Exists) throw new Exception($"BuildInfo [{path}] file doesn't exist.");
            StreamReader streamReader = new StreamReader(path);
            string str = "";
            while (!streamReader.EndOfStream)
            {
                str += streamReader.ReadLine() + "§";
            }
            streamReader.Close();
            foreach (var line in str.Split('§'))
            {
                if (line.Contains(param))
                {
                    return line.Split('=').Last().Trim();
                }
            }
            throw new Exception($"Parameter [{param}] was not found in [{path}]");
        }

        static string GetCMDParam(IEnumerable<string> args, string name, bool mandatory = true)
        {
            foreach (var arg in args)
            {
                if (arg.ToLower().StartsWith(name.ToLower()))
                {
                    return arg.Split('=').Last();
                }
            }
            if (mandatory)
            {
                throw new ArgumentException($"Parameter [{name}] wasn't passed. Please check command line arguments.");
            }
            return null;
        }
    }
}