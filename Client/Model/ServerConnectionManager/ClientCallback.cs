using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Client.ClientsCommunicationService;

namespace Client.Model.ServerConnectionManager
{
    class ClientCallback : IClientsCommunicationServiceCallback
    {
        public void ClientJoinedSession(Session session, ServerClient joinedClient)
        {
            throw new NotImplementedException();
        }

        public void ClientLeftSession(Session session, ServerClient clientLeft)
        {
            throw new NotImplementedException();
        }

        public void NewSessionCreated(Session newSession)
        {
            throw new NotImplementedException();
        }

        public void ServerShutDownNoticeReceive(long millisecondsBeforeShutDown)
        {
            throw new NotImplementedException();
        }

        public void SessionClientIPAddressChanged(Session session, ServerClient serverClient, IPEndPoint clientOldIPAddress, IPEndPoint clientNewIPAddress)
        {
            throw new NotImplementedException();
        }

        public void SessionClientNickNameChanged(Session session, ServerClient serverClient, string clientOldNickName, string clientNewNickName)
        {
            throw new NotImplementedException();
        }

        public void SessionDeleted(ServerClient serverClient, Session session, SessionDeletionCause sessionDeletionCause)
        {
            throw new NotImplementedException();
        }

        public void SessionNameChanged(Session session, string sessionOldName, string sessionNewName)
        {
            throw new NotImplementedException();
        }

        public void SessionPasswordChanged(Session session)
        {
            throw new NotImplementedException();
        }
    }
}
