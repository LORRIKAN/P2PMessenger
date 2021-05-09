using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;

namespace WCF_Service
{
    [DataContract]
    public class ServerClient : IEquatable<ServerClient>
    {
        [DataMember]
        public IPEndPoint IPAddress { get; internal set; }

        [DataMember]
        public string NickName { get; internal set; }

        [DataMember]
        public List<Session> Sessions { get; set; } = new List<Session>();
        
        internal ServerClient() { }

        public override bool Equals(object obj)
        {
            return Equals(obj as ServerClient);
        }

        public bool Equals(ServerClient other)
        {
            return other != null &&
                   EqualityComparer<IPEndPoint>.Default.Equals(IPAddress, other.IPAddress) &&
                   NickName == other.NickName;
        }

        public override int GetHashCode()
        {
            int hashCode = -481975591;
            hashCode = hashCode * -1521134295 + EqualityComparer<IPEndPoint>.Default.GetHashCode(IPAddress);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(NickName);
            return hashCode;
        }

        public static bool operator ==(ServerClient left, ServerClient right)
        {
            return EqualityComparer<ServerClient>.Default.Equals(left, right);
        }

        public static bool operator !=(ServerClient left, ServerClient right)
        {
            return !(left == right);
        }
    }
}