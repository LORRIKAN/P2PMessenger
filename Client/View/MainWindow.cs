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
using Client.Model.ServerConnectionManager;
using System.Net;

namespace Client.View
{
    public partial class MainWindow : Form
    {
        public event Func<bool> FormLoad;
        public event Func<string, bool> UsernameEntered;
        public event Func<IPEndPoint> AdressRequested;
        public event Func<string[]> ListOfSessionsRequested;
        public event Func<string, Task<bool>> CreateSession;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            Registration(FormLoad());
        }

        private void Registration(bool natAvailable)
        {
            if(natAvailable == false)
            {
                MessageBox.Show("Невозможно установить p2p соединения из вашей сети. Пожалуйста смените сеть.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            else
            {
                EnterUsername();
            }
        }

        private void EnterUsername()
        {
            bool usernameAccepted;
            string username;
            do
            {
                username = Microsoft.VisualBasic.Interaction.InputBox("Введите свое имя:");
                if (username.Trim() == "")
                    Environment.Exit(1);
                usernameAccepted = UsernameEntered(username);
            } while (usernameAccepted == false);
            IPEndPoint tmp = AdressRequested();
            textBox1.Text += "Соединение с сервером установлено. Вы зашли под именем: " + username + " Ваш адрес: " + AdressRequested().ToString() + Environment.NewLine;
            GetListOfSessions();
        }

        private void GetListOfSessions()
        {
            string[] sessions = ListOfSessionsRequested();
            foreach(string s in sessions)
            {
                listBox1.Items.Add(s);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            string password;
            password = Microsoft.VisualBasic.Interaction.InputBox("Установка пароля для сессии:");
            if(await CreateSession(password) == true)
            {
                listBox1.Items.Clear();
                listBox1.Enabled = false;
                button1.Enabled = false;
                button2.Enabled = false;
                textBox2.Enabled = true;
                button3.Enabled = true;
                textBox1.Text += "Сессия создана. Пароль: " + password + "Ожидание подключения." + Environment.NewLine; 
            }
            else
            {
                MessageBox.Show("Ошибка при создании сессии.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /*
        public void Visualize()
        {
            this.Invoke(new MethodInvoker(() => { textBox1.Text = "1"; textBox2.Text = "2";
                }));
        }
        */
    }
}
