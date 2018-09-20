using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
/// To use Newtonsoft JSON add "JSON_NET" to the "Scripting Define Symbols" in Player Settings > Other Settings
#if JSON_NET
using Newtonsoft.Json; // Requires Newtonsoft JSON plugin
#endif

namespace UnityRESTRequest
{
    public static class Serializer
    {
        public static T ParseJson<T>(string json)
        {
            //Debug.Log("Try parse JSON using type: " + typeof(T) + "\n" + json);
            try
            {
#if JSON_NET
                return JsonConvert.DeserializeObject<T>(json); // Requires Newtonsoft JSON plugin
#else
                return JsonUtility.FromJson<T>(json);
#endif
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
