using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Model.p2pConnectionManager;
using Client.Model.ServerConnectionManager;
using System.Net;
using System.Net.Sockets;
using Client.ClientsCommunicationService;

namespace Client.Model
{
    class Model
    {
        p2pConnectionManager.p2pConnectionManager p2p;
        ServerConnectionManager.ServerConnectionManager server;
        ClientCallback callback;
        StunManager stun;
        int port = 1100;
        UdpClient client;

        public event Action<string> SessionCreated;
        public event Action<string, string> ClientJoined;

        public Model()
        {
            client = new UdpClient(port);
            stun = new StunManager(client);           
            callback = new ClientCallback();
            server = new ServerConnectionManager.ServerConnectionManager(callback);
            CallbackEventSubscribe();
        }

        private void CallbackEventSubscribe()
        {
            callback.SessionCreated += OnSessionCreated;
            callback.ClientJoined += OnClientJoined;
        }

        private void OnSessionCreated(Session session)
        {
            server.AddSession(session);
            SessionCreated(session.SessionName);
        }

        private void OnClientJoined(Session session, ServerClient client)
        {
            p2p.SetConnectionAdress(client.IPAddress);
            server.ClientJoined(session);
            ClientJoined(client.NickName, client.IPAddress.ToString());
        }

        public bool CheckNAT()
        {
            string type = stun.GetTypeOfNAT();
            if (type != "UdpBlocked" && type != "SymmetricUdpFirewall" && type != "Symmetric")
                return true;
            else
                return false;
        }

        public async Task<bool> TryConnect(string username)
        {
            return await server.TryConnect(stun.GetIpPort(), username);
        }

        public IPEndPoint GetAdress()
        {
            return stun.GetIpPort();
        }

        public async Task<string[]> GetListOfSessions()
        {
            return await server.GetListOfSessions();
        }

        public async Task<bool> CreateSession(string password)
        {
            return await server.CreateSession(password);
        }

        public async Task<bool> JoinSession(string sessionName, string password)
        {
            bool result = await server.JoinSession(sessionName, password);
            if(result == true)
            {
                p2p.SetConnectionAdress(server.GetAdress());
            }
            return result;
        }
    }
}
