using System.Xml.Linq;

namespace UpdateResultsToInfluxDb
{
    class JUnitTCDetails
    {
        public string suiteName;
        public string className;
        public string testCaseName;
        public string status;
        public decimal time;

        public JUnitTCDetails(string suiteName, XElement elem)
        {
            this.suiteName = suiteName;
            this.className = (string)elem.Attribute("classname");
            this.testCaseName = (string)elem.Attribute("name");
            this.time = (decimal)elem.Attribute("time");
            this.status = (elem.Element("failure") == null) ? "Passed" : "Failed";
        }
    }
}
