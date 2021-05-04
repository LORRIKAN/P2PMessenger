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
            using (var host = new ServiceHost(typeof(ClientsCommunicationService)))
            {
                host.Open();

                Console.WriteLine("Хост стартовал");
                Console.ReadKey();
            }
        }
    }
}