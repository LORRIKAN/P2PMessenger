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
    public class Session : IEquatable<Session>
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

        public override bool Equals(object obj)
        {
            return Equals(obj as Session);
        }

        public bool Equals(Session other)
        {
            return other != null &&
                   SessionName == other.SessionName;
        }

        public override int GetHashCode()
        {
            return 1594474388 + EqualityComparer<string>.Default.GetHashCode(SessionName);
        }

        public static bool operator ==(Session left, Session right)
        {
            return EqualityComparer<Session>.Default.Equals(left, right);
        }

        public static bool operator !=(Session left, Session right)
        {
            return !(left == right);
        }
    }
}