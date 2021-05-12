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
using Client.ClientsCommunicationService1;
using Client.Model.ServerConnectionManager;
using System.Net;

namespace Client.View
{
    public partial class MainWindow : Form
    {
        string myName;

        public event Func<bool> FormLoad;
        public event Func<string, Task<bool>> UsernameEntered;
        public event Func<IPEndPoint> AdressRequested;
        public event Func<Task<string[]>> ListOfSessionsRequested;
        public event Func<string, Task<bool>> CreateSession;
        public event Func<string, string, Task<bool>> JoinSession;
        public event Func<bool> StartChat;
        public event Action<string> MessageSent;
        public event Func<Task> ChatClosed;

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

        private async void EnterUsername()
        {
            bool usernameAccepted;
            string username;
            do
            {
                username = Microsoft.VisualBasic.Interaction.InputBox("Введите свое имя:");
                if (username.Trim() == "")
                    Environment.Exit(1);
                usernameAccepted = await UsernameEntered(username);
            } while (usernameAccepted == false);
            IPEndPoint tmp = AdressRequested();
            textBox1.Text += "Соединение с сервером установлено. Вы зашли под именем: " + username + " Ваш адрес: " + AdressRequested().ToString() + Environment.NewLine;
            myName = username;
            GetListOfSessions();
        }

        private async void GetListOfSessions()
        {
            string[] sessions = await ListOfSessionsRequested();
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
                textBox1.Text += "Сессия создана. Пароль: " + password + " Ожидание подключения." + Environment.NewLine; 
            }
            else
            {
                MessageBox.Show("Ошибка при создании сессии.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public void AddSession(string sessionName)
        {
            listBox1.Items.Add(sessionName);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string password;
            password = Microsoft.VisualBasic.Interaction.InputBox("Введите пароль:");
            if(await JoinSession(listBox1.SelectedItem.ToString(), password) == true)
            {
                listBox1.Items.Clear();
                listBox1.Enabled = false;
                button1.Enabled = false;
                button2.Enabled = false;                
                textBox1.Text += "Вы подключились к сессии." + Environment.NewLine;
                if (StartChat() == true)
                {
                    textBox1.Text += "Теперь вы можете отправлять сообщения." + Environment.NewLine;
                    textBox2.Enabled = true;
                    button3.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Неверный пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ClientJoined(string name, string adress)
        {
            textBox1.Text += name + " вошел в чат. Его адресс: " + adress + Environment.NewLine;
            if (StartChat() == true)
            {
                textBox1.Text += "Теперь вы можете отправлять сообщения." + Environment.NewLine;
                textBox2.Enabled = true;
                button3.Enabled = true;
            }
        }

        public void ShowClientInfo(string name, string adress)
        {
            textBox1.Text += "Создатель сессии: " + name + " Его адрес: " + adress + Environment.NewLine;
        }

        public void ShowMessage(string message)
        {
            this.Invoke(new MethodInvoker(() => { textBox1.Text += message + Environment.NewLine; }));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            ChatClosed();
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == (char)13 && button3.Enabled == true)
            {
                SendMessage();
            }
        }

        private void SendMessage()
        {
            if (textBox2.Text.Trim() == "")
                return;
            string message = myName + ": " + textBox2.Text;
            message = message.Replace("\n", String.Empty);
            message = message.Replace("\r", String.Empty);
            message = message.Replace("\t", String.Empty);
            textBox2.Text = string.Empty;
            textBox1.Text += message + Environment.NewLine;
            MessageSent(message);
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
