using System;
using System.Xml.Linq;

namespace UpdateResultsToInfluxDb
{
    class JUnitSummaryDetails
    {
        public string suiteName;
        public DateTime timestamp;
        public int id;
        public string hostname;
        public int tests;
        public int errors;
        public int failures;
        public string status;
        public decimal time;

        public JUnitSummaryDetails(XElement elem)
        {
            this.suiteName = (string)elem.Attribute("name");
            this.timestamp = (DateTime)elem.Attribute("timestamp");
            this.id = (int)elem.Attribute("id");
            this.hostname = (string)elem.Attribute("hostname");
            this.tests = (int)elem.Attribute("tests");
            this.errors = (int)elem.Attribute("errors");
            this.failures = (int)elem.Attribute("failures");
            this.time = (decimal)elem.Attribute("time");
            this.status = (failures == 0) ? "Passed" : "Failed";
        }
    }
}
