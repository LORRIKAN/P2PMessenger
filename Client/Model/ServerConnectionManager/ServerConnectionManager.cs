using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.ClientsCommunicationService1;
using Client.Model.ServerConnectionManager;
using System.ServiceModel;
using System.Net;

namespace Client.Model.ServerConnectionManager
{
    class ServerConnectionManager
    {
        private ClientsCommunicationServiceClient server;
        private ServerClient serverClient;
        private List<Session> sessions;
        private Session curSession;

        public event Action<string, string> GiveClientInfo;

        public ServerConnectionManager(ClientCallback clientCallback)
        {
            server = new ClientsCommunicationServiceClient(new InstanceContext(clientCallback));            
        }

        public async Task<bool> TryConnect(IPEndPoint adress, string username)
        {
            var connectionResult = await server.ConnectAsync(adress, username);

            switch (connectionResult.ServerFault)
            {
                case null:
                    serverClient = connectionResult.Result;
                    return true;

                default:
                    return false;
            }
        }

        public async Task<string[]> GetListOfSessions()
        {
            var sessionsResult = await server.GetSessionsListAsync();

            switch (sessionsResult.ServerFault)
            {
                case null:
                    this.sessions = sessionsResult.Result.ToList();
                    string[] sessionsNames = new string[sessionsResult.Result.Length];
                    int count = 0;
                    foreach(Session s in sessionsResult.Result)
                    {
                        sessionsNames[count] = s.SessionName;
                        count++;
                    }
                    return sessionsNames;
                default:
                    return new string[] { "Ошибка при загрузке списка сессий" };
            }
        }

        public async Task<bool> CreateSession(string password)
        {
            var creationResult = await server.CreateSessionAsync(serverClient, serverClient.NickName, password);
            switch (creationResult.ServerFault)
            {
                case null:
                    curSession = creationResult.Result.Item1;
                    return true;
                default:
                    return false;
            }
        }

        public async Task<bool> JoinSession(string sessionName, string password)
        {
            Session selectedSession = (from Session s in sessions where s.SessionName == sessionName select s).Single();
            var joinResult = await server.JoinSessionAsync(selectedSession, serverClient, password);
            switch (joinResult.ServerFault)
            {
                case null:
                    curSession = selectedSession;
                    GiveClientInfo(curSession.Creator.NickName, curSession.Creator.IPAddress.ToString());
                    return true;
                default:                    
                    return false;
            }
        }

        public IPEndPoint GetAdress()
        {
            if(serverClient.NickName == curSession.Creator.NickName)
            {
                return curSession.Clients[0].IPAddress;
            }
            else
            {
                return curSession.Creator.IPAddress;
            }
        }

        public void AddSession(Session session)
        {
            sessions.Add(session);
        }

        public void ClientJoined(Session newSession)
        {
            curSession = newSession;
            for(int i = 0; i < sessions.Count; i++)
            {
                if (sessions[i].SessionName == newSession.SessionName)
                    sessions[i] = newSession;
            }
        }
    }
}
