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

        public void ChangeNickName(ServerClient serverClient, string newNickName)
        {
            foreach (Session session in serverClient.Sessions)
            {
                foreach (ServerClient sessionClient in session.Clients)
                {
                    sessionClient.OperationContext.GetCallbackChannel<IClientCallback>()
                        .SessionClientNickNameChanged(session, serverClient, serverClient.NickName, newNickName);
                }
            }

            serverClient.NickName = newNickName;
        }

        public void ChangeSessionPassword(Session session, string newPassword)
        {
            session.SessionPassword = newPassword;

            foreach (ServerClient sessionClient in session.Clients)
            {
                sessionClient.OperationContext.GetCallbackChannel<IClientCallback>().SessionPasswordChanged(session);
            }
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
            if (ServerSessions.Any(s => s.SessionName == sessionName))
                throw new SessionWithSuchNameExistsException("Сессия с таким именем уже существует.");

            var newSession = new Session
            {
                SessionName = sessionName,
                SessionPassword = sessionPassword,
                Creator = serverClient
            };

            newSession.ClientsInternal.Add(serverClient);

            ServerSessions.Add(newSession);

            serverClient.SessionsInternal.Add(newSession);

            foreach (ServerClient client in ServerClients)
            {
                client.OperationContext.GetCallbackChannel<IClientCallback>().NewSessionCreated(newSession);
            }

            return newSession;
        }

        public void DeleteSession(Session session, SessionDeletionCause deletionCause)
        {
            foreach (ServerClient client in session.Clients)
            {
                client.OperationContext.GetCallbackChannel<IClientCallback>().SessionDeleted(session, deletionCause);
                client.SessionsInternal.Remove(session);
            }

            ServerSessions.Remove(session);
        }

        public void Disconnect(ServerClient serverClient)
        {
            foreach (Session session in serverClient.Sessions)
                DisconnectFromSession(session, serverClient);

            ServerClients.Remove(serverClient);
        }

        public void DisconnectFromSession(Session session, ServerClient serverClient)
        {
            foreach (ServerClient client in session.Clients)
            {
                client.OperationContext.GetCallbackChannel<IClientCallback>().ClientLeftSession(session, serverClient);
            }

            serverClient.SessionsInternal.Remove(session);

            session.ClientsInternal.Remove(serverClient);
        }

        public IEnumerable<Session> GetSessionsList()
        {
            return ServerSessions;
        }

        public void JoinSession(Session session, ServerClient serverClient, string sessionPassword)
        {
            if (session.IsPasswordRequired && sessionPassword != session.SessionPassword)
                throw new SessionPasswordIsWrongException("Указанный пароль при попытке присоединения к " +
                    "сессии является неверным.");

            foreach (ServerClient client in session.Clients)
            {
                client.OperationContext.GetCallbackChannel<IClientCallback>().ClientJoinedSession(session, serverClient);
            }

            session.ClientsInternal.Add(serverClient);

            serverClient.SessionsInternal.Add(session);
        }

        public void RenameSession(Session session, string newName)
        {
            foreach (ServerClient client in session.Clients)
            {
                client.OperationContext.GetCallbackChannel<IClientCallback>().SessionNameChanged(session, session.SessionName, newName);
            }

            session.SessionName = newName;
        }

        public void UpdateClientIPAddress(ServerClient serverClient, IPEndPoint newIPAddress)
        {
            if (serverClient.IPAddress != newIPAddress)
            {
                foreach (Session session in serverClient.Sessions)
                {
                    foreach (ServerClient client in session.Clients)
                    {
                        client.OperationContext.GetCallbackChannel<IClientCallback>()
                            .SessionClientIPAddressChanged(session, serverClient, serverClient.IPAddress, newIPAddress);
                    }
                }
            }

            serverClient.IPAddress = newIPAddress;
        }

        private ServerClient FindClient(IPEndPoint IPAddress)
        {
            ServerClient foundClient = ServerClients.FirstOrDefault(sc => sc.IPAddress == IPAddress);

            if (foundClient == null)
                throw new NoClientWithSuchIPAddressException("Не удаётся найти клиента с заданным IP-адресом.");

            return foundClient;
        }

        private ServerClient FindClient(string nickName)
        {
            ServerClient foundClient = ServerClients.FirstOrDefault(sc => sc.NickName == nickName);

            if (foundClient == null)
                throw new NoClientWithSuchNickNameAndIPAddressException("Не удаётся найти клиента с заданным ник неймом.");

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