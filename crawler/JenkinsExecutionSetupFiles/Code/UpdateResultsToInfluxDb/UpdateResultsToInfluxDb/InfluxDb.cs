using AdysTech.InfluxDB.Client.Net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.Linq;

namespace UpdateResultsToInfluxDb
{
    class InfluxDb
    {
        // initialize
        public static string connection;
        public static InfluxDBClient client;
        static string username;
        static string encryptedPassword;
        static string dbNameSoapUI;
        static string dbNameJUnit;
        static string dbNameNUnit;
        static string dbNameJMeter;
        static string dbNameJMeterCompact;
        static DateTime reporttime;

        public static void Initialize(CommandLineObject cl)
        {
            // Get config values from App.config
            connection = ConfigurationManager.AppSettings["InfluxDbConnection"];
            if (String.IsNullOrEmpty(connection))
            {
                throw new Exception($"Connection string to InfluxDb [InfluxDbConnection] not found in config file!");
            }
            username = ConfigurationManager.AppSettings["Username"];
            encryptedPassword = ConfigurationManager.AppSettings["EncryptedPassword"];
            if (String.IsNullOrEmpty(ConfigurationManager.AppSettings[$"DatabaseName_{cl.reportType}"]))
                throw new Exception($"Database entry for [{cl.reportType}] not found in config file!");
            dbNameSoapUI = ConfigurationManager.AppSettings["DatabaseName_soapuilog"];
            dbNameJUnit = ConfigurationManager.AppSettings["DatabaseName_junit"];
            dbNameNUnit = ConfigurationManager.AppSettings["DatabaseName_nunit"];
            dbNameJMeter = ConfigurationManager.AppSettings["DatabaseName_jmeter"];
            dbNameJMeterCompact = ConfigurationManager.AppSettings["DatabaseName_jmeter-compact"];
            reporttime = DateTime.UtcNow;
            // Define client
            if (String.IsNullOrEmpty(username))
                client = new InfluxDBClient(connection);
            else
                client = new InfluxDBClient(connection, username, Cipher.cipher.DecryptString(encryptedPassword));
            // Check connectivity
            CheckConnection();
        }

        public static void WriteInternalTestCasesSummaryToInfluxDb(List<TestCaseDetails> results)
        {
            // add data to TestFixtureSummary
            foreach (TestCaseDetails row in results)
            {
                var val = new InfluxDatapoint<InfluxValueField>();
                val.MeasurementName = "InternalTestCasesSummary";
                val.UtcTimestamp = reporttime;
                val.Precision = TimePrecision.Seconds;

                val.Tags.Add("ProjectName", row.projName);
                val.Tags.Add("TestCaseName", row.testCaseName);
                val.Tags.Add("Status", row.status);
                val.Fields.Add("Duration", new InfluxValueField(row.duration));

                // Write to database 
                Task.Run(async () =>
                {
                    var r = await client.PostPointAsync(dbNameSoapUI, val);
                }).GetAwaiter().GetResult();
            }
        }

        internal static void WriteJMeterResultsToInfluxDb(List<JMeterResultRow> jResultRow, string additionalAttributes)
        {
            foreach(var row in jResultRow)
            {
                var val = new InfluxDatapoint<InfluxValueField>();
                val.MeasurementName = "AggregateReport";
                val.UtcTimestamp = reporttime;
                val.Precision = TimePrecision.Minutes;

                val.Tags.Add("sampler_label", row.sampler_label);
                val.Fields.Add("aggregate_report_count", new InfluxValueField(row.aggregate_report_count));
                val.Fields.Add("average", new InfluxValueField(row.average));
                val.Fields.Add("aggregate_report_median", new InfluxValueField(row.aggregate_report_median));
                val.Fields.Add("aggregate_report_90%_line", new InfluxValueField(row.aggregate_report_90_line));
                val.Fields.Add("aggregate_report_95%_line", new InfluxValueField(row.aggregate_report_95_line));
                val.Fields.Add("aggregate_report_99%_line", new InfluxValueField(row.aggregate_report_99_line));
                val.Fields.Add("aggregate_report_min", new InfluxValueField(row.aggregate_report_min));
                val.Fields.Add("aggregate_report_max", new InfluxValueField(row.aggregate_report_max));
                val.Fields.Add("aggregate_report_error%", new InfluxValueField(row.aggregate_report_error));
                val.Fields.Add("aggregate_report_rate", new InfluxValueField(row.aggregate_report_rate));
                val.Fields.Add("aggregate_report_bandwidth", new InfluxValueField(row.aggregate_report_bandwidth));
                val.Fields.Add("aggregate_report_stddev", new InfluxValueField(row.aggregate_report_stddev));

                AddAdditionalTags(ref val, additionalAttributes, excludeAttributes: "rampup_time");
                AddAdditionalFields(ref val, additionalAttributes, includeAttributes: "rampup_time:int");

                // Write to database
                Task.Run(async () =>
                {
                    var r = await client.PostPointAsync(dbNameJMeter, val);
                }).GetAwaiter().GetResult();
            }
        }

