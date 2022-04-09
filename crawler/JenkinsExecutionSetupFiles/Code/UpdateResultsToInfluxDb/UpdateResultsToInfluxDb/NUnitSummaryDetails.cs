using System;
using System.Xml.Linq;

namespace UpdateResultsToInfluxDb
{
    class NUnitSummaryDetails
    {
        public string suiteName;
        public string id;
        public int testcasecount;
        public string result;
        public decimal time;
        public int total;
        public int passed;
        public int failed;
        public int inconclusive;
        public int skipped;
        public int asserts;

        public NUnitSummaryDetails(XElement elem)
        {
            this.suiteName = (string)elem.Attribute("name");
            this.id = (string)elem.Attribute("id");
            this.testcasecount = (int)elem.Attribute("testcasecount");
            this.result = (string)elem.Attribute("result");
            this.time = Decimal.Round ((decimal)(elem.Attribute("time") != null ? elem.Attribute("time") : elem.Attribute("duration")), 2);
            this.total = (int)elem.Attribute("total");
            this.passed = (int)elem.Attribute("passed");
            this.failed = (int)elem.Attribute("failed");
            this.inconclusive = (int)elem.Attribute("inconclusive");
            this.skipped = (int)elem.Attribute("skipped");
            this.asserts = (int)elem.Attribute("asserts");
            CheckCounts();
        }

        private void CheckCounts()
        {
            if (testcasecount < total)
                throw new ArithmeticException($"Incorrect values in Test Result xml for test fixture [{suiteName}]: testcasecount[{testcasecount}] < total [{total}]");
            if (total != passed + failed + skipped + inconclusive)
                throw new ArithmeticException($"Incorrect values in Test Result xml for test fixture [{suiteName}]: total[{total}] != passed[{passed}] + failed[{failed}] + skipped[{skipped}] + inconclusive[{inconclusive}]");
        }
    }
}
