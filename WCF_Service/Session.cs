using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WCF_Service
{
    [DataContract]
    public class Session
    {
        public string SessionName { get; }

        public bool IsPasswordRequired => !string.IsNullOrEmpty(SessionPassword);

        public IEnumerable<ServerClient> Clients { get; } = new List<ServerClient>();

        public ServerClient Creator { get; }

        [IgnoreDataMember]
        internal string SessionPassword { get; set; }
    }
}