using System;
using System.Xml.Serialization;

namespace UnityRESTRequest.XMLModels
{
    [Serializable]
    [XmlRoot("item")]
    public class Message
    {
        [XmlElement("message")]
        public string message;
    }
}