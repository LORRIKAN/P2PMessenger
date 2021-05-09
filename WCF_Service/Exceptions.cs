using System.Runtime.Serialization;

namespace WCF_Service
{
    [DataContract]
    public class ServerFault
    {
        [DataMember]
        public string Message { get; }

        public ServerFault(string message)
        {
            Message = message;
        }
    }

    [DataContract]
    [KnownType(typeof(ServerFault))]
    public class ClientWithSuchIPAddressExistsException : ServerFault
    {
        public ClientWithSuchIPAddressExistsException(string message) : base(message)
        {
        }
    }

    [DataContract]
    [KnownType(typeof(ServerFault))]
    public class ClientWithSuchNickNameExistsException : ServerFault
    {
        public ClientWithSuchNickNameExistsException(string message) : base(message)
        {
        }
    }

    [DataContract]
    [KnownType(typeof(ServerFault))]
    public class SessionWithSuchNameExistsException : ServerFault
    {
        public SessionWithSuchNameExistsException(string message) : base(message)
        {
        }
    }

    [DataContract]
    [KnownType(typeof(ServerFault))]
    public class SessionPasswordIsWrongException : ServerFault
    {
        public SessionPasswordIsWrongException(string message) : base(message)
        {
        }
    }

    [DataContract]
    [KnownType(typeof(ServerFault))]
    public class ArgumentNullException : ServerFault
    {
        public ArgumentNullException(string message) : base(message)
        {

        }
    }
}
