using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAutomation.Core
{
    public class MetaDataCollection : Dictionary<string, MetaData>
    {
        public MetaDataCollection()
        {
        }

        public MetaDataCollection(MetaDataCollection initializer)
        {
            Set(initializer);
        }

        public new MetaData this[string name]
        {
            get { return Get(name); }
            set { Set(name, value); }
        }

        public void Set(string name, string value)
        {
            base[name.ToLower()] = new MetaData(name, value);
        }

        public void Set(string name, string value, MetaDataValidity validity)
        {
            base[name.ToLower()] = new MetaData(name, value, validity);
        }

        public void Set(string name, string value, MetaDataValidity validity, Location l)
        {
            base[name.ToLower()] = new MetaData(name, value, validity, l);
        }

        public void Set(string name, MetaData value)
        {
            base[name.ToLower()] = value;
        }

        public void Set(MetaDataCollection mdc)
        {
            foreach (string key in mdc.Keys)
            {
                this.Set(key, mdc.Get(key));
            }
        }

        public MetaData Get(string name)
        {
            if (!Contains(name)) return null;

            return base[name.ToLower()];
        }

        public new bool Remove(string name)
        {
            if (!Contains(name)) return false;

            return base.Remove(name.ToLower());
        }

        public bool Contains(string name)
        {
            return base.ContainsKey(name.ToLower());
        }

        public MetaDataValidity Validity
        {
            get
            {
                if (HasValidity(MetaDataValidity.NotValid)) { return MetaDataValidity.NotValid; }
                if (HasValidity(MetaDataValidity.Suspicious)) { return MetaDataValidity.Suspicious; }
                if (HasValidity(MetaDataValidity.Unknown)) { return MetaDataValidity.Unknown; }

                return MetaDataValidity.Valid;
            }
        }

        private bool HasValidity(MetaDataValidity validity)
        {
            foreach (string key in Keys)
            {
                MetaData metadata = Get(key);
                if (metadata.Validity == validity)
                {
                    return true;
                }
            }

            return false;
        }
    }
}