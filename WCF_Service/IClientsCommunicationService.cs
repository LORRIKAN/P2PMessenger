﻿using System.Collections.Generic;
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
        ServerFault? UpdateClientIPAddress(ServerClient serverClient, IPEndPoint newIPAddress);

        [OperationContract]
        ServerFault? ChangeNickName(ServerClient serverClient, string nickName);

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
        OperationResult<Session> CreateSession(ServerClient serverClient, string sessionName,
            string sessionPassword = null);

        [OperationContract]
        ServerFault? RenameSession(Session session, string newName);

        [OperationContract]
        ServerFault? ChangeSessionPassword(Session session, string newPassword);

        [OperationContract]
        OperationResult<IEnumerable<Session>> GetSessionsList();

        [OperationContract]
        ServerFault? DeleteSession(Session session, SessionDeletionCause deletionCause);

        [OperationContract]
        ServerFault? DisconnectFromSession(Session session, ServerClient serverClient);

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
        ServerFault? JoinSession(Session session, ServerClient serverClient, string sessionPassword);
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