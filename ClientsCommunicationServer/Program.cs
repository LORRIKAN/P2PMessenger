using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using WCF_Service;

namespace ClientsCommunicationServer
{
    class Program
    {
        static void Main()
        {
            Uri tcpUri = new Uri("net.tcp://gorelovopc.ddns.net:8302");

            // Create the ServiceHost.
            ServiceHost sh = new ServiceHost(typeof(ClientsCommunicationService), tcpUri);

            // Create a binding that uses TCP and set the security mode to none.
            NetTcpBinding b = new NetTcpBinding();
            b.Security.Mode = SecurityMode.None;
            b.OpenTimeout = new TimeSpan(1, 0, 0);
            b.CloseTimeout = new TimeSpan(1, 0, 0);
            b.SendTimeout = new TimeSpan(1, 0, 0);
            b.ReceiveTimeout = new TimeSpan(1, 0, 0);

            // Add an endpoint to the service.
            sh.AddServiceEndpoint(typeof(IClientsCommunicationService), b, "");
            // Open the service and wait for calls.
            sh.Open();

            Console.WriteLine("Хост стартовал");
            Console.ReadKey();
        }
    }
}