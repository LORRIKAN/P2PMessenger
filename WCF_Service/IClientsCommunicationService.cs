using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace WCF_Service
{
    [ServiceContract(CallbackContract = typeof(IClientCallback))]
    public interface IClientsCommunicationService
    {
        /// <summary>
        /// Исключения: 
        /// ArgumentNull_Exception, 
        /// ServerClient_NicknameOccupied_Exception, 
        /// ServerClient_IPAddressOccupied_Exception.
        /// </summary>
        /// <param name="IPAddress"></param>
        /// <param name="nickName"></param>
        /// <returns></returns>
        [OperationContract]
        OperationResult<ServerClient> Connect(IPEndPoint IPAddress, string nickName);

        [OperationContract]
        OperationResult<ServerClient> UpdateClientIPAddress(ServerClient serverClient, IPEndPoint newIPAddress);

        [OperationContract]
        OperationResult<ServerClient> ChangeNickName(ServerClient serverClient, string nickName);

        [OperationContract]
        ServerFault? Disconnect(ServerClient serverClient);

        /// <summary>
        /// Исключения:
        /// ArgumentNull_Exception,
        /// Session_NameOccupied_Exception.
        /// </summary>
        /// <param name="serverClient"></param>
        /// <param name="sessionName"></param>
        /// <param name="sessionPassword"></param>
        /// <returns></returns>
        [OperationContract]
        OperationResult<Tuple<Session, ServerClient>> CreateSession(ServerClient serverClient, string sessionName,
            string sessionPassword = null);

        [OperationContract]
        OperationResult<Session> RenameSession(Session session, string newName);

        [OperationContract]
        OperationResult<Session> ChangeSessionPassword(Session session, string newPassword);

        [OperationContract]
        OperationResult<IEnumerable<Session>> GetSessionsList();

        [OperationContract]
        OperationResult<ServerClient> DeleteSession(ServerClient serverClient,Session session, SessionDeletionCause deletionCause);

        [OperationContract]
        OperationResult<Tuple<Session, ServerClient>> DisconnectFromSession(Session session, ServerClient serverClient);

        /// <summary>
        /// Исключения:
        /// ArgumentNull_Exception,
        /// WrongPassword_Exception
        /// </summary>
        /// <param name="session"></param>
        /// <param name="serverClient"></param>
        /// <param name="sessionPassword"></param>
        /// <returns></returns>
        [OperationContract]
        OperationResult<Tuple<Session, ServerClient>> JoinSession(Session session, ServerClient serverClient, string sessionPassword);
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
        void SessionDeleted(ServerClient serverClient, Session session, SessionDeletionCause sessionDeletionCause);

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