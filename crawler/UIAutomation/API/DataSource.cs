using System.Collections.Generic;

namespace UIAutomation.API
{
    public enum DataState
    {
        NoChange = 0,
        Created = 1,
        Modified = 2,
        Deleted = 3
    }

    public class DBColumn
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int Type { get; set; } = 2;
        public DataState State { get; set; }
    }

    public class DataSource : IDocument
    {
        public string Id { get; set; } = "";
        public string TenantId { get; set; } = "";
        public string Name { get; set; }
        public List<DBColumn> Columns { get; set; }
    }
}