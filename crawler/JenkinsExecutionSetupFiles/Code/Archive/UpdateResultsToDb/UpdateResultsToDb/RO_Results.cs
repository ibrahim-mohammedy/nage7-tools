//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UpdateResultsToDb
{
    using System;
    using System.Collections.Generic;
    
    public partial class RO_Results
    {
        public long Id { get; set; }
        public System.DateTime DateTimeStamp { get; set; }
        public string BuildUrl { get; set; }
        public string Environment { get; set; }
        public string Browser { get; set; }
        public string Persona { get; set; }
        public string TestName { get; set; }
        public string TestCaseId { get; set; }
        public string TestDescription { get; set; }
        public Nullable<double> ExecutionTime { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
    }
}
