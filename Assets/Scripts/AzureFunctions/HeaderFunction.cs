using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityRESTRequest;

public sealed class HeaderFunction : AzureFunction
{
    public override void OnSuccess(Response response)
    {
        base.OnSuccess(response);

        string id = response.GetHeaderValue("some-id");
        Debug.Log("hey, I got some id:" + id);
    }
}
