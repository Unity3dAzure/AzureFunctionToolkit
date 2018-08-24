using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace UnityRESTRequest
{
    public class Response: EventArgs
    {
        public bool IsError;
        public int StatusCode;
        public string Url;
        public Dictionary<string, string> ResponseHeaders;

        public Response(UnityWebRequest www)
        {
            this.IsError = www.isNetworkError;
            this.StatusCode = (int) www.responseCode;
            this.Url = www.url;
            this.ResponseHeaders = !(www.isNetworkError) ? www.GetResponseHeaders() : null;
        }

        public Response(bool isError, int statusCode, string url, Dictionary<string, string> responseHeaders)
        {
            this.IsError = isError;
            this.StatusCode = statusCode;
            this.Url = url;
            this.ResponseHeaders = responseHeaders;
        }

        public string GetHeaderValue(string key)
        {
            string value = null;
            if (this.ResponseHeaders == null)
            {
                return value;
            }
            this.ResponseHeaders.TryGetValue(key, out value);
            return value;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Status Code: {0} Url: {1} Response Headers:", StatusCode, Url);
            foreach(var header in ResponseHeaders.Reverse())
            {
                sb.AppendFormat("\n{0}:\"{1}\"", header.Key, header.Value);
            }
            return sb.ToString();
        }
    }

    public class ResponseText : Response
    {
        public string Text { get; private set; }

        public ResponseText(UnityWebRequest www, string text) : base(www)
        {
            this.Text = text;
        }

        public ResponseText(bool isError, int statusCode, string url, Dictionary<string, string> responseHeaders, string text) : base(isError, statusCode, url, responseHeaders)
        {
            this.Text = text;
        }
    }

    public class ResponseData<T> : Response
    {
        public T Data { get;  private set; }

        public ResponseData(UnityWebRequest www, T data) : base(www)
        {
            this.Data = data;
        }

        public ResponseData(bool isError, int statusCode, string url, Dictionary<string, string> responseHeaders, T data) : base(isError, statusCode, url, responseHeaders)
        {
            this.Data = data;
        }
    }


}
