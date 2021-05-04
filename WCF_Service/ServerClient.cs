using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCF_Service
{
    [DataContract]
    public class ServerClient
    {
        public IPEndPoint IPAddress { get; internal set; }

        public string NickName { get; internal set; }

        internal ServerClient() { }

        [IgnoreDataMember]
        internal OperationContext OperationContext { get; set; }
    }
}