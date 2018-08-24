using System;
using System.Xml.Serialization;

namespace UnityRESTRequest.XMLModels
{
    [Serializable]
    [XmlRoot("item")]
    public class Name
    {
        [XmlElement("name")]
        public string name;
    }
}