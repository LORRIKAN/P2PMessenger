using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;

namespace WCF_Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ClientsCommunicationService : IClientsCommunicationService
    {
        private Dictionary<ServerClient, OperationContext> ServerClients { get; set; } = new Dictionary<ServerClient, OperationContext>();

        private List<Session> ServerSessions { get; set; } = new List<Session>();

        public OperationResult<ServerClient> ChangeNickName(ServerClient serverClient, string newNickName)
        {
            OperationResult<ServerClient> result = CheckForArgumentNullException<ServerClient>(serverClient, newNickName);

            if (!(result.ServerFault is null))
                return result;

            serverClient = FindClient(serverClient);

            string oldNickName = serverClient.NickName;

            serverClient.NickName = newNickName;

            foreach (Session session in serverClient.Sessions.ToArray())
            {
                foreach (ServerClient sessionClient in session.Clients.ToArray())
                {
                    IClientCallback callbackChannel = ServerClients[sessionClient].GetCallbackChannel<IClientCallback>();
                    Task.Run(() => {
                    try
                    {
                        callbackChannel.SessionClientNickNameChanged(session, serverClient, oldNickName, newNickName);
                    }
                    catch { } });
                }
            }

            result.Result = serverClient;

            return result;
        }

        public OperationResult<Session> ChangeSessionPassword(Session session, string newPassword)
        {
            OperationResult<Session> result = CheckForArgumentNullException<Session>(session, newPassword);

            if (!(result.ServerFault is null))
                return result;

            session = FindSession(session);
            session.SessionPassword = newPassword;

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrWhiteSpace(newPassword))
                session.IsPasswordRequired = false;
            else
                session.IsPasswordRequired = true;

            foreach (ServerClient sessionClient in session.Clients.ToArray())
            {
                IClientCallback callbackChannel = ServerClients[sessionClient].GetCallbackChannel<IClientCallback>();
                Task.Run(() => { try { callbackChannel.SessionPasswordChanged(session); } catch { } });
            }

            result.Result = session;

            return result;
        }

        public OperationResult<ServerClient> Connect(IPEndPoint IPAddress, string nickName)
        {
            OperationResult<ServerClient> result = CheckForArgumentNullException<ServerClient>(IPAddress, nickName);

            if (!(result.ServerFault is null))
                return result;

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

        public OperationResult<Tuple<Session, ServerClient>> CreateSession(ServerClient serverClient, string sessionName,
            string sessionPassword = null)
        {
            OperationResult<Tuple<Session, ServerClient>> result = 
                CheckForArgumentNullException<Tuple<Session, ServerClient>>(serverClient, sessionName);

            if (!(result.ServerFault is null))
                return result;

            if (ServerSessions.Any(s => s.SessionName == sessionName))
            {
                result.ServerFault = ServerFault.Session_NameOccupied_Exception;
                return result;
            }

            var newSession = new Session
            {
                SessionName = sessionName,
                SessionPassword = sessionPassword,
                Creator = serverClient,
                IsPasswordRequired = !string.IsNullOrEmpty(sessionPassword) && 
                !string.IsNullOrWhiteSpace(sessionPassword)
            };

            serverClient = FindClient(serverClient);

            newSession.Clients.Add(serverClient);

            serverClient.Sessions.Add(newSession);

            ServerSessions.Add(newSession);

            foreach (ServerClient client in ServerClients.Keys.ToArray())
            {
                IClientCallback callbackChannel = ServerClients[client].GetCallbackChannel<IClientCallback>();
                Task.Run(() => { try { callbackChannel.NewSessionCreated(newSession); } catch { } });
            }

            result.Result = new Tuple<Session, ServerClient>(newSession, serverClient);

            return result;
        }

        public OperationResult<ServerClient> DeleteSession(ServerClient serverClient, Session session, SessionDeletionCause deletionCause)
        {
            OperationResult<ServerClient> result =
                CheckForArgumentNullException<ServerClient>(session);

            if (!(result.ServerFault is null))
                return result;

            session = FindSession(session);

            serverClient = FindClient(serverClient);

            foreach (ServerClient client in session.Clients.ToArray())
            {
                IClientCallback callbackChannel = ServerClients[client].GetCallbackChannel<IClientCallback>();
                client.Sessions.Remove(session);
                Task.Run(() => { try { callbackChannel.SessionDeleted(serverClient, session, deletionCause); } catch { } });
            }

            ServerSessions.Remove(session);

            result.Result = serverClient;

            return result;
        }

        public ServerFault? Disconnect(ServerClient serverClient)
        {
            OperationResult<ServerClient> result =
                CheckForArgumentNullException<ServerClient>(serverClient);

            if (!(result.ServerFault is null))
                return result.ServerFault;

            serverClient = FindClient(serverClient);

            foreach (Session session in ServerSessions.Where(s => s.Clients.Contains(serverClient)))
                DisconnectFromSession(session, serverClient);

            ServerClients.Remove(serverClient);

            return null;
        }

        public OperationResult<Tuple<Session, ServerClient>> DisconnectFromSession(Session session, ServerClient serverClient)
        {
            OperationResult<Tuple<Session, ServerClient>> result =
                CheckForArgumentNullException<Tuple<Session, ServerClient>>(session, serverClient);

            if (!(result.ServerFault is null))
                return result;

            session = FindSession(session);

            serverClient = FindClient(serverClient);

            serverClient.Sessions.Remove(session);

            session.Clients.Remove(serverClient);
            
            foreach (ServerClient client in session.Clients.ToArray())
            {
                IClientCallback callbackChannel = ServerClients[client].GetCallbackChannel<IClientCallback>();
                Task.Run(() => { try { callbackChannel.ClientLeftSession(session, serverClient); } catch { } });
            }

            result.Result = new Tuple<Session, ServerClient>(session, serverClient);

            return result;
        }

        public OperationResult<IEnumerable<Session>> GetSessionsList()
        {
            return new OperationResult<IEnumerable<Session>>(null, ServerSessions);
        }

        public OperationResult<Tuple<Session, ServerClient>> JoinSession(Session session, ServerClient serverClient, 
            string sessionPassword)
        {
            OperationResult<Tuple<Session, ServerClient>> result =
                CheckForArgumentNullException<Tuple<Session, ServerClient>>(session, serverClient);

            if (!(result.ServerFault is null))
                return result;

            session = FindSession(session);

            serverClient = FindClient(serverClient);

            if (session.IsPasswordRequired && session.SessionPassword != sessionPassword)
            {
                result.ServerFault = ServerFault.WrongPassword_Exception;
                return result;
            }


            foreach (ServerClient client in session.Clients.ToArray())
            {
                IClientCallback callbackChannel = ServerClients[client].GetCallbackChannel<IClientCallback>();
                Task.Run(() => { try { callbackChannel.ClientJoinedSession(session, serverClient); } catch { } });
            }

            session.Clients.Add(serverClient);

            serverClient.Sessions.Add(session);

            result.Result = new Tuple<Session, ServerClient>(session, serverClient);

            return result;
        }

        public OperationResult<Session> RenameSession(Session session, string newName)
        {
            OperationResult<Session> result =
                CheckForArgumentNullException<Session>(session, newName);

            if (!(result.ServerFault is null))
                return result;

            session = FindSession(session);

            string oldName = session.SessionName;

            session.SessionName = newName;

            foreach (ServerClient client in session.Clients.ToArray())
            {
                IClientCallback callbackChannel = ServerClients[client].GetCallbackChannel<IClientCallback>();
                Task.Run(() => { try { callbackChannel.SessionNameChanged(session, oldName, newName); } catch { } });
            }

            result.Result = session;

            return result;
        }

        public OperationResult<ServerClient> UpdateClientIPAddress(ServerClient serverClient, IPEndPoint newIPAddress)
        {
            OperationResult<ServerClient> result =
                CheckForArgumentNullException<ServerClient>(serverClient, newIPAddress);

            if (!(result.ServerFault is null))
                return result;

            serverClient = FindClient(serverClient);

            IPEndPoint oldIPAddress = serverClient.IPAddress;

            serverClient.IPAddress = newIPAddress;

            ServerClients[serverClient] = OperationContext.Current;

            if (oldIPAddress != newIPAddress)
            {
                foreach (Session session in serverClient.Sessions.ToArray())
                {
                    foreach (ServerClient client in session.Clients.ToArray())
                    {
                        IClientCallback callbackChannel = ServerClients[client].GetCallbackChannel<IClientCallback>();
                        Task.Run(() =>
                        {
                            try
                            {
                                callbackChannel.SessionClientIPAddressChanged(session, serverClient, oldIPAddress, newIPAddress);
                            }
                            catch { }
                        });
                    }
                }
            }

            result.Result = serverClient;

            return result;
        }

        private OperationResult<T> CheckForArgumentNullException<T>(params object[] args)
        {
            var result = new OperationResult<T>();

            foreach (object arg in args)
            {
                if (arg is null)
                {
                    result.ServerFault = ServerFault.ArgumentNull_Exception;
                    return result;
                }

                if (arg is string str)
                {
                    if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
                    {
                        result.ServerFault = ServerFault.ArgumentNull_Exception;
                        return result;
                    }
                }
            }

            return result;
        }

        private ServerClient FindClient(ServerClient serverClient)
        {
            return ServerClients.Keys.FirstOrDefault(sc => sc == serverClient);
        }

        private Session FindSession(Session session)
        {
            return ServerSessions.Find(s => s == session);
        }
    }
}