        internal static void WriteJMeterCompactResultsToInfluxDb(List<JMeterResultRow> jResultRow, string additionalAttributes)
        {
            foreach (var row in jResultRow)
            {
                var val = new InfluxDatapoint<InfluxValueField>();
                val.MeasurementName = "AggregateReport";
                val.UtcTimestamp = reporttime;
                val.Precision = TimePrecision.Minutes;

                val.Tags.Add("sampler_label", row.sampler_label);
                val.Fields.Add("aggregate_report_count", new InfluxValueField(row.aggregate_report_count));
                val.Fields.Add("average", new InfluxValueField(row.average));
                val.Fields.Add("median", new InfluxValueField(row.aggregate_report_median));

                AddAdditionalTags(ref val, additionalAttributes, excludeAttributes: "rampup_time");

                // Write to database
                Task.Run(async () =>
                {
                    var r = await client.PostPointAsync(dbNameJMeterCompact, val);
                }).GetAwaiter().GetResult();
            }
        }

        public static void WriteInternalProjectsSummaryToInfluxDb(List<ProjectDetails> results)
        {
            // add data to TestFixtureSummary
            foreach (ProjectDetails row in results)
            {
                var val = new InfluxDatapoint<InfluxValueField>();
                val.MeasurementName = "InternalProjectsSummary";
                val.UtcTimestamp = reporttime;
                val.Precision = TimePrecision.Seconds;

                val.Tags.Add("ProjectName", row.projName);
                val.Tags.Add("Status", row.status);
                val.Fields.Add("NumberOfTestSuits", new InfluxValueField(row.testSuites));
                val.Fields.Add("NumberOfPassedTestCases", new InfluxValueField(row.passedTestCases));
                val.Fields.Add("NumberOfFailedCases", new InfluxValueField(row.failedTestCases));
                val.Fields.Add("TotalTestSteps", new InfluxValueField(row.testSteps));
                val.Fields.Add("NumberOfPassedAssertions", new InfluxValueField(row.passedAssertions));
                val.Fields.Add("NumberOfFailedAssertions", new InfluxValueField(row.failedAssertions));
                val.Fields.Add("Duration", new InfluxValueField(row.duration));

                // Write to database 
                Task.Run(async () =>
                {
                    var r = await client.PostPointAsync(dbNameSoapUI, val);
                }).GetAwaiter().GetResult();
            }
        }

        public static void WriteJUnitTestCasesSummaryToInfluxDb(List<JUnitTCDetails> results)
        {
            // add data to TestCasesSummary
            foreach (var row in results)
            {
                var val = new InfluxDatapoint<InfluxValueField>();
                val.MeasurementName = "TestCasesSummary";
                val.UtcTimestamp = reporttime;
                val.Precision = TimePrecision.Seconds;

                val.Tags.Add("TestSuiteName", row.suiteName);
                val.Tags.Add("ClassName", row.className);
                val.Tags.Add("TestCaseName", row.testCaseName);
                val.Tags.Add("Status", row.status);
                val.Fields.Add("Duration", new InfluxValueField((float)row.time));

                // Write to database 
                Task.Run(async () =>
                {
                    var r = await client.PostPointAsync(dbNameJUnit, val);
                }).GetAwaiter().GetResult();
            }
        }

        public static void WriteJUnitTestSuitesSummaryToInfluxDb(List<JUnitSummaryDetails> results)
        {
            // add data to TestSuitesSummary
            foreach (var row in results)
            {
                var val = new InfluxDatapoint<InfluxValueField>();
                val.MeasurementName = "TestSuitesSummary";
                val.UtcTimestamp = reporttime;
                val.Precision = TimePrecision.Seconds;

                val.Tags.Add("TestSuiteName", row.suiteName);
                val.Tags.Add("ID", row.id.ToString());
                val.Tags.Add("Hostname", row.hostname);
                val.Tags.Add("Status", row.status);
                val.Fields.Add("Duration", new InfluxValueField((float)row.time));
                val.Fields.Add("TotalTests", new InfluxValueField(row.tests));
                val.Fields.Add("FailedTests", new InfluxValueField(row.failures));
                val.Fields.Add("TestErrors", new InfluxValueField(row.errors));

                // Write to database 
                Task.Run(async () =>
                {
                    var r = await client.PostPointAsync(dbNameJUnit, val);
                }).GetAwaiter().GetResult();
            }
        }

