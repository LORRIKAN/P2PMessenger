using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WCF_Service
{
    [DataContract]
    public class Session : IEquatable<Session>
    {
        [DataMember]
        public string SessionName { get; internal set; }

        private bool isPasswordRequired;
        [DataMember]
        public bool IsPasswordRequired { 
            get { isPasswordRequired = !string.IsNullOrEmpty(SessionPassword); return isPasswordRequired; } 
            set => isPasswordRequired = value; }

        [DataMember]
        public ServerClient Creator { get; internal set; }

        [DataMember]
        public List<ServerClient> Clients { get; set; } = new List<ServerClient>();
        
        internal Session() { }

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