using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityRESTRequest
{
    /// <summary>
    /// Durable Azure Functions beta (implementation may change)
    /// </summary>
    /// <typeparam name="E"></typeparam>
    /// <typeparam name="S"></typeparam>
    public abstract class DurableFunction<E,S> : AzureFunction<E,S>
    {
        public string TaskHub = "SampleHubJs";
        public string Connection = "Storage";

        /// <summary>
        /// Request will be redirected using the following endpoint using the instance id returned by the initial request headers. 
        /// </summary> 
        protected string RedirectedStatusEndpointFormat = "{0}/runtime/webhooks/DurableTaskExtension/instances/{1}?taskHub={2}&connection={3}";
        private bool shouldRedirect = false;
        private string hostApi = "";
        private string instanceId = "";

        /// <summary>
        /// Here we override the http method as Durable functions use POST to start the task and then use GET to poll status
        /// </summary>
        /// <param name="www"></param>
        protected override void CustomRequest(UnityWebRequest www)
        {
            base.CustomRequest(www);
            if (!shouldRedirect)
            {
                www.method = UnityWebRequest.kHttpVerbPOST;
            }
            else
            {
                www.method = UnityWebRequest.kHttpVerbGET;
            }
        }

        /// <summary>
        /// Ensure request should use POST 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filePath"></param>
        protected override void AsyncSend(byte[] data = null, string filePath = "")
        {
            /// reset state if previous request has completed
            if (HasRequestSucceeded())
            {
                shouldRedirect = false;
                hostApi = "";
                instanceId = "";
            }
            base.AsyncSend(data, filePath);
        }

        public override void OnSuccess(Response response)
        {
            if (!shouldRedirect)
            {
                var res = response as ResponseText;
                if (CheckId(res.Text))
                {
                    instanceId = res.Text;
                    log(LogType.Log, "Got instance id: " + instanceId);
                    shouldRedirect = true;
                }
            }
            else if (ValidateResponse(response))
            {
                log(LogType.Log, "Durable Function Request Succeeded!", response);

                /// return instance output array 
                var res = response as ResponseData<DurableInstance<S>>;
                var output = res.Data.output;
                var ouputResponse = new ResponseData<S[]>(response.IsError, response.StatusCode, response.Url, response.ResponseHeaders, output);

                RequestSucceeded();
                FireResponseSuccess(ouputResponse);
            }
        }

        protected override bool ValidateResponse(Response response)
        {
            var result = response as ResponseData<DurableInstance<S>>;
            if (result != null && string.Equals(result.Data.runtimeStatus, "Completed"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check the instance id is returned ok in the response body text
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool CheckId(string id)
        {
            string pattern = @"[a-z0-9-]+";
            RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnoreCase;
            var regex = Regex.Matches(id, pattern, options);
            if (regex.Count == 1)
            {
                return true;
            }
            Debug.LogError("Unexpected instance id format");
            return false;
        }

        protected override string ConfigureApi()
        {
            if (!shouldRedirect)
            {
                return base.ConfigureApi();
            }
            if (string.IsNullOrEmpty(hostApi))
            {
                hostApi = GetHost(Account);
            }
            string pollingUri = string.Format(RedirectedStatusEndpointFormat, hostApi, instanceId, TaskHub, Connection);
            return pollingUri;
        }

        protected override void SuccessHandler(UnityWebRequest www)
        {
            if (!shouldRedirect)
            {
                base.SuccessHandler(www);
            }
            else
            {
                var response = ParseResponse<DurableInstance<S>>(www, AutoDetect);
                OnSuccess(response);
            }
        }
    }
}
