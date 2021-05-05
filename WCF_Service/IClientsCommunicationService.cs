﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WCF_Service
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени интерфейса "IServiceChat" в коде и файле конфигурации.
    [ServiceContract(CallbackContract = typeof(IClientCallback))]
    public interface IClientsCommunicationService
    {
        [OperationContract]
        ServerClient Connect(IPEndPoint IPAddress, string nickName);

        [OperationContract]
        void UpdateClientIPAddress(ServerClient serverClient, IPEndPoint newIPAddress);

        [OperationContract]
        void ChangeNickName(ServerClient serverClient, string nickName);

        [OperationContract]
        void Disconnect(ServerClient serverClient);

        [OperationContract]
        Session CreateSession(ServerClient serverClient, string sessionName, string sessionPassword = null);

        [OperationContract]
        void RenameSession(Session session, string newName);

        [OperationContract]
        void ChangeSessionPassword(Session session, string newPassword);

        [OperationContract]
        IEnumerable<Session> GetSessionsList();

        [OperationContract]
        void DeleteSession(Session session, SessionDeletionCause deletionCause);

        [OperationContract]
        void DisconnectFromSession(Session session, ServerClient serverClient);

        [OperationContract]
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
    public class NoClientWithSuchIPAddressException : Exception
    {
        public NoClientWithSuchIPAddressException(string message) : base(message)
        {

        }
    }

    [DataContract]
    public class NoClientWithSuchNickNameAndIPAddressException : Exception
    {
        public NoClientWithSuchNickNameAndIPAddressException(string message) : base(message)
        {

        }
    }

    [DataContract]
    public class ClientWithSuchIPAddressExistsException : Exception
    {
        public ClientWithSuchIPAddressExistsException(string message) : base(message)
        {

        }
    }

    [DataContract]
    public class ClientWithSuchNickNameExistsException : Exception
    {
        public ClientWithSuchNickNameExistsException(string message) : base(message)
        {

        }
    }

    [DataContract]
    public class NoSessionWithSuchNameException : Exception
    {
        public NoSessionWithSuchNameException(string message) : base(message)
        {

        }
    }

    [DataContract]
    public class SessionWithSuchNameExistsException : Exception
    {
        public SessionWithSuchNameExistsException(string message) : base(message)
        {

        }
    }

    [DataContract]
    public class SessionPasswordIsWrongException : Exception
    {
        public SessionPasswordIsWrongException(string message) : base(message)
        {

        }
    }

    [DataContract]
    public class SessionDeletionCause { }
}