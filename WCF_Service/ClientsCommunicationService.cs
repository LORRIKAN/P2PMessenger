using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;

namespace WCF_Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    public class ClientsCommunicationService : IClientsCommunicationService
    {
        private Dictionary<ServerClient, OperationContext> ServerClients { get; set; } = new Dictionary<ServerClient, OperationContext>();

        private List<Session> ServerSessions { get; set; } = new List<Session>();

        public ServerFault? ChangeNickName(ServerClient serverClient, string newNickName)
        {
            try { CheckForArgumentNullException(serverClient, newNickName); }
            catch { return ServerFault.ArgumentNull_Exception; }

            foreach (Session session in serverClient.Sessions)
            {
                foreach (ServerClient sessionClient in session.Clients)
                {
                    IClientCallback callbackChannel = ServerClients[sessionClient].GetCallbackChannel<IClientCallback>();
                    callbackChannel.SessionClientNickNameChanged(session, serverClient, serverClient.NickName, newNickName);
                }
            }

            serverClient.NickName = newNickName;

            return null;
        }

        public ServerFault? ChangeSessionPassword(Session session, string newPassword)
        {
            try { CheckForArgumentNullException(session); }
            catch { return ServerFault.ArgumentNull_Exception; }

            session.SessionPassword = newPassword;

            foreach (ServerClient sessionClient in session.Clients)
            {
                IClientCallback callbackChannel = ServerClients[sessionClient].GetCallbackChannel<IClientCallback>();
                callbackChannel.SessionPasswordChanged(session);
            }

            return null;
        }

        public OperationResult<ServerClient> Connect(IPEndPoint IPAddress, string nickName)
        {
            var result = new OperationResult<ServerClient>();

            try { CheckForArgumentNullException(IPAddress, nickName); }
            catch
            {
                result.ServerFault = ServerFault.ArgumentNull_Exception;
                return result;
            }

            if (ServerClients.Keys.Any(sc => sc.NickName == nickName))
            {
                result.ServerFault = ServerFault.ServerClient_NicknameOccupied_Exception;
                return result;
            }

            if (ServerClients.Keys.Any(sc => sc.IPAddress.Equals(IPAddress)))
            {
                result.ServerFault = ServerFault.ServerClient_IPAddressOccupied_Exception;
                return result;
            }

            var serverClient = new ServerClient
            {
                IPAddress = IPAddress,
                NickName = nickName,
            };

            ServerClients[serverClient] = OperationContext.Current;

            result.Result = serverClient;

            return result;
        }

        public OperationResult<Session> CreateSession(ServerClient serverClient, string sessionName,
            string sessionPassword = null)
        {
            var result = new OperationResult<Session>();

            try { CheckForArgumentNullException(serverClient, sessionName); }
            catch
            {
                result.ServerFault = ServerFault.ArgumentNull_Exception;
                return result;
            }

            if (ServerSessions.Any(s => s.SessionName == sessionName))
            {
                result.ServerFault = ServerFault.Session_NameOccupied_Exception;
                return result;
            }

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

            result.Result = newSession;

            return result;
        }

        public ServerFault? DeleteSession(Session session, SessionDeletionCause deletionCause)
        {
            try { CheckForArgumentNullException(session); }
            catch { return ServerFault.ArgumentNull_Exception; }

            foreach (ServerClient client in session.Clients)
            {
                IClientCallback callbackChannel = ServerClients[client].GetCallbackChannel<IClientCallback>();
                callbackChannel.SessionDeleted(session, deletionCause);
                client.SessionsInternal.Remove(session);
            }

            ServerSessions.Remove(session);

            return null;
        }

        public ServerFault? Disconnect(ServerClient serverClient)
        {
            try { CheckForArgumentNullException(serverClient); }
            catch { return ServerFault.ArgumentNull_Exception; }

            foreach (Session session in serverClient.Sessions)
                DisconnectFromSession(session, serverClient);

            ServerClients.Remove(serverClient);

            return null;
        }

        public ServerFault? DisconnectFromSession(Session session, ServerClient serverClient)
        {
            try { CheckForArgumentNullException(session, serverClient); }
            catch { return ServerFault.ArgumentNull_Exception; }

            foreach (ServerClient client in session.Clients)
            {
                IClientCallback callbackChannel = ServerClients[client].GetCallbackChannel<IClientCallback>();
                callbackChannel.ClientLeftSession(session, serverClient);
            }

            serverClient.SessionsInternal.Remove(session);

            session.ClientsInternal.Remove(serverClient);

            return null;
        }

        public OperationResult<IEnumerable<Session>> GetSessionsList()
        {
            return new OperationResult<IEnumerable<Session>>(null, ServerSessions);
        }

        public ServerFault? JoinSession(Session session, ServerClient serverClient, string sessionPassword)
        {
            try { CheckForArgumentNullException(session, serverClient); }
            catch { return ServerFault.ArgumentNull_Exception; }

            Session foundSession = ServerSessions.Find(s => s.SessionName == session.SessionName);

            if (foundSession.IsPasswordRequired && foundSession.SessionPassword != sessionPassword)
                return ServerFault.WrongPassword_Exception;

            foreach (ServerClient client in session.Clients)
            {
                IClientCallback callbackChannel = ServerClients[client].GetCallbackChannel<IClientCallback>();
                callbackChannel.ClientJoinedSession(session, serverClient);
            }

            session.ClientsInternal.Add(serverClient);

            serverClient.SessionsInternal.Add(session);

            return null;
        }

        public ServerFault? RenameSession(Session session, string newName)
        {
            try { CheckForArgumentNullException(session, newName); }
            catch { return ServerFault.ArgumentNull_Exception; }

            foreach (ServerClient client in session.Clients)
            {
                IClientCallback callbackChannel = ServerClients[client].GetCallbackChannel<IClientCallback>();
                callbackChannel.SessionNameChanged(session, session.SessionName, newName);
            }

            session.SessionName = newName;

            return null;
        }

        public ServerFault? UpdateClientIPAddress(ServerClient serverClient, IPEndPoint newIPAddress)
        {
            try { CheckForArgumentNullException(serverClient, newIPAddress); }
            catch { return ServerFault.ArgumentNull_Exception; }

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

            return null;
        }

        private void CheckForArgumentNullException(params object[] args)
        {
            foreach (object arg in args)
            {
                if (arg is null)
                    throw new System.ArgumentNullException(arg.GetType().Name + " не может быть null");

                if (arg is string str)
                {
                    if (string.IsNullOrEmpty(str))
                        throw new System.ArgumentNullException(str.GetType().Name + " не может быть null или пустой");
                    if (string.IsNullOrWhiteSpace(str))
                        throw new System.ArgumentNullException(str.GetType().Name + " не может быть null или заполненной только знаками пробела");
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