using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Configuration;
using System.Xml;

namespace SendResultEmail
{
    public enum TestCaseStatus { Failed = 0, Skipped = 1, Passed = 2 }

    public class TestCase
    {
        public TestCaseStatus Status { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public static class Email
    {
        public static void InvokeSendMail(string emailSubject, string attachmentPath, string mailToAddress, Dictionary<string, string> mailContent, string resultFolder)
        {
            mailContent["testresultsummary"] = BuildTestResultSummaryHTHML(resultFolder);

            using (MailMessage msg = new MailMessage())
            {
                msg.Sender = new MailAddress(ConfigurationManager.AppSettings["fromEmail"]);
                msg.From = msg.Sender;
                msg.To.Add(mailToAddress);

                msg.Subject = ApplySubstitutionTo(emailSubject, mailContent);

                msg.Priority = (int.Parse(mailContent["failed"]) == 0) ? MailPriority.Normal : MailPriority.High;

                msg.BodyEncoding = Encoding.ASCII;
                msg.IsBodyHtml = true;
                msg.Body = ApplySubstitutionTo(Resources.EmailBody, mailContent);

                //if (File.Exists(Path.Combine(resultFolder, "TestResult.html"))) msg.Attachments.Add(new Attachment(Path.Combine(resultFolder, "TestResult.html")));
                if (File.Exists(attachmentPath)) msg.Attachments.Add(new Attachment(attachmentPath));

                SendMail(msg);
            }
        }

        static string BuildTestResultSummaryHTHML(string resultFolder)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<table class='summary'>");
            foreach (TestCase tc in Summarize(Path.Combine(resultFolder, "TestResult.xml")))
            {
                sb.Append("<tr>");

                sb.Append($"<td>{tc.Name}</td>");
                sb.Append($"<td>{tc.Status}</td>");
                sb.Append($"<td>{tc.Description}</td> ");

                sb.Append("</tr>");
            }
            sb.Append("</table>");

            return sb.ToString();
        }

        private static string GetDateTimeFromResultPath(string attachmentPath)
        {
            string dtStr = attachmentPath.Substring(attachmentPath.LastIndexOf('.') - 9, 9);
            DateTime dt = DateTime.ParseExact(dtStr, "MMdd_HHmm", System.Globalization.CultureInfo.InvariantCulture);
            return dt.ToString();
        }

        public static void SendMail(MailMessage msg)
        {
            SmtpClient mClient = new SmtpClient();
            mClient.Host = ConfigurationManager.AppSettings["smtpHost"];
            mClient.Port = Convert.ToInt32(ConfigurationManager.AppSettings["smtpPort"]);
            mClient.Credentials = new System.Net.NetworkCredential(
                ConfigurationManager.AppSettings["smtpUsername"],
                Cipher.cipher.DecryptString(ConfigurationManager.AppSettings["smtpEncryptedPassword"])
                );
            mClient.EnableSsl = true;
            mClient.Send(msg);
        }

        private static List<TestCase> Summarize(string nunitResultFile)
        {
            List<TestCase> results = new List<TestCase>();

            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(nunitResultFile);

                foreach (XmlNode testCase in xd.SelectNodes("//test-case"))
                {
                    TestCase tc = new TestCase();

                    tc.Status = (TestCaseStatus)Enum.Parse(typeof(TestCaseStatus), testCase.Attributes["result"].Value);
                    tc.Name = (testCase.Attributes["name"] != null) ? testCase.Attributes["name"].Value : "UNKNOWN";
                    tc.Description = "";

                    if (tc.Status == TestCaseStatus.Failed)
                    {
                        XmlNode message = testCase.SelectSingleNode("failure/message");
                        XmlNode stacktrace = testCase.SelectSingleNode("failure/stack-trace");

                        if (message != null) tc.Description += message.InnerText;
                        if (stacktrace != null) tc.Description += $"<br>{stacktrace.InnerText}";
                    }
                    results.Add(tc);
                }

                results.Sort((x, y) => String.Compare(x.Name, y.Name, true));
                results.Sort((x, y) => ((int)x.Status).CompareTo((int)y.Status));
            }
            catch (Exception ex)
            {
            }

            return results;
        }

        private static bool AttributeIs(XmlNode testCase, string sAttribute, string sValue)
        {
            if (testCase.Attributes[sAttribute] == null) return false;

            return testCase.Attributes[sAttribute].Value == sValue;
        }

        public static string ApplySubstitutionTo(string value, Dictionary<string, string> vars)
        {
            try
            {
                List<string> targets = FindSubstitutionTargets(value);
                if (targets.Count == 0) return value;

                StringBuilder sb = new StringBuilder(value);

                foreach (string target in targets)
                {
                    string sanitized = target.Trim('%');
                    string replacement = vars.ContainsKey(sanitized) ? vars[sanitized] : $"**UNKNOWN {sanitized}**";

                    sb.Replace(target, replacement);
                }

                return sb.ToString();
            }
            catch
            {
                return $"**FAILED {value}**";
            }
        }

