using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace SPService.Model
{
    //[DataContract]
    //[XmlType(AnonymousType = false, TypeName = "SSOMMock", Namespace = "http://www.digitcyber.com/")]
    [XmlRoot(Namespace = "http://www.digitcyber.com/", IsNullable = false)]
    public class SSOMMock
    {
        [XmlAttribute()]
        public int ID { get; set; }

        [XmlElement()]
        public string Name { get; set; }
    }
}
