using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UIAutomation.Core
{
    public enum MetaDataValidity { Unknown = 0, Valid = 1, Suspicious = 2, NotValid = 3 };

    public enum MetaDataValueType { String = 0, LineItems = 1 };

    public class Location
    {
        public bool Valid { get; set; }
        public int Page { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }
    }

    public class MetaData
    {
        public MetaData(string name, object value)
        {
            Name = name;
            Value = value;

            ValueType = MetaDataValueType.String;
        }

        public MetaData(string name, object value, MetaDataValidity validity) : this(name, value)
        {
            Validity = validity;
        }

        public MetaData(string name, object value, MetaDataValidity validity, Location l) : this(name, value, validity)
        {
            Location = l;
        }

        public MetaData()
        {
        }

        public string Name { get; set; } = "";

        public MetaDataValidity Validity { get; set; } = MetaDataValidity.Unknown;

        public MetaDataValueType ValueType { get; set; } = MetaDataValueType.String;

        public Location Location { get; set; } = new Location();

        [JsonConverter(typeof(MetaDataValueConverter))]
        public object Value { get; set; } = "";

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class MetaDataValueConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MetaData);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return "";
            }
            else if (reader.TokenType == JsonToken.String)
            {
                return serializer.Deserialize(reader, typeof(string));
            }

            return "";
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}