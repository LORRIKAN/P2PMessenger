using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WCF_Service
{
    [ServiceContract(CallbackContract = typeof(IClientCallback))]
    public interface IClientsCommunicationService
    {
        [OperationContract]
        [FaultContract(typeof(ClientWithSuchNickNameExistsException))]
        [FaultContract(typeof(ClientWithSuchIPAddressExistsException))]
        [FaultContract(typeof(ArgumentNullException))]
        ServerClient Connect(IPEndPoint IPAddress, string nickName);

        [OperationContract]
        [FaultContract(typeof(ArgumentNullException))]
        void UpdateClientIPAddress(ServerClient serverClient, IPEndPoint newIPAddress);

        [OperationContract]
        [FaultContract(typeof(ArgumentNullException))]
        void ChangeNickName(ServerClient serverClient, string nickName);

        [OperationContract]
        [FaultContract(typeof(ArgumentNullException))]
        void Disconnect(ServerClient serverClient);

        [OperationContract]
        [FaultContract(typeof(ArgumentNullException))]
        [FaultContract(typeof(SessionWithSuchNameExistsException))]
        Session CreateSession(ServerClient serverClient, string sessionName, string sessionPassword = null);

        [OperationContract]
        [FaultContract(typeof(ArgumentNullException))]
        void RenameSession(Session session, string newName);

        [OperationContract]
        [FaultContract(typeof(ArgumentNullException))]
        void ChangeSessionPassword(Session session, string newPassword);

        [OperationContract]
        IEnumerable<Session> GetSessionsList();

        [OperationContract]
        [FaultContract(typeof(ArgumentNullException))]
        void DeleteSession(Session session, SessionDeletionCause deletionCause);

        [OperationContract]
        [FaultContract(typeof(ArgumentNullException))]
        void DisconnectFromSession(Session session, ServerClient serverClient);

        [OperationContract]
        [FaultContract(typeof(SessionPasswordIsWrongException))]
        [FaultContract(typeof(ArgumentNullException))]
        void JoinSession(Session session, ServerClient serverClient, string sessionPassword);
    }

    public interface IClientCallback
    {
        [OperationContract]
        void NewSessionCreated(Session newSession);

        [OperationContract]
        void ClientJoinedSession(Session session, ServerClient joinedClient);

        [OperationContract]
        void ClientLeftSession(Session session, ServerClient clientLeft);

        [OperationContract]
        void SessionNameChanged(Session session, string sessionOldName, string sessionNewName);

        [OperationContract]
        void SessionPasswordChanged(Session session);

        [OperationContract]
        void SessionClientNickNameChanged(Session session, ServerClient serverClient, string clientOldNickName,
            string clientNewNickName);

        [OperationContract]
        void SessionClientIPAddressChanged(Session session, ServerClient serverClient,
            IPEndPoint clientOldIPAddress, IPEndPoint clientNewIPAddress);

        [OperationContract]
        void SessionDeleted(Session session, SessionDeletionCause sessionDeletionCause);

        [OperationContract]
        void ServerShutDownNoticeReceive(long millisecondsBeforeShutDown);
    }

    [DataContract]
    public class SessionDeletionCause
    {
        [DataMember]
        public string Message { get; set; }
    }
}