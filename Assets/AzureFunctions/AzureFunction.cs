using System;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityRESTRequest
{
    public abstract class AzureFunction<E,S> : RetryableRequest<E,S>
    {
        [Header("Azure Function")]

        [Tooltip("Enter Function account name")]
        public string Account = "localhost";

        [Tooltip("Enter route prefix")]
        public string RoutePrefix = "api";

        [Tooltip("Enter Function name")]
        public string Function = "hello";

        [SerializeField, Tooltip("Enter Function code. \nNB: This is for development use only. Don't store your private Function Code in public source control!")]
        private string Code = "";

        protected static string LOCALHOST = "http://localhost:7071";
        protected static string HOST_ACCOUNT = "https://{0}.azurewebsites.net";

        protected override void CustomRequest(UnityWebRequest www)
        {
            if (!string.IsNullOrEmpty(Code))
            {
                www.SetRequestHeader("code", Code.Trim());
            }
        }

        protected override string ConfigureApi()
        {
            /// Use URLEndpoint if defined
            if (!string.IsNullOrEmpty(URLEndpoint))
            {
                return URLEndpoint;
            }
            /// Else build Uri using Function properties
            string host = GetHost(Account);
            string functionPath = string.IsNullOrEmpty(RoutePrefix) ? Function : "/" + RoutePrefix + "/" + Function;
            return host + functionPath;
        }

        /// <summary>
        /// Returns localhost API or deployed Azure Functions API base url
        /// </summary>
        /// <returns></returns>
        public string GetHost(string accountName)
        {
            if (string.IsNullOrEmpty(Account) ||
                string.Equals(Account, "localhost", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Account, "127.0.0.1", System.StringComparison.OrdinalIgnoreCase))
            {
                return LOCALHOST;
            }
            else
            {
                return string.Format(HOST_ACCOUNT, accountName);
            }
        }

        public void SetFunctionCode(string code)
        {
            Code = code;
        }
    }

    public abstract class AzureFunction : AzureFunction<string,string>
    {
    }
}
