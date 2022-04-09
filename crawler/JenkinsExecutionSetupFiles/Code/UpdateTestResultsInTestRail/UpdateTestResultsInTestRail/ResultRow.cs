using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateTestResultsInTestRail
{
    class ResultRow
    {
        public string scriptName;
        public string caseId;
        public string suiteId;
        public string runId;
        public string resultStatus;

        public ResultRow(string scriptName, string caseId, string suiteId, string runId, string resultStatus)
        {
            this.scriptName = scriptName;
            this.caseId = caseId;
            this.suiteId = suiteId;
            this.runId = runId;
            this.resultStatus = resultStatus;
            //Display();
        }

        public void Display()
        {
            Console.WriteLine("scriptName: {0}, caseId: {1}, suiteId: {2}, runId: {3}, resultStatus: {4}", scriptName, caseId, suiteId, runId, resultStatus);
        }
    }
}
