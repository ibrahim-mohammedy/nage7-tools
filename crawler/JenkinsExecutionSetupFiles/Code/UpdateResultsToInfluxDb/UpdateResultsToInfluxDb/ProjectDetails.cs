namespace UpdateResultsToInfluxDb
{
    class ProjectDetails
    {
        public string projName;
        public int testSuites;
        public int passedTestCases;
        public int failedTestCases;
        public int testSteps;
        public int passedAssertions;
        public int failedAssertions;
        public string status;
        public int duration;

        public ProjectDetails(string projName, int duration, int testSuites, int totalTestCases, int failedTestCases, int testSteps, int totalAssertions, int failedAssertions)
        {
            this.projName = projName;
            this.duration = duration;
            this.testSuites = testSuites;
            this.passedTestCases = totalTestCases - failedTestCases;
            this.failedTestCases = failedTestCases;
            this.testSteps = testSteps;
            this.passedAssertions = totalTestCases - failedAssertions;
            this.failedAssertions = failedAssertions;
            this.status = (failedAssertions == 0) ? "Passed" : "Failed";
        }
    }
}
