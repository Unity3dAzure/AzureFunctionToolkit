using System;
using UnityEngine;

namespace UnityRESTRequest
{
    /// <summary>
    /// Response body text types
    /// </summary>
    [Serializable]
    public enum BodyTypePresets
    {
        RawData,
        Text,
        JSON,
        XML,
        HTML
    }

    public static class BodyTypes
    {
        public const string RawData = "application/octet-stream";
        public const string Text = "text/plain";
        public const string JSON = "application/json";
        public const string XML = "application/xml";
        public const string XMLText = "text/xml";
        public const string HTML = "text/html";

        public static string GetMimeType(BodyTypePresets contentType)
        {
            switch (contentType)
            {
                case BodyTypePresets.Text:
                    return BodyTypes.Text;
                case BodyTypePresets.JSON:
                    return BodyTypes.JSON;
                case BodyTypePresets.XML:
                    return BodyTypes.XML;
                case BodyTypePresets.HTML:
                    return BodyTypes.HTML;
                default:
                    return BodyTypes.RawData;
            }
        }
    }
}