using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WCF_Service
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "ServiceChat" в коде и файле конфигурации.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientsCommunicationService : IClientsCommunicationService
    {
        private List<ServerClient> ServerClients { get; set; } = new List<ServerClient>();

        private List<Session> ServerSessions { get; set; } = new List<Session>();

        public ServerClient ChangeNickName(ServerClient serverClient, string nickName)
        {
            throw new NotImplementedException();
        }

        public Session ChangeSessionPassword(Session session, string newPassword)
        {
            throw new NotImplementedException();
        }

        public ServerClient Connect(IPEndPoint IPAddress, string nickName)
        {
            var serverClient = new ServerClient
            {
                IPAddress = IPAddress,
                NickName = nickName,
                OperationContext = OperationContext.Current
            };

            ServerClients.Add(serverClient);

            return serverClient;
        }

        public Session CreateSession(ServerClient serverClient, string sessionName, string sessionPassword = null)
        {
            throw new NotImplementedException();
        }

        public void DeleteSession(Session session)
        {
            throw new NotImplementedException();
        }

        public void Disconnect(ServerClient serverClient)
        {
            ServerClients.Remove(serverClient);
        }

        public void DisconnectFromSession(Session session, ServerClient serverClient)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Session> GetSessionsList()
        {
            throw new NotImplementedException();
        }

        public Session JoinSession(ServerClient serverClient, string sessionName, string sessionPassword)
        {
            throw new NotImplementedException();
        }

        public Session JoinSession(ServerClient serverClient, Session session, string sessionPassword)
        {
            throw new NotImplementedException();
        }

        public Session RenameSession(Session session, string newName)
        {
            throw new NotImplementedException();
        }

        public ServerClient UpdateClientIPAddress(ServerClient serverClient, IPEndPoint newIPAddress)
        {
            throw new NotImplementedException();
        }
    }
}