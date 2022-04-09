using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JenkinsNET;
using JenkinsNET.Models;
using JenkinsNET.Utilities;

namespace DistributedRunController
{
    class Program
    {

        static void Main(string[] args)
        {

            CommandLineObject cl = new CommandLineObject(args);
            cl.CompleteParameterCheck();

            // Get jenkins username and token
            string jenkinsUsername = ConfigurationManager.AppSettings["JenkinsUsername"];
            string jenkinsToken = ConfigurationManager.AppSettings["JenkinsToken"];

            List<string> hostnames = cl.instancesPrivateIPs.Split(',').ToList();

            // Instantiate new jenkins clients for all hostnames
            var clients = hostnames.Select(x => new JenkinsClient { BaseUrl = $"http://{x}:8080/", UserName = jenkinsUsername, ApiToken = jenkinsToken }).ToList();

            // DOWNLOAD JOB
            Console.WriteLine("Starting download jobs");
            List<Task> downloadJobTasks = new List<Task>();
            foreach (var client in clients)
            {
                downloadJobTasks.Add(Task.Factory.StartNew(() => { RunDownloadJob(client, cl.s3uri, cl.artifactFolderName); }));
            }

            Task.WaitAll(downloadJobTasks.ToArray());
            Console.WriteLine("Completed: All download jobs completed");

            // NODE JOBS
            // Get list of execution configs assuming that they are in the folder path \UIAutomation\UIAutomation\
            DirectoryInfo d = new DirectoryInfo(cl.configexecutionConfigFolderRelPath);
            ConcurrentQueue<string> excutionConfigQ = new ConcurrentQueue<string>(d.GetFiles("*.xml").Select(x => x.Name).ToList());

            // Start threads with all node jobs
            Console.WriteLine("Starting node jobs");
            List<Task> nodeJobTasks = new List<Task>();
            foreach (var client in clients)
            {
                Task t = Task.Factory.StartNew(() =>
                {
                    while (!excutionConfigQ.IsEmpty)
                    {
                        string executionConfigName;
                        int threadRetryCounter = 0;
                        while (!excutionConfigQ.TryDequeue(out executionConfigName) && threadRetryCounter++ < 10) { Thread.Sleep(10); } // Re-try 10 times to get value from excutionConfigQ
                        if (threadRetryCounter != 10)
                        {
                            var parameters = new Dictionary<string, string> {
                                {"browser", cl.browser},
                                {"environment", cl.environment},
                                {"executionConfigName", executionConfigName.Split('.')[0]},
                                {"executionConfigRelPath", cl.configGrpFolder + "/" + executionConfigName},
                                {"testDataFileName", cl.testDataFileName},
                                {"threads", cl.threads},
                                {"additionalData", cl.additionalData},
                                {"relativePathOfSolution", cl.relativePathOfSolution},
                                {"s3uri", cl.s3uri},
                                {"s3resultFolder", cl.s3resultFolder}
                            };
                            RunNodeJob(client, parameters);
                            Console.WriteLine($"# of Execution Configs Remaining => [{excutionConfigQ.Count}]");
                        }
                    }
                }
                );
                nodeJobTasks.Add(t);
            }

            Task.WaitAll(nodeJobTasks.ToArray());

            Console.WriteLine("Completed: All node jobs completed");
        }

        static void RunDownloadJob(JenkinsClient client, string s3uri, string artifactFolderName)
        {
            string downloadJobName = ConfigurationManager.AppSettings["DownloadJobName"];
            var runner = new JenkinsJobRunner(client);
            runner.QueueTimeout = 300;
            var buildResult = runner.RunWithParameters(downloadJobName, new Dictionary<string, string>() {
                { "s3uri", s3uri },
                { "artifactFolderName", artifactFolderName }
            });
            if (!string.Equals(buildResult?.Result, "SUCCESS"))
                throw new ApplicationException($"Failed: Download job [{downloadJobName}]  failed for: [{getBuildUrl(buildResult, client)}]");
            Console.WriteLine($"Completed: Build [{getBuildUrl(buildResult, client)}] completed successfully.");
        }

        static void RunNodeJob(JenkinsClient client, Dictionary<string, string> parameters)
        {
            string slaveJobName = ConfigurationManager.AppSettings["NodeJobName"];
            var runner = new JenkinsJobRunner(client)
            {
                QueueTimeout = 300,
                BuildTimeout = 1800 // 30 mins timeout
            };
            var buildResult = runner.RunWithParameters(slaveJobName, parameters);
            Console.WriteLine($"Node job [{getBuildUrl(buildResult, client)}] finished running ConfigGroup [{parameters["executionConfigName"]}] with status [{buildResult?.Result}]");
        }

        // Retrieves build url with ip addresses instead of localhost
        internal static string getBuildUrl(JenkinsBuildBase buildResult, JenkinsClient client)
        {
            return buildResult?.Url.Replace("http://localhost:8080/", client.BaseUrl);
        }
    }
}