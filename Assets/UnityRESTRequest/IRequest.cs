using UnityEngine.Networking;

namespace UnityRESTRequest
{
    interface IRequest
    {
        #region Input

        void Send();

        void Send(byte[] data);

        void Send(string filePath);

        #endregion

        #region Output

        void OnSuccess(Response response);

        void OnError(Response response);

        #endregion
    }
}
