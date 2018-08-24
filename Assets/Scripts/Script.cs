using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityRESTRequest;
using UnityRESTRequest.XMLModels;

public class Script : MonoBehaviour {

    public TextMesh textMesh;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdateText(string text)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
        }
    }

    public void OnSuccessResponse(Response response)
    {
        /// Handle JSON data response
        var responseData = response as ResponseData<Message>;
        if (responseData != null)
        {
            Debug.Log("Got success message data:\n" + responseData.Data.message);
            UpdateText(responseData.Data.message);
            return;
        }

        /// Handle Durable Function array data response
        var responseArray = response as ResponseData<string[]>;
        if (responseArray != null)
        {
            var sb = new StringBuilder();
            foreach(var s in responseArray.Data)
            {
                sb.Append(s + "\n");
            }
            Debug.Log("Got success array data:\n" + sb);
            UpdateText(sb.ToString());
            return;
        }

        /// Handle text response
        var responseText = response as ResponseText;
        if (responseText != null)
        {
            Debug.Log("Got text:\n" + responseText.Text);
            UpdateText(responseText.Text);
            return;
        }

        Debug.Log("Success " + response.ToString());
        UpdateText("Success");
    }

    public void OnErrorResponse(Response response)
    {
        /// Handle JSON error response
        var res = response as ResponseData<Error>;
        if (res != null)
        {
            Debug.Log("Got error message:\n" + res.Data.error);
            UpdateText(res.Data.error);
            return;
        }

        var responseText = response as ResponseText;
        if (responseText != null)
        {
            Debug.LogWarningFormat("Error message: {2} \nStatus Code: {0} Url: {1}", response.StatusCode, response.Url, responseText.Text);
            UpdateText(responseText.Text);
            return;
        }

        Debug.LogWarningFormat("Something went wrong... \nStatus Code: {0} Url: {1}", response.StatusCode, response.Url);
        UpdateText("Error");
    }
}
