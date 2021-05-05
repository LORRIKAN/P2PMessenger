using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client.ClientsCommunicationService;

namespace Client.View
{
    public partial class MainWindow : Form
    {
        private ClientsCommunicationServiceClient Server { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        public void Method()
        {
            try
            {
                ServerClient serverClient = Server.Connect(null, null);

                Server.ChangeNickName(serverClient, "Пажилой");
            }

            catch (FaultException<ClientWithSuchIPAddressExistsException> ex)
            {

            }
        }
    }
}
