using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Model.p2pConnectionManager;
using Client.Model.ServerConnectionManager;
using System.Net;
using System.Net.Sockets;
using Client.ClientsCommunicationService1;

namespace Client.Model
{
    class Model
    {
        p2pConnectionManager.p2pConnectionManager p2p;
        ServerConnectionManager.ServerConnectionManager server;
        ClientCallback callback;
        StunManager stun;
        int port = 1101;
        UdpClient client;

        public event Action<string> SessionCreated;
        public event Action<string, string> ClientJoined;
        public event Action<string, string> GiveClientInfo;
        public event Action<string> MessageReceived;

        public Model()
        {
            client = new UdpClient(port);
            stun = new StunManager(client);           
            callback = new ClientCallback();
            server = new ServerConnectionManager.ServerConnectionManager(callback);
            p2p = new p2pConnectionManager.p2pConnectionManager(client);
            CallbackEventSubscribe();            
        }

        private void CallbackEventSubscribe()
        {
            callback.SessionCreated += OnSessionCreated;
            callback.ClientJoined += OnClientJoined;
            server.GiveClientInfo += OnGiveClientInfo;
            p2p.MessageReceived += OnMessageReceived;
        }


        public void OnMessageReceived(string message)
        {
            MessageReceived(message);
            //p2p.Cycle();
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

        private void OnGiveClientInfo(string name, string adress)
        {
            GiveClientInfo(name, adress);
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

        public bool StartChat()
        {
            bool result = p2p.CheckConnection();
            if (result == true)
                p2p.Cycle();
            return result;
        }

        public void SendMessage(string message)
        {
            p2p.SendMessage(message);
        }
    }
}
