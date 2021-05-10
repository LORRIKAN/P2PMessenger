using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.ClientsCommunicationService;
using Client.Model.ServerConnectionManager;
using System.ServiceModel;
using System.Net;

namespace Client.Model.ServerConnectionManager
{
    class ServerConnectionManager
    {
        private ClientsCommunicationServiceClient server;
        private ServerClient serverClient;
        private Session[] sessions;
        private Session curSession;

        public ServerConnectionManager()
        {
            server = new ClientsCommunicationServiceClient(new InstanceContext(new ClientCallback()));
        }

        public bool TryConnect(IPEndPoint adress, string username)
        {
            var connectionResult = server.Connect(adress, username);

            switch (connectionResult.ServerFault)
            {
                case null:
                    serverClient = connectionResult.Result;
                    return true;

                default:
                    return false;
            }
        }

        public string[] GetListOfSessions()
        {
            var sessionsResult = server.GetSessionsList();

            switch (sessionsResult.ServerFault)
            {
                case null:
                    this.sessions = sessionsResult.Result;
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
    }
}
