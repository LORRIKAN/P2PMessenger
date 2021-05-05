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
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientsCommunicationService : IClientsCommunicationService
    {
        private Dictionary<ServerClient, OperationContext> ServerClients { get; set; } = new Dictionary<ServerClient, OperationContext>();

        private List<Session> ServerSessions { get; set; } = new List<Session>();

        public void ChangeNickName(ServerClient serverClient, string newNickName)
        {
            try { CheckForArgumentNullException(serverClient, newNickName); }
            catch(ArgumentNullException ex) { throw new FaultException<ArgumentNullException>(ex); }

            foreach (Session session in serverClient.Sessions)
            {
                foreach (ServerClient sessionClient in session.Clients)
                {
                    IClientCallback callbackChannel = ServerClients[sessionClient].GetCallbackChannel<IClientCallback>();
                    callbackChannel.SessionClientNickNameChanged(session, serverClient, serverClient.NickName, newNickName);
                }
            }

            serverClient.NickName = newNickName;
        }

        public void ChangeSessionPassword(Session session, string newPassword)
        {
            try { CheckForArgumentNullException(session); }
            catch(ArgumentNullException ex) { throw new FaultException<ArgumentNullException>(ex); }

            session.SessionPassword = newPassword;

            foreach (ServerClient sessionClient in session.Clients)
            {
                IClientCallback callbackChannel = ServerClients[sessionClient].GetCallbackChannel<IClientCallback>();
                callbackChannel.SessionPasswordChanged(session);
            }
        }

        public ServerClient Connect(IPEndPoint IPAddress, string nickName)
        {
            try { CheckForArgumentNullException(IPAddress, nickName); }
            catch (ArgumentNullException ex) { throw new FaultException<ArgumentNullException>(ex); }

            if (ServerClients.Keys.Any(sc => sc.NickName == nickName))
                throw new FaultException<ClientWithSuchNickNameExistsException>(
                    new ClientWithSuchNickNameExistsException("Клиент с таким именем уже существует."));

            if (ServerClients.Keys.Any(sc => sc.IPAddress == IPAddress))
                throw new FaultException<ClientWithSuchIPAddressExistsException>(
                    new ClientWithSuchIPAddressExistsException("Клиент с таким IP-адресом уже существует."));

            var serverClient = new ServerClient
            {
                IPAddress = IPAddress,
                NickName = nickName,
            };

            ServerClients[serverClient] = OperationContext.Current;

            return serverClient;
        }

        public Session CreateSession(ServerClient serverClient, string sessionName, string sessionPassword = null)
        {
            try { CheckForArgumentNullException(serverClient, sessionName); }
            catch (ArgumentNullException ex) { throw new FaultException<ArgumentNullException>(ex); }

            if (ServerSessions.Any(s => s.SessionName == sessionName))
                throw new FaultException<SessionWithSuchNameExistsException>(
                    new SessionWithSuchNameExistsException("Сессия с таким именем уже существует."));

            var newSession = new Session
            {
                SessionName = sessionName,
                SessionPassword = sessionPassword,
                Creator = serverClient
            };

            newSession.ClientsInternal.Add(serverClient);

            ServerSessions.Add(newSession);

            serverClient.SessionsInternal.Add(newSession);

            foreach (ServerClient client in ServerClients.Keys)
            {
                IClientCallback callbackChannel = ServerClients[client].GetCallbackChannel<IClientCallback>();
                callbackChannel.NewSessionCreated(newSession);
            }

            return newSession;
        }

        public void DeleteSession(Session session, SessionDeletionCause deletionCause)
        {
            try { CheckForArgumentNullException(session); }
            catch (ArgumentNullException ex) { throw new FaultException<ArgumentNullException>(ex); }

            foreach (ServerClient client in session.Clients)
            {
                IClientCallback callbackChannel = ServerClients[client].GetCallbackChannel<IClientCallback>();
                callbackChannel.SessionDeleted(session, deletionCause);
                client.SessionsInternal.Remove(session);
            }

            ServerSessions.Remove(session);
        }

        public void Disconnect(ServerClient serverClient)
        {
            try { CheckForArgumentNullException(serverClient); }
            catch (ArgumentNullException ex) { throw new FaultException<ArgumentNullException>(ex); }

            foreach (Session session in serverClient.Sessions)
                DisconnectFromSession(session, serverClient);

            ServerClients.Remove(serverClient);
        }

        public void DisconnectFromSession(Session session, ServerClient serverClient)
        {
            try { CheckForArgumentNullException(session, serverClient); }
            catch (ArgumentNullException ex) { throw new FaultException<ArgumentNullException>(ex); }

            foreach (ServerClient client in session.Clients)
            {
                IClientCallback callbackChannel = ServerClients[client].GetCallbackChannel<IClientCallback>();
                callbackChannel.ClientLeftSession(session, serverClient);
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
            try { CheckForArgumentNullException(session, serverClient); }
            catch (ArgumentNullException ex) { throw new FaultException<ArgumentNullException>(ex); }

            if (session.IsPasswordRequired && sessionPassword != session.SessionPassword)
                throw new FaultException<SessionPasswordIsWrongException>(
                    new SessionPasswordIsWrongException("Указанный пароль при попытке присоединения к " +
                    "сессии является неверным."));

            foreach (ServerClient client in session.Clients)
            {
                IClientCallback callbackChannel = ServerClients[client].GetCallbackChannel<IClientCallback>();
                callbackChannel.ClientJoinedSession(session, serverClient);
            }

            session.ClientsInternal.Add(serverClient);

            serverClient.SessionsInternal.Add(session);
        }

        public void RenameSession(Session session, string newName)
        {
            try { CheckForArgumentNullException(session, newName); }
            catch (ArgumentNullException ex) { throw new FaultException<ArgumentNullException>(ex); }

            foreach (ServerClient client in session.Clients)
            {
                IClientCallback callbackChannel = ServerClients[client].GetCallbackChannel<IClientCallback>();
                callbackChannel.SessionNameChanged(session, session.SessionName, newName);
            }

            session.SessionName = newName;
        }

        public void UpdateClientIPAddress(ServerClient serverClient, IPEndPoint newIPAddress)
        {
            try { CheckForArgumentNullException(serverClient, newIPAddress); }
            catch (ArgumentNullException ex) { throw new FaultException<ArgumentNullException>(ex); }

            if (serverClient.IPAddress != newIPAddress)
            {
                foreach (Session session in serverClient.Sessions)
                {
                    foreach (ServerClient client in session.Clients)
                    {
                        IClientCallback callbackChannel = ServerClients[client].GetCallbackChannel<IClientCallback>();
                        callbackChannel.SessionClientIPAddressChanged(session, serverClient, serverClient.IPAddress, newIPAddress);
                    }
                }
            }

            serverClient.IPAddress = newIPAddress;
        }

        private void CheckForArgumentNullException(params object[] args)
        {
            foreach (object arg in args)
            {
                if (arg is null)
                    throw new ArgumentNullException(arg.GetType().Name + " не может быть null");

                if (arg is string str)
                {
                    if (string.IsNullOrEmpty(str))
                        throw new ArgumentNullException(str.GetType().Name + " не может быть null или пустой");
                    if (string.IsNullOrWhiteSpace(str))
                        throw new ArgumentNullException(str.GetType().Name + " не может быть null или заполненной только знаками пробела");
                }
            }
        }

        //private ServerClient FindClient(IPEndPoint IPAddress)
        //{
        //    ServerClient foundClient = ServerClients.FirstOrDefault(sc => sc.IPAddress == IPAddress);

        //    if (foundClient == null)
        //        throw new NoClientWithSuchIPAddressException("Не удаётся найти клиента с заданным IP-адресом.");

        //    return foundClient;
        //}

        //private ServerClient FindClient(string nickName)
        //{
        //    ServerClient foundClient = ServerClients.FirstOrDefault(sc => sc.NickName == nickName);

        //    if (foundClient == null)
        //        throw new NoClientWithSuchNickNameAndIPAddressException("Не удаётся найти клиента с заданным ник неймом.");

        //    return foundClient;
        //}

        //private ServerClient FindClient(IPEndPoint IPAddress, string nickName)
        //{
        //    ServerClient clientFoundByIPAddress;
        //    ServerClient clientFoundByNickName;
        //    try
        //    {
        //        clientFoundByIPAddress = FindClient(IPAddress);
        //        clientFoundByNickName = FindClient(nickName);

        //        if (clientFoundByIPAddress.IPAddress != clientFoundByNickName.IPAddress ||
        //            clientFoundByIPAddress.NickName != clientFoundByNickName.NickName)
        //            throw new InvalidOperationException("Было найдено два разных клиента для заданных IP-адреса " +
        //                "и ник нейма.");
        //    }
        //    catch
        //    {
        //        throw;
        //    }

        //    return clientFoundByIPAddress;
        //}

        //private Session FindSession(string sessionName)
        //{
        //    Session foundSession = ServerSessions.FirstOrDefault(sc => sc.SessionName == sessionName);

        //    if (foundSession == null)
        //        throw new NoSessionWithSuchNameException("Не удаётся найти сессию с заданным именем.");

        //    return foundSession;
        //}
    }
}