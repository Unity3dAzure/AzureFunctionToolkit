using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
//using Newtonsoft.Json; /// uncomment if you have Newtownsoft plugin

namespace UnityRESTRequest
{
    static class Serializer
    {
        public static T ParseJson<T>(string json)
        {
            //Debug.Log("Try parse JSON using type: " + typeof(T) + "\n" + json);
            try
            {
                //var data = JsonConvert.DeserializeObject<T>(json); /// uncomment if you have Newtownsoft plugin
                var data = JsonUtility.FromJson<T>(json);
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError("Parse JSON Type " + typeof(T) + " exception: " + ex);
            }
            return default(T);
        }

        public static T ParseXml<T>(string xml)
        {
            //Debug.Log("Try parse XML using type: " + typeof(T) + "\n" + xml);
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
                {
                    return (T)serializer.Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Parse XML Type " + typeof(T) + " exception: " + ex);
            }
            return default(T);
        }
    }
}
