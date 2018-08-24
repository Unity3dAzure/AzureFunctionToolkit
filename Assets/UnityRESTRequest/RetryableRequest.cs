using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityRESTRequest;

/// <summary>
/// Request with retry after number of sceonds if response is not successful. 
/// Note: You might want to set the Request Timeout value to be a number of seconds.
/// </summary>
public abstract class RetryableRequest<E,S> : Request<E,S>
{
    private bool requestSucceeded = false;

    /// <summary>
    ///  State retained for retryable request
    /// </summary>
    private byte[] _data = null;
    private string _filePath = "";

    [Tooltip("Retry request after a number of seconds after Timeout time")]
    public float RetryAfter = 0.5f; /// seconds
    private float timer = 0;

    void Update () {
		if (requestSucceeded || sending)
        {
            return;
        }
        timer += Time.deltaTime;
        if (timer > Timeout + RetryAfter)
        {
            log(LogType.Log, "Retrying request after " + timer);
            timer = 0;
            Retry();
        }
	}

    protected virtual void Retry()
    {
        AsyncSend(_data, _filePath);
    }

    protected override void AsyncSend(byte[] data = null, string filePath = "")
    {
        /// Retain state for retry request
        this._data = data;
        this._filePath = filePath;
        this.requestSucceeded = false;
        /// Send with state
        base.AsyncSend(data, filePath);
    }

    /// <summary>
    /// This is the Custom logic to implement to tell if the request has succeeded.
    /// </summary>
    protected virtual bool ValidateResponse(Response response)
    {
        if (response.IsError)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Note that we don't trigger the "FireResponseSuccess" event until the response has been validated as completed using the "ValidateResponse" method
    /// </summary>
    /// <param name="response"></param>
    public override void OnSuccess(Response response)
    {
        if (ValidateResponse(response))
        {
            log(LogType.Log, "Retry Request Succeeded!", response);
            RequestSucceeded();
            FireResponseSuccess(response);
        }
    }

    protected void RequestSucceeded()
    {
        requestSucceeded = true; /// stops the retry logic
        Clear();
    }

    public bool HasRequestSucceeded()
    {
        return requestSucceeded;
    }

    private void Clear()
    {
        _filePath = "";
        _data = null;
        timer = 0;
    }

}
