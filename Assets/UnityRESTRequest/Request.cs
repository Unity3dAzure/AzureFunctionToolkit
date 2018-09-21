using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace UnityRESTRequest
{
    /// <summary>
    /// UnityEvent for handling a request's reponse event args
    /// </summary>
    [Serializable]
    public class UnityEventResponse : UnityEvent<Response> { };

    /// <summary>
    /// Serializable Key Value pair for use inside Unity Editor
    /// </summary>
    [Serializable]
    public struct SerializableKeyValue
    {
        public string Key;
        public string Value;

        public SerializableKeyValue(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    /// <summary>
    /// Request methods
    /// </summary>
    [Serializable]
    public enum HttpMethod
    {
        GET,
        POST,
        PUT,
        CREATE,
        DELETE,
        HEAD
    }

    /// <summary>
    /// Request base class sending with data type <R> returning Error <E> and Success <S> types for parsing responses
    /// </summary>
    /// <typeparam name="E">Error</typeparam>
    /// <typeparam name="S">Success</typeparam>
    public abstract class Request<E,S> : MonoBehaviour, IRequest //where E : class where S : class
    {
        [Header("Request")]

        [Tooltip("URL Endpoint")]
        public string URLEndpoint = "";
        public Uri Uri;

        /// <summary>
        /// Http Method
        /// </summary>
        public HttpMethod Method = HttpMethod.GET;

        /// <summary>
        /// Query params in format "?key=value&key2=value2"
        /// </summary>
        [SerializeField, Tooltip("URL query parameters")]
        protected List<SerializableKeyValue> Params;

        /// <summary>
        /// Headers
        /// </summary>
        [SerializeField, Tooltip("Header key values")]
        protected List<SerializableKeyValue> Headers;

        public BodyTypePresets ContentType = BodyTypePresets.JSON;

        /// <summary>
        /// Upload body content is not used when using GET or HEAD requests
        /// </summary>
        [TextArea, Tooltip("Request Body")]
        public string Body = "";

        [Header("Response")]

        /// <summary>
        /// Content Type is not used when using GET or HEAD requests
        /// </summary>
        //[Tooltip("Request body type")]
        //public BodyTypePresets ResponseBody = BodyTypePresets.JSON; // using response headers instead for auto detection

        /// <summary>
        /// Hook up a request's success or error status to an external script containing public method handlers with a 'Response' param
        /// </summary>
        [Header("SuccessCallback(Response response)")]
        public UnityEventResponse ResponseSuccess;
        [Header("ErrorCallback(Response response)")]
        public UnityEventResponse ResponseError;

        /// <summary>
        /// Automatically sends request on Start()
        /// </summary>
        public bool AutoSend = false;
        protected bool sending = false;

        /// <summary>
        /// Try to detect json text in the response body for parsing when the incorrect content-type header is sent in the response.
        /// </summary>
        public bool AutoDetect = true;

        /// <summary>
        /// Logging preferences
        /// </summary>
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler);
        public bool Logging = false;

        [Tooltip("The whole number of seconds for client to abort request. No timeout is applied when timeout is 0.")]
        public int Timeout = 0; /// seconds

        void Start()
        {
            if (AutoSend)
            {
                Send();
            }
        }

        public void Send()
        {
            AsyncSend();
        }

        public void Send(byte[] data)
        {
            AsyncSend(data);
        }

        public void Send(string filePath)
        {
            AsyncSend(null, filePath);
        }

        protected virtual void AsyncSend(byte[] data = null, string filePath = "")
        {
            StartCoroutine(SendRequest(data, filePath));
        }

        public virtual void OnSuccess(Response response)
        {
            log(LogType.Log, "Response success", response);
            FireResponseSuccess(response);
        }

        public virtual void OnError(Response response)
        {
            log(LogType.Warning, "Response error", response);
            FireResponseError(response);
        }
        
        /// <summary>
        /// Hook to customize the Unity Web Request before sending
        /// </summary>
        /// <param name="www"></param>
        protected virtual void CustomRequest(UnityWebRequest www)
        {
            /// You can add your custom request options here!
        }

        protected virtual string ConfigureApi()
        {
            /// If you do not want to specify an absolute URLEndpoint value you can set your own host api or base URL for use in all requests
            return ""; 
        }

        protected virtual Uri CreateUri()
        {
            Uri uri;

            string api = ConfigureApi();
            string endpoint = URLEndpoint;
            string query = ParamsToQueryString(Params);
            
            if (endpoint.StartsWith("http"))
            {
                Uri.TryCreate(endpoint + query, UriKind.Absolute, out uri);
                return uri;
            }
            string uriString = api.IndexOf('?') > 0 ? api : api + "/" + endpoint + query;
            Uri.TryCreate(uriString, UriKind.Absolute, out uri);
            return uri;
        }

        private IEnumerator SendRequest(byte[] data = null, string filePath = "")
        {
            Uri = CreateUri();
            if (Uri == null || !Uri.IsAbsoluteUri)
            {
                Debug.LogError("Failed to create absolute URI for request: " + URLEndpoint);
                yield break;
            }
            sending = true;
            using (UnityWebRequest www = new UnityWebRequest(Uri.AbsoluteUri))
            {
                www.method = Method.ToString();
                www.downloadHandler = new DownloadHandlerBuffer();
                www.timeout = Timeout;
                SetRequestHeaders(www, Headers);
                SetUploadHandler(www, data, filePath);
                AddContentTypeHeader(www);
                CustomRequest(www);

                yield return www.SendWebRequest();
                sending = false;

                if (www.isNetworkError || www.isHttpError)
                {
                    log(LogType.Warning, www.method + " Request error", www);
                    ErrorHandler(www);
                }
                else
                {
                    SuccessHandler(www);
                }
            }
        }

        protected virtual void ErrorHandler(UnityWebRequest www)
        {
            var response = ParseResponse<E>(www, AutoDetect);
            OnError(response);
        }

        protected virtual void SuccessHandler(UnityWebRequest www)
        {
            var response = ParseResponse<S>(www, AutoDetect);
            OnSuccess(response);
        }

        public static Response ParseResponse<T>(UnityWebRequest www, bool autoDetect)
        {
            Response response;
            /// Plain text response (string type)
            if (typeof(T).Equals(typeof(string)))
            {
                response = string.IsNullOrEmpty(www.downloadHandler.text) ? new Response(www) : new ResponseText(www, www.downloadHandler.text);
            }
            /// Try to parse response body text as JSON, XML or just return as string.
            else if (!string.IsNullOrEmpty(www.downloadHandler.text))
            {
                string contentTypeHeader = www.GetResponseHeader("content-type");
                string contentType = contentTypeHeader.Split(';').First();
                switch (contentType)
                {
                    case BodyTypes.JSON:
                        var json = Serializer.ParseJson<T>(www.downloadHandler.text);
                        response = new ResponseData<T>(www, json);
                        break;
                    case BodyTypes.XML:
                    case BodyTypes.XMLText:
                        var xml = Serializer.ParseXml<T>(www.downloadHandler.text);
                        response = new ResponseData<T>(www, xml);
                        break;
                    default:
                        if (autoDetect && !string.IsNullOrEmpty(www.downloadHandler.text))
                        {
                            if ( (www.downloadHandler.text.First().Equals('{') && www.downloadHandler.text.Last().Equals('}')) || 
                                 (www.downloadHandler.text.First().Equals('[') && www.downloadHandler.text.Last().Equals(']')) )
                            {
                                Debug.Log("Attempting to parse json string detected in response body");
                                response = new ResponseData<T>(www, Serializer.ParseJson<T>(www.downloadHandler.text));
                                break;
                            }
                        }
                        response = new ResponseText(www, www.downloadHandler.text);
                        break;
                }
            }
            /// Generic binary type response
            else if (www.downloadHandler.data != null)
            {
                response = new ResponseData<byte[]>(www, www.downloadHandler.data);
            }
            /// No body response
            else
            {
                response = new Response(www);
            }
            return response;
        }

        public static string ParamsToQueryString(List<SerializableKeyValue> keyValues)
        {
            if (keyValues == null)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder("?");
            foreach (var kv in keyValues)
            {
                sb.Append(UnityWebRequest.EscapeURL(kv.Key) + "=" + UnityWebRequest.EscapeURL(kv.Value) + "&");
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        private void SetUploadHandler(UnityWebRequest www, byte[] data = null, string filePath = "")
        {
            if (Method.Equals(HttpMethod.GET) || Method.Equals(HttpMethod.HEAD))
            {
                return;
            }
            if (!string.IsNullOrEmpty(Body) && data == null && string.IsNullOrEmpty(filePath))
            {
                log(LogType.Log, string.Format("Upload text:\n\"{0}\"", Body));
                byte[] bodyText = Encoding.UTF8.GetBytes(Body);
                www.chunkedTransfer = false;
                www.uploadHandler = new UploadHandlerRaw(bodyText);
            }
            else if (!string.IsNullOrEmpty(filePath))
            {
#if UNITY_2018
                www.uploadHandler = new UploadHandlerFile(filePath);
#else
                if (File.Exists(filePath))
                {
                    var bytes = File.ReadAllBytes(filePath);
                    www.uploadHandler = new UploadHandlerRaw(bytes);
                }
                else
                {
                    Debug.LogError("Error file not found: " + filePath);
                }
#endif
            }
            else if (data != null)
            {
                www.uploadHandler = new UploadHandlerRaw(data);
            }
            else
            {
                //log(LogType.Warning, "No payload to upload", www);
            }
        }

        private void SetRequestHeaders(UnityWebRequest www, List<SerializableKeyValue> keyValues)
        {
            if (www == null || keyValues == null || keyValues.Count == 0)
            {
                return;
            }
            foreach(var kv in keyValues)
            {
                www.SetRequestHeader(kv.Key, kv.Value);
            }
        }

        public void AddQueryParam(string key, string value)
        {
            var item = new SerializableKeyValue(key, value);
            var index = Params.FindIndex(kv => string.Equals(kv.Key, key));
            if (index < 0)
            {
                Params.Add(item);
            }
            else
            {
                Params[index] = item;
            }
        }

        public void AddHeader(string key, string value)
        {
            var item = new SerializableKeyValue(key, value);
            if (!Headers.Contains(item))
            {
                Headers.Add(item);
            }
            else
            {
                Headers[Headers.FindIndex(kv => string.Equals(kv.Key, key))] = item;
            }
        }

        private void AddContentTypeHeader(UnityWebRequest www)
        {
            if (!Headers.Exists(kv => string.Equals(kv.Key, "content-type", StringComparison.OrdinalIgnoreCase)))
            {
                www.SetRequestHeader("Content-Type", BodyTypes.GetMimeType(ContentType));
            }
        }

#region Unity Events
        protected void FireResponseError(Response response)
        {
            if (ResponseError != null)
            {
                ResponseError.Invoke(response);
            }
        }

        protected void FireResponseSuccess(Response response)
        {
            if (ResponseSuccess != null)
            {
                ResponseSuccess.Invoke(response);
            }
        }
#endregion

#region Logging format helpers for request and response

        protected void log(LogType logType, string message, UnityWebRequest request)
        {
            if (Logging)
            {
                logger.LogFormat(logType, message + "\nStatus Code: {0} Url: {1} Body:\n{2}", request.responseCode, request.url, request.downloadHandler.text);
            }
        }

        protected void log(LogType logType, string message, Response response)
        {
            if (Logging)
            {
                logger.LogFormat(logType, message + "\nStatus Code: {0} Url: {1}", response.StatusCode, response.Url);
            }
        }

        protected void log(LogType logType, string message)
        {
            if (Logging)
            {
                logger.Log(logType, message);
            }
        }

#endregion
    }
}