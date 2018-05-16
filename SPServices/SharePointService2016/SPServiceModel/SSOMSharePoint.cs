using System;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SPService.Model
{
    /// <summary>
    /// Reponse Wrapper
    /// </summary>
    //[DataContract(Namespace = "http://www.digitcyber.com/")]
    //[XmlRoot(Namespace = "http://www.digitcyber.com/")]
    [DataContract]
    [XmlSerializerFormat]
    public class SSOMSharePoint
    {
        [DataMember, XmlAttribute]
        public string Version { get; set; }

        [DataMember, XmlAttribute]
        public string Machine { get; set; }

        [DataMember, XmlAttribute]
        public string URL { get; set; }

        [DataMember, XmlAttribute]
        public string UserName { get; set; }

        [DataMember, XmlAttribute]
        public string UserDomain { get; set; }

        [DataMember, XmlAttribute]
        public string Method { get; set; }

        [DataMember, XmlAttribute]
        public DateTime StartTimeUTC { get; set; }

        [DataMember, XmlAttribute]
        public DateTime EndTimeUTC { get; set; }

        [DataMember, XmlAttribute]
        public long DurationTicks { get; set; }

        [DataMember, XmlAttribute]
        public int DurationTicksPerSecond { get; set; }

        [DataMember, XmlAttribute]
        public string Culture { get; set; }

        [DataMember, XmlAttribute]
        public string CultureName { get; set; }

        [DataMember, XmlAttribute]
        public bool Error { get; set; }

        [DataMember, XmlAttribute]
        public string ErrorMessage { get; set; }

        [DataMember, XmlAttribute]
        public Result Result { get; set; }

        //public XmlSchema GetSchema()
        //{
        //    return null;
        //}

        #region IXmlSerializable Members
        //public void ReadXml(XmlReader reader)
        //{
        //}

        //public void WriteXml(XmlWriter writer)
        //{
        //    var properties = this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        //    foreach (var property in properties)
        //    {
        //        var propertyValue = property.GetValue(this, null);
        //        var attributes = property.Attributes as IEnumerable;

        //        foreach (var attribute in attributes)
        //        {
        //            if (attribute is OrderAttribute)
        //            {
        //                var orderAttr = attribute as OrderAttribute;
        //                if (orderAttr.Required)
        //                    propertyValue = jGadgets.GetValueByType(property.PropertyType, jAdapter.ToString(propertyValue));

        //                if (property.PropertyType == typeof(DateTime))
        //                {
        //                    if (Convert.ToDateTime(propertyValue) < Constants.DefaultMinDate
        //                        || Convert.ToDateTime(propertyValue) > Constants.DefaultMaxDate)
        //                        propertyValue = DateTime.UtcNow;
        //                }

        //                if (!string.IsNullOrEmpty(jAdapter.ToString(propertyValue)) && orderAttr.Length > 0)
        //                {
        //                    if (jAdapter.ToString(propertyValue).Length > orderAttr.Length)
        //                        propertyValue = jAdapter.ToString(propertyValue).Truncate(orderAttr.Length);
        //                }
        //            }
        //        }

        //    }
        //    writer.WriteAttributeString("id", ID.ToString());
        //    writer.WriteAttributeString("name", Name);
        //    //we'll keep the description as an element as it could be long.
        //    writer.WriteElementString("description", Description);
        //}
        #endregion 
    }

    [DataContract]
    public class Result
    {
        [DataMember, XmlAttribute]
        public string Type { get; set; }

        [DataMember, XmlAttribute]
        public string Value { get; set; }

        [DataMember, XmlAttribute]
        public string DESC { get; set; }

        [DataMember, XmlAttribute]
        public bool IsCompressed { get; set; }
    }
}
