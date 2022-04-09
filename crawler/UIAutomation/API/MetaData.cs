using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAutomation.API
{
    public enum MetaDataType
    {
        Text = 0,
        LargeText = 1,
        Password = 2,
        Integer = 3,
        Currency = 4,
        Date = 6,
        List = 8,
        LineItems = 9,
        Decimal = 10,
        Lookup = 11,
        Expression = 12,
        CascadingOptions = 13
    }

    public class ValidationTypes
    {
        public const string Length = "Length";
        public const string Regex = "Regex";
        public const string RegEx = "RegEx"; // nuisance :(
        public const string List = "List";
        public const string DataSource = "DataSource";
    }

    // this is a "simplification hack" - merging all validation together into one object through the magic of json
    public class Validation
    {
        // core
        public string Type { get; set; } = "MetaDataNOOPValidator";

        public string ValidationMessage { get; set; } = "";

        // length
        public int MinLength { get; set; }

        public int MaxLength { get; set; }

        // regex
        public string Pattern { get; set; } = "";
    }

    // this is a "simplification hack" - merging all validation together into one object through the magic of json
    public class Lookup
    {
        public class Mapping
        {
            public int FieldNumber { get; set; }
            public string MetaDataId { get; set; } = string.Empty;
            public bool Overwrite { get; set; } = true;
        }

        public string Type { get; set; } = "Upland.Hydra.Core.MetaDataNoopLookup";

        public int LookupField { get; set; }
        public List<Mapping> Mappings { get; set; } = new List<Mapping>();

        // FILEBOUND
        public string FileBoundURL { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;

        // CSV
        public string FileName { get; set; } = string.Empty;

        public int RowCount { get; set; }
        public bool ContainsHeader { get; set; }
        public List<string> FirstRow { get; set; } = new List<string>();
    }

    public class DataSourceItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class MetaDataDataSource
    {
        public string Type { get; set; } = "MetaDataListDataSource"; // on purpose
        public List<DataSourceItem> DataSet { get; set; } = new List<DataSourceItem>();
    }

    public class CascadingOptionsDataSourceItem : DataSourceItem
    {
        public string MetaDataId { get; set; }
    }

    public class MetaDataCascadingOptionsDataSource : MetaDataDataSource
    {
        public string DataSourceId { get; set; }

        public MetaDataCascadingOptionsDataSource()
        {
            Type = "MetaDataCascadingOptionsDataSource";
        }
    }

    public class LineItemConfiguration
    {
        public string MetaDataId { get; set; } = "";

        public string CompareToMetaDataId { get; set; } = "";

        public bool ShowTotal { get; set; }

        public LineItemConfiguration()
        {
        }

        public LineItemConfiguration(string metaDataId)
        {
            MetaDataId = metaDataId;
        }
    }

    public class MetaData : IDocument
    {
        public string Id { get; set; } = "";

        public string TenantId { get; set; } = "";

        public string Name { get; set; } = "";

        public string Label { get; set; } = "";

        public MetaDataType Type { get; set; } = MetaDataType.Text;

        public string DefaultValue { get; set; } = "";

        public bool StickyField { get; set; } = false;

        public bool AllowNoSelection { get; set; } = false;

        public Validation Validation { get; set; } = new Validation();

        public List<LineItemConfiguration> LineItemConfigurations { get; set; } = new List<LineItemConfiguration>();

        public string GroupingFormat { get; set; } = "";

        public Lookup Lookup { get; set; } = new Lookup();

        public List<string> AssignedWorkflowIds { get; set; } = new List<string>();

        public MetaDataDataSource DataSource { get; set; } = new MetaDataDataSource();
    }
}