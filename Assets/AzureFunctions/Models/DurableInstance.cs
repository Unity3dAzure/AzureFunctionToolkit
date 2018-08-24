using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityRESTRequest
{
    [Serializable]
    public class DurableInstance<T>
    {
        public string instanceId;
        public string runtimeStatus;
        public T[] output;
        public DateTime createdTime;
        public DateTime lastUpdatedTime;
    }
}
