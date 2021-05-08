using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Client.Model.p2pConnectionManager
{
    class p2pConnectionManager
    {
        public bool StopReceiving;
        UdpClient client = new UdpClient();
        event Action<string> MessageReceive;

        public Task Cycle()
        {
            return Task.Run(() =>
            {
                while (!StopReceiving)
                {
                    if (client.Available != 0)
                    {
                        MessageReceive("1");
                    }
                }
            });
        }
    }
}
