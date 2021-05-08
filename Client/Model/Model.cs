using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Model.p2pConnectionManager;
using Client.Model.ServerConnectionManager;
using System.Net;
using System.Net.Sockets;

namespace Client.Model
{
    class Model
    {
        p2pConnectionManager.p2pConnectionManager p2p;
        ServerConnectionManager.ServerConnectionManager server;
        StunManager stun;
        int port = 1100;
        UdpClient client;

        public Model()
        {
            client = new UdpClient(port);
            stun = new StunManager(client);           
            server = new ServerConnectionManager.ServerConnectionManager();
        }

        public bool CheckNAT()
        {
            string type = stun.GetTypeOfNAT();
            if (type != "UdpBlocked" && type != "SymmetricUdpFirewall" && type != "Symmetric")
                return true;
            else
                return false;
        }

        public bool TryConnect(string username)
        {
            return server.TryConnect(stun.GetIpPort(), username);
        }

        public IPEndPoint GetAdress()
        {
            return stun.GetIpPort();
        }
    }
}