        public static void WriteNUnitTestCasesSummaryToInfluxDb(List<NUnitTCDetails> results, string additionalAttributes)
        {
            // add data to TestCasesSummary
            foreach (var row in results)
            {
                var val = new InfluxDatapoint<InfluxValueField>();
                val.MeasurementName = "TestCasesSummary";
                val.UtcTimestamp = reporttime;
                val.Precision = TimePrecision.Seconds;

                val.Tags.Add("TestSuiteName", row.suiteName);
                val.Tags.Add("TestCaseName", row.testCaseName);
                val.Tags.Add("Status", row.result);
                val.Fields.Add("Asserts", new InfluxValueField(row.asserts));
                val.Fields.Add("Duration", new InfluxValueField((float)row.time)); 
                val.Fields.Add("FailureMessage", new InfluxValueField(row.failure));

                AddAdditionalTags(ref val, additionalAttributes);

                // Write to database 
                Task.Run(async () =>
                {
                    var r = await client.PostPointAsync(dbNameNUnit, val);
                }).GetAwaiter().GetResult();
            }
        }

        public static void WriteNUnitTestSuitesSummaryToInfluxDb(List<NUnitSummaryDetails> results, string additionalAttributes)
        {
            // add data to TestSuitesSummary
            foreach (var row in results)
            {
                var val = new InfluxDatapoint<InfluxValueField>();
                val.MeasurementName = "TestSuitesSummary";
                val.UtcTimestamp = reporttime;
                val.Precision = TimePrecision.Seconds;

                val.Tags.Add("TestSuiteName", row.suiteName);
                val.Tags.Add("ID", row.id.ToString());
                val.Tags.Add("Status", row.result);
                val.Fields.Add("Duration", new InfluxValueField((float)row.time));
                val.Fields.Add("TestCasesCount", new InfluxValueField(row.testcasecount));
                val.Fields.Add("TotalTests", new InfluxValueField(row.total));
                val.Fields.Add("PassedTests", new InfluxValueField(row.passed));
                val.Fields.Add("FailedTests", new InfluxValueField(row.failed));
                val.Fields.Add("SkippedTests", new InfluxValueField(row.skipped));
                val.Fields.Add("inconclusive", new InfluxValueField(row.inconclusive));

                AddAdditionalTags(ref val, additionalAttributes);

                // Write to database 
                Task.Run(async () =>
                {
                    var r = await client.PostPointAsync(dbNameNUnit, val);
                }).GetAwaiter().GetResult();
            }
        }

        private static void AddAdditionalTags(ref InfluxDatapoint<InfluxValueField> val, string additionalAttributes, string excludeAttributes = null)
        {
            if (String.IsNullOrEmpty(additionalAttributes)) return;
            string [] tagValueArray = additionalAttributes.Split(';');
            string [] excludeAttributesArray = excludeAttributes?.Split(';');
            foreach(string tagValue in tagValueArray)
            {
                if (excludeAttributesArray != null)
                {
                    if (!excludeAttributesArray.Contains(tagValue.Split(':')[0]))
                        val.Tags.Add(tagValue.Split(':')[0], tagValue.Split(':')[1]);
                }
                else
                {
                    val.Tags.Add(tagValue.Split(':')[0], tagValue.Split(':')[1]);
                }
            }
        }

        // includeAttribute should be in the format fieldKey1:dataType1,fieldKey2:dataType2
        private static void AddAdditionalFields(ref InfluxDatapoint<InfluxValueField> val, string additionalAttributes, string includeAttributes)
        {
            if (String.IsNullOrEmpty(additionalAttributes) || String.IsNullOrEmpty(additionalAttributes)) return;
            Dictionary<string, string> fieldAttributes = new Dictionary<string, string>();
            string[] fieldValueArray = additionalAttributes.Split(';');
            string[] includeAttributesArray = includeAttributes.Split(';');
            foreach (var attribute in includeAttributesArray)
            {
                fieldAttributes.Add(attribute.Split(':')[0], attribute.Split(':')[1]);
            }
            foreach(var fieldValue in fieldValueArray)
            {
                if (fieldAttributes.ContainsKey(fieldValue.Split(':')[0]))
                {
                    switch (fieldAttributes[fieldValue.Split(':')[0]])
                    {
                        case "int":
                            val.Fields.Add(fieldValue.Split(':')[0], new InfluxValueField(Convert.ToInt32(fieldValue.Split(':')[1])));
                            break;
                        default:
                            throw new Exception($"Data Type [{fieldAttributes[fieldValue.Split(':')[0]]}] for additional attribute [{fieldValue.Split(':')[0]}] NOT FOUND");
                    }
                }
                    
            }
        }

        private static void CheckConnection()
        {
            try
            {
                Task.Run(async () =>
                {
                    var r = await client.GetServerVersionAsync();
                }).GetAwaiter().GetResult();
            } catch (Exception e)
            {
                throw new Exception(e.Message + $"InfluxDB Connection String: [{connection}]");
            }
        }
    }
}
