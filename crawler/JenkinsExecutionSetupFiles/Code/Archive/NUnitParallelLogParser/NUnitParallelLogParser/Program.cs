using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NUnitParallelLogParser
{
    class Program
    {
        static Dictionary<string, string> logs;

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                throw new Exception("Two input parameters expected: in and out file pathes");
            }

            if (!new FileInfo(args[0]).Exists)
            {
                throw new Exception($"Input file [{args[0]}] doesn't exist.");
            }

            logs = new Dictionary<string, string>();
            string[] lines = File.ReadAllLines(args[0]);

            bool logsStarted = false;
            string testNameInProgress = "";
            foreach (var line in lines)
            {
                if (line.Contains("=> UIAutomation"))
                {
                    logsStarted = true;
                    testNameInProgress = line.Split('.').Last();
                    if (!logs.ContainsKey(testNameInProgress))
                    {
                        logs.Add(testNameInProgress, "");
                    }
                    continue;
                }
                if (!logsStarted)
                {
                    continue;
                }
                if (line.Trim().Length == 0)
                {
                    break;
                }
                string oldValue = logs[testNameInProgress];
                logs[testNameInProgress] = oldValue.Trim().Length == 0 ? line : oldValue + "\n" + line;
            }

            string output = "";

            if (logs.Keys.Count == 0)
            {
                throw new Exception("Nothing was parsed. Something went wrong?");
            }

            FileInfo file = new FileInfo(args[1]);
            if (file.Exists)
            {
                file.Delete();
            }

            StreamWriter write_text;
            write_text = file.AppendText();

            foreach (var z in logs)
            {
                write_text.WriteLine($"------------------- STARTING AUTOMATED TEST [{z.Key}] -------------------");
                write_text.WriteLine(z.Value);
                write_text.WriteLine($"------------------- FINISHED AUTOMATED TEST [{z.Key}] -------------------");
                write_text.WriteLine();
            }
            write_text.Close();
        }
    }
}
