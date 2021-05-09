using System.Runtime.Serialization;

namespace WCF_Service
{
    [DataContract]
    public struct OperationResult<T>
    {
        public OperationResult(ServerFault? serverException, T result)
        {
            ServerFault = serverException;
            Result = result;
        }

        [DataMember]
        public ServerFault? ServerFault { get; set; }

        [DataMember]
        public T Result { get; set; }
    }
}