using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using LumiSoft.Net.STUN.Client;
using System.Net;

namespace Client.Model
{
    class StunManager
    {
        UdpClient client;
        string stunAdress;
        STUN_Result result;

        public StunManager(UdpClient client)
        {
            client.AllowNatTraversal(true);
            this.client = client;
            stunAdress = "stun.l.google.com";
            result = STUN_Client.Query(stunAdress, 19302, client.Client);
        }

        public string GetTypeOfNAT()
        {
            return result.NetType.ToString();
        }

        public IPEndPoint GetIpPort()
        {
            return result.PublicEndPoint;
        }
    }
}
