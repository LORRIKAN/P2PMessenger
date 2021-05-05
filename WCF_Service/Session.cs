using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WCF_Service
{
    [DataContract]
    public class Session
    {
        [DataMember]
        public string SessionName { get; internal set; }

        [DataMember]
        public bool IsPasswordRequired => !string.IsNullOrEmpty(SessionPassword);

        [DataMember]
        public IEnumerable<ServerClient> Clients { get => ClientsInternal; }

        [DataMember]
        public ServerClient Creator { get; internal set; }

        internal Session() { }

        internal List<ServerClient> ClientsInternal { get; } = new List<ServerClient>();

        internal string SessionPassword { get; set; }
    }
}