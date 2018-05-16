using System.Collections.Generic;

namespace hp.utilities
{
    internal class NameValuePair
    {
        internal string Name { get; set; }
        internal string Value { get; set; }
    }

    internal class NameValuePairs : List<NameValuePair>
    {
        internal NameValuePair this[string _Name]
        {
            get
            {
                foreach (NameValuePair Item in this)
                {
                    if (Item.Name.Equals(_Name))
                    {
                        return Item;
                    }
                }
                return null;
            }
        }
    }
}
