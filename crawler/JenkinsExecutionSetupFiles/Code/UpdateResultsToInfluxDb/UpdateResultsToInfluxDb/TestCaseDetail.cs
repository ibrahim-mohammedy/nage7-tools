namespace UpdateResultsToInfluxDb
{
    class TestCaseDetails
    {
        public string projName;
        public string testCaseName;
        public string status;
        public int duration;

        public TestCaseDetails(string projName, string testCaseName, string status, int duration)
        {
            this.projName = projName;
            this.testCaseName = testCaseName;
            this.status = status;
            this.duration = duration;
        }
    }
}
