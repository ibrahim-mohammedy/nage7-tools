using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAutomation.Core
{
    public class JiraAttribute : Attribute
    {
        public string Issue { get; }

        public JiraAttribute(string jiraIssue)
        {
            Issue = jiraIssue;
        }
    }
}