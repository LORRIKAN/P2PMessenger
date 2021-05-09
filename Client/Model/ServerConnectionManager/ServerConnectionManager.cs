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
    }
}
