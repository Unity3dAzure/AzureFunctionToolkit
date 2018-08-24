using System;
using System.Xml.Serialization;

namespace UnityRESTRequest.XMLModels
{
    [Serializable]
    [XmlRoot("item")]
    public class Error
    {
        [XmlElement("message")]
        public string error;
    }
}