using System.Collections.Generic;

namespace hp.utilities
{
    public class NameValuePair
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public NameValuePair(string pName, object pValue)
        {
            Name = pName;
            Value = pValue;
        }
    }

    public class NameValuePairs : List<NameValuePair>
    {
        public NameValuePair this[string pName]
        {
            get
            {
                foreach (NameValuePair mItem in this)
                {
                    if (mItem.Name.Equals(pName))
                        return mItem;
                }

                return null;
            }
        }
    }
}
