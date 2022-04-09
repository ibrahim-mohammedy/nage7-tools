using System.Xml.Linq;
using System.Linq;
using System;

namespace UpdateResultsToInfluxDb
{
    class NUnitTCDetails
    {
        public string suiteName;
        public string testCaseName;
        public string result;
        public decimal time;
        public int asserts;
        public string failure;

        public NUnitTCDetails(string suiteName, XElement elem)
        {
            this.suiteName = suiteName;
            this.testCaseName = (string)elem.Attribute("name");
            this.time = Decimal.Round((decimal)(elem.Attribute("time") != null ? elem.Attribute("time") : elem.Attribute("duration")), 2);
            this.result = (string)elem.Attribute("result");
            this.asserts = (int)elem.Attribute("asserts");
            this.failure = elem.Descendants("failure").Any() ? GetFailureMessage(elem) : "";
        }

        private string GetFailureMessage(XElement elem)
        {
            string msg = elem.Descendants("failure").Elements("message").FirstOrDefault().Value;
            string stackTrace = elem.Descendants("failure").Elements("stack-trace").FirstOrDefault().Value;
            stackTrace = stackTrace.Length > 250 ? stackTrace.Substring(0, 250) : stackTrace;
            return msg + stackTrace;
        }
    }
}
