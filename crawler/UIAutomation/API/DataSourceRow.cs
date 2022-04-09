using System.Collections.Generic;

namespace UIAutomation.API
{
    public class DataSourceCell
    {
        public string ColumnId { get; set; }

        public string Value { get; set; }
    }

    public class DataSourceRow : IDocument
    {
        public string Id { get; set; } = "";
        public string DataSourceId { get; set; }

        public List<DataSourceCell> Cells { get; set; }
    }
}