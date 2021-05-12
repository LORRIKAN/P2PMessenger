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

        private Socket _socket;
        private const int bufSize = 8 * 1024;
        private State state = new State();        
        private AsyncCallback recv = null;
        private EndPoint epFrom;

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public event Action<string> MessageReceived;

        public p2pConnectionManager(UdpClient client)
        {
            this.client = client;
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

        public void StartChat()
        {
            _socket = client.Client;
            _socket.Connect(connectionAdress);
            epFrom = connectionAdress;
            Receive();
        }

        public void SendMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            _socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndSend(ar);                
            }, state);
        }

        public void Receive()
        {
            _socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndReceiveFrom(ar, ref epFrom);
                _socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);
                MessageReceived(Encoding.ASCII.GetString(so.buffer, 0, bytes));
            }, state);
        }
    }

    enum Status
    {
        NoConnection,
        FirstCheck,
        SecondCheck
    }
}