        private static List<string> FindSubstitutionTargets(string value)
        {
            List<string> targets = new List<string>();

            int last = 0;

            while (true)
            {
                int start = value.IndexOf('%', last);
                if (start == -1) break;

                int end = value.IndexOf('%', start + 1);
                if (end == -1) break;

                string target = value.Substring(start, end - start + 1);

                targets.Add(target);

                last = end + 1;
            }

            return targets;
        }

        public static string CreateHtmlBody(Dictionary<string, string> mailContent, string resultFolder)
        {
            return Resources.EmailBody;

            //// Initialize
            //StringWriter stringWriter = new StringWriter();
            //int sNo = 1;

            //using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            //{
            //    writer.AddStyleAttribute("font-size", "11pt");
            //    writer.RenderBeginTag(HtmlTextWriterTag.Body); // Begin #1

            //    writer.Write("Hello,");

            //    writer.RenderBeginTag(HtmlTextWriterTag.Br); // Begin #2
            //    writer.RenderEndTag(); // End #2

            //    writer.Write("Automated execution was run and following is a summary of the results:");

            //    writer.RenderBeginTag(HtmlTextWriterTag.Br); // Begin #3
            //    writer.RenderEndTag(); // End #3

            //    writer.RenderBeginTag(HtmlTextWriterTag.P); // Begin #3.1
            //    writer.RenderBeginTag(HtmlTextWriterTag.Font); // Begin #3.2
            //    writer.RenderBeginTag(HtmlTextWriterTag.B); // Begin #3.3
            //    writer.Write("Config File: ");
            //    writer.RenderEndTag(); // End #3.3
            //    writer.WriteLine(mailContent["config"]);
            //    writer.WriteBreak();
            //    writer.RenderBeginTag(HtmlTextWriterTag.B); // Begin #3.4
            //    writer.Write("Environment: ");
            //    writer.RenderEndTag(); // End #3.4
            //    writer.WriteLine(mailContent["env"]);
            //    writer.WriteBreak();
            //    writer.RenderBeginTag(HtmlTextWriterTag.B); // Begin #3.5
            //    writer.Write("Browser: ");
            //    writer.RenderEndTag(); // End #3.5
            //    writer.WriteLine(mailContent["browser"]);
            //    writer.RenderEndTag(); // End #3.2
            //    writer.RenderEndTag(); // End #3.1

            //    writer.RenderBeginTag(HtmlTextWriterTag.P); // Begin #3.1
            //    writer.RenderBeginTag(HtmlTextWriterTag.Font); // Begin #3.2
            //    writer.RenderBeginTag(HtmlTextWriterTag.B); // Begin #3.3
            //    writer.Write("Total Tests: ");
            //    writer.RenderEndTag(); // End #3.3
            //    writer.WriteLine(mailContent["total"]);
            //    writer.WriteBreak();
            //    writer.RenderBeginTag(HtmlTextWriterTag.B); // Begin #3.4
            //    writer.Write("Passed: ");
            //    writer.RenderEndTag(); // End #3.4
            //    writer.WriteLine(mailContent["passed"]);
            //    writer.WriteBreak();
            //    writer.RenderBeginTag(HtmlTextWriterTag.B); // Begin #3.5
            //    writer.Write("Failed: ");
            //    writer.RenderEndTag(); // End #3.5
            //    writer.WriteLine(mailContent["failed"]);
            //    writer.WriteBreak();
            //    writer.RenderBeginTag(HtmlTextWriterTag.B); // Begin #3.6
            //    writer.Write("Duration: ");
            //    writer.RenderEndTag(); // End #3.6
            //    writer.Write(mailContent["totalDuration"] + " (in HH:MM:ss)");

            //    if (!string.IsNullOrEmpty(mailContent["buildLog"]))
            //    {
            //        writer.WriteBreak();
            //        writer.RenderBeginTag(HtmlTextWriterTag.B); // Begin #3.7
            //        writer.Write("Build log: ");
            //        writer.RenderEndTag(); // End #3.7
            //        writer.RenderBeginTag(HtmlTextWriterTag.A); // Begin #3.8
            //        writer.AddAttribute(HtmlTextWriterAttribute.Href, mailContent["buildLog"]);
            //        writer.WriteLine(mailContent["buildLog"]);
            //        writer.RenderEndTag(); // End #3.8
            //    }

            //    if (mailContent.ContainsKey("NodesNumber") && mailContent.ContainsKey("NodesNames"))
            //    {
            //        writer.WriteBreak();
            //        writer.WriteBreak();
            //        writer.Write("Execution was run on ");
            //        writer.RenderBeginTag(HtmlTextWriterTag.B);
            //        writer.Write(mailContent["NodesNumber"]);
            //        writer.RenderEndTag(); // End #3.6
            //        writer.Write(" nodes : ");
            //        writer.RenderBeginTag(HtmlTextWriterTag.B);
            //        writer.Write(mailContent["NodesNames"]);
            //        writer.RenderEndTag(); // End #3.6
            //    }

            //    if (mailContent.ContainsKey("threads"))
            //    {
            //        writer.WriteBreak();
            //        writer.Write("Threads used on every single node: ");
            //        writer.RenderBeginTag(HtmlTextWriterTag.B);
            //        writer.Write(mailContent["threads"]);
            //        writer.RenderEndTag(); // End #3.6
            //    }

            //    writer.RenderEndTag(); // End #3.2
            //    writer.RenderEndTag(); // End #3.1

            //    //writer.WriteBreak();

            //    //writer.AddAttribute(HtmlTextWriterAttribute.Border, "1");
            //    //writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            //    //writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "3");
            //    //writer.RenderBeginTag(HtmlTextWriterTag.Table); // Begin #4

            //    //writer.AddStyleAttribute("style", "font - weight:bold");
            //    //writer.RenderBeginTag(HtmlTextWriterTag.Tr); // Begin #5
            //    //writer.RenderBeginTag(HtmlTextWriterTag.Th); // Begin #6
            //    //writer.Write("S. No.");
            //    //writer.RenderEndTag(); // End #6
            //    //writer.RenderBeginTag(HtmlTextWriterTag.Th); // Begin #7
            //    //writer.Write("Unit Test Name");
            //    //writer.RenderEndTag(); // End #7
            //    //writer.RenderBeginTag(HtmlTextWriterTag.Th); // Begin #7.1
            //    //writer.Write("Test Fixture Name");
            //    //writer.RenderEndTag(); // End #7.1
            //    //writer.RenderBeginTag(HtmlTextWriterTag.Th); // Begin #8
            //    //writer.Write("Status");
            //    //writer.RenderEndTag(); // End #8
            //    //writer.RenderBeginTag(HtmlTextWriterTag.Th); // Begin #9
            //    //writer.Write("Duration");
            //    //writer.RenderEndTag(); // End #9
            //    //writer.RenderBeginTag(HtmlTextWriterTag.Th); // Begin #10
            //    //writer.Write("Passed");
            //    //writer.RenderEndTag(); // End #10
            //    //writer.RenderBeginTag(HtmlTextWriterTag.Th); // Begin #11
            //    //writer.Write("Failed");
            //    //writer.RenderEndTag(); // End #11
            //    //writer.RenderEndTag(); // End #5

            //    //// Loop over some strings.
            //    //foreach (ResultRow result in results)
            //    //{
            //    //    writer.RenderBeginTag(HtmlTextWriterTag.Tr); // Begin #12
            //    //    writer.RenderBeginTag(HtmlTextWriterTag.Td); // Begin #13
            //    //    writer.Write(sNo.ToString());
            //    //    writer.RenderEndTag(); // End #13
            //    //    writer.RenderBeginTag(HtmlTextWriterTag.Td); // Begin #14
            //    //    writer.Write(result.unitTestDll);
            //    //    writer.RenderEndTag(); // End #14
            //    //    writer.RenderBeginTag(HtmlTextWriterTag.Td); // Begin #14.1
            //    //    writer.Write(result.testFixtureName);
            //    //    writer.RenderEndTag(); // End #14.1
            //    //    writer.RenderBeginTag(HtmlTextWriterTag.Td); // Begin #15
            //    //    writer.Write(result.status);
            //    //    writer.RenderEndTag(); // End #15
            //    //    writer.RenderBeginTag(HtmlTextWriterTag.Td); // Begin #16
            //    //    writer.Write(result.duration);
            //    //    writer.RenderEndTag(); // End #16
            //    //    writer.RenderBeginTag(HtmlTextWriterTag.Td); // Begin #17
            //    //    writer.Write(result.passed);
            //    //    writer.RenderEndTag(); // End #17
            //    //    writer.RenderBeginTag(HtmlTextWriterTag.Td); // Begin #18
            //    //    writer.Write(result.failed);
            //    //    writer.RenderEndTag(); // End #18
            //    //    writer.RenderEndTag(); // End #12
            //    //    sNo++;
            //    //}
            //    //writer.RenderEndTag(); // End #4

            //    //writer.RenderBeginTag(HtmlTextWriterTag.Br); // Begin #19
            //    //writer.RenderEndTag(); // End #19

            //    writer.WriteLine("Thanks,");
            //    writer.WriteBreak();
            //    writer.Write("Automation");

            //    writer.RenderEndTag(); // End #1
            //}
            //// Return the result.
            //return stringWriter.ToString();
        }
    }
}