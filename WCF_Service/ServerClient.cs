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
        [DataMember]
        public IPEndPoint IPAddress { get; internal set; }

        [DataMember]
        public string NickName { get; internal set; }

        [DataMember]
        public IEnumerable<Session> Sessions { get => SessionsInternal; }

        internal ServerClient() { }

        [DataMember]
        internal List<Session> SessionsInternal { get; set; } = new List<Session>();
    }
}