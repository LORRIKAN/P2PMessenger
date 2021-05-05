using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Collections.Concurrent;
using System.Text;

namespace WCF_Service
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "ServiceChat" в коде и файле конфигурации.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientsCommunicationService : IClientsCommunicationService
    {
        private List<ServerClient> ServerClients { get; set; } = new List<ServerClient>();

        private List<Session> ServerSessions { get; set; } = new List<Session>();

        public ServerClient ChangeNickName(ServerClient serverClient, string newNickName)
        {
            ServerClient foundClient;

            try { foundClient = FindClient(serverClient.IPAddress); } catch { throw; }

            IEnumerable<Session> clientSessions = ServerSessions
                .Where(s => s.Clients.Any(c => c.IPAddress == foundClient.IPAddress));

            foreach (Session session in clientSessions)
            {
                foreach (ServerClient sessionClient in session.Clients)
                {
                    sessionClient.OperationContext.GetCallbackChannel<IClientCallback>()
                        .SessionClientNickNameChanged(session, foundClient, foundClient.NickName, newNickName);
                }
            }

            foundClient.NickName = newNickName;

            return foundClient;
        }

        public Session ChangeSessionPassword(Session session, string newPassword)
        {
            Session foundSession;

            try { foundSession = FindSession(session.SessionName); } catch { throw; }

            foundSession.SessionPassword = newPassword;

            foreach (ServerClient sessionClient in session.Clients)
            {
                sessionClient.OperationContext.GetCallbackChannel<IClientCallback>().SessionPasswordChanged(session);
            }

            return foundSession;
        }

        public ServerClient Connect(IPEndPoint IPAddress, string nickName)
        {
            if (ServerClients.Any(sc => sc.NickName == nickName))
                throw new ClientWithSuchNickNameExistsException("Клиент с таким именем уже существует.");

            if (ServerClients.Any(sc => sc.IPAddress == IPAddress))
                throw new ClientWithSuchIPAddressExistsException("Клиент с таким IP-адресом уже существует.");

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
            ServerClient foundClient;

            try { foundClient = FindClient(serverClient.IPAddress, serverClient.NickName); } catch { throw; }

            if (ServerSessions.Any(s => s.SessionName == sessionName))
                throw new SessionWithSuchNameExistsException("Сессия с таким именем уже существует.");

            var newSession = new Session
            {
                SessionName = sessionName,
                SessionPassword = sessionPassword,
                Creator = foundClient
            };

            newSession.ClientsInternal.Add(foundClient);

            ServerSessions.Add(newSession);

            foundClient.SessionsInternal.Add(newSession);

            return newSession;
        }

        public void DeleteSession(Session session)
        {
            Session foundSession;
            try { foundSession = FindSession(session.SessionName); } catch { return; }

            foreach (ServerClient client in session.Clients)
            {
                client.OperationContext.GetCallbackChannel<IClientCallback>().SessionDeleted(session, SessionDeletionCause.DeletedByCreator);
                client.SessionsInternal.Remove(foundSession);
            }

            ServerSessions.Remove(foundSession);
        }

        public void Disconnect(ServerClient serverClient)
        {
            ServerClient foundClient;
            try { foundClient = FindClient(serverClient.IPAddress, serverClient.NickName); } catch { return; }

            foreach (Session session in serverClient.Sessions)
                DisconnectFromSession(session, foundClient);

            ServerClients.Remove(foundClient);
        }

        public void DisconnectFromSession(Session session, ServerClient serverClient)
        {
            ServerClient foundClient;
            try { foundClient = FindClient(serverClient.IPAddress, serverClient.NickName); } catch { return; }

            ServerClient sessionCreator = session.Creator;


        }

        public IEnumerable<Session> GetSessionsList()
        {
            return ServerSessions;
        }

        public Session JoinSession(Session session, ServerClient serverClient, string sessionPassword)
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

        private void DisconnectFromSessionInternal(Session session, ServerClient serverClient)
        {
            if (session.Creator == serverClient)
        }

        private void DeleteSessionInternal(Session session, SessionDeletionCause deletionCause)
        {

        }

        private ServerClient FindClient(IPEndPoint IPAddress)
        {
            ServerClient foundClient = ServerClients.FirstOrDefault(sc => sc.IPAddress == IPAddress);

            if (foundClient == null)
                throw new EndpointNotFoundException("Не удаётся найти клиента с заданным IP-адресом.");

            return foundClient;
        }

        private ServerClient FindClient(string nickName)
        {
            ServerClient foundClient = ServerClients.FirstOrDefault(sc => sc.NickName == nickName);

            if (foundClient == null)
                throw new ArgumentException("Не удаётся найти клиента с заданным ник неймом.");

            return foundClient;
        }

        private ServerClient FindClient(IPEndPoint IPAddress, string nickName)
        {
            ServerClient clientFoundByIPAddress;
            ServerClient clientFoundByNickName;
            try
            {
                clientFoundByIPAddress = FindClient(IPAddress);
                clientFoundByNickName = FindClient(nickName);

                if (clientFoundByIPAddress.IPAddress != clientFoundByNickName.IPAddress ||
                    clientFoundByIPAddress.NickName != clientFoundByNickName.NickName)
                    throw new InvalidOperationException("Было найдено два разных клиента для заданных IP-адреса " +
                        "и ник нейма.");
            }
            catch
            {
                throw;
            }

            return clientFoundByIPAddress;
        }

        private Session FindSession(string sessionName)
        {
            Session foundSession = ServerSessions.FirstOrDefault(sc => sc.SessionName == sessionName);

            if (foundSession == null)
                throw new NoSessionWithSuchNameException("Не удаётся найти сессию с заданным именем.");

            return foundSession;
        }
    }
}