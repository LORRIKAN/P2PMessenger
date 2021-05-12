using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Client.Model.p2pConnectionManager
{
    class p2pConnectionManager
    {
        private IPEndPoint connectionAdress;
        private UdpClient client;
        private Status status = Status.NoConnection;

        public event Action<string> MessageReceived;

        public p2pConnectionManager(UdpClient client)
        {
            this.client = client;
        }

        public Task Cycle()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    if (client.Available != 0)
                    {
                        string message = Encoding.UTF8.GetString(client.Receive(ref connectionAdress));
                        MessageReceived(message);
                    }
                }
            });
        }

        public void SetConnectionAdress(IPEndPoint address)
        {
            connectionAdress = address;
        }

        public bool CheckConnection()
        {
            byte[] firstCheck = new byte[1] { 100 };
            byte[] secondCheck = new byte[1] { 200 };

            for (int i = 0; i < 2000; i++)
            {
                if(status == Status.NoConnection)
                {
                    client.Send(firstCheck, 1, connectionAdress);
                    Thread.Sleep(100);
                    if (client.Available > 0)
                    {
                        byte[] income = client.Receive(ref connectionAdress);
                        foreach(byte b in income)
                        {
                            if (b == 200)
                            {
                                client.Send(secondCheck, 1, connectionAdress);
                                status = Status.SecondCheck;
                                return true;
                            }
                        }
                        status = Status.FirstCheck;
                    }
                }
                else
                {
                    client.Send(secondCheck, 1, connectionAdress);
                    Thread.Sleep(100);
                    if (client.Available > 0)
                    {
                        byte[] income = client.Receive(ref connectionAdress);
                        foreach (byte b in income)
                        {
                            if (b == 200)
                            {
                                status = Status.SecondCheck;
                                return true;
                            }
                        }                        
                    }
                }
            }

            return false;
        }

        public void SendMessage(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            client.Send(messageBytes, messageBytes.Length, connectionAdress);
        }
    }

    enum Status
    {
        NoConnection,
        FirstCheck,
        SecondCheck
    }
}
