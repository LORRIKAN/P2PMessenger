using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WCF_Service
{
    [DataContract]
    public class ServerException
    {
        [DataMember]
        public string Message { get; }

        public ServerException(string message)
        {
            Message = message;
        }
    }

    [DataContract]
    [KnownType(typeof(ServerException))]
    public class ClientWithSuchIPAddressExistsException : ServerException
    {
        public ClientWithSuchIPAddressExistsException(string message) : base(message)
        {
        }
    }

    [DataContract]
    [KnownType(typeof(ServerException))]
    public class ClientWithSuchNickNameExistsException : ServerException
    {
        public ClientWithSuchNickNameExistsException(string message) : base(message)
        {
        }
    }

    [DataContract]
    [KnownType(typeof(ServerException))]
    public class SessionWithSuchNameExistsException : ServerException
    {
        public SessionWithSuchNameExistsException(string message) : base(message)
        {
        }
    }

    [DataContract]
    [KnownType(typeof(ServerException))]
    public class SessionPasswordIsWrongException : ServerException 
    {
        public SessionPasswordIsWrongException(string message) : base(message)
        {
        }
    }
}
