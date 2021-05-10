using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client;
using Client.Model;
using Client.View;
using System.Net;

namespace Client.Presenter
{
    class Presenter
    {
        MainWindow view;
        Model.Model model;

        public Presenter(MainWindow view)
        {
            this.view = view;
            ViewEventsSubscribe();
            model = new Model.Model();
            ModelEventsSubscribe();
        }

        private void ViewEventsSubscribe()
        {
            view.FormLoad += OnFormLoad;
            view.UsernameEntered += OnUsernameEntered;
            view.AdressRequested += OnAdressRequest;
            view.ListOfSessionsRequested += OnListOfSessionsRequested;
            view.CreateSession += OnCreateSession;
            view.JoinSession += OnJoinSession;
        }

        private void ModelEventsSubscribe()
        {
            model.SessionCreated += OnSessionCreated;
            model.ClientJoined += OnClientJoined;
        }

        private void OnSessionCreated(string sessionName)
        {
            view.AddSession(sessionName);
        }

        public void OnClientJoined(string name, string adress)
        {
            view.ClientJoined(name, adress);
        }

        public bool OnFormLoad()
        {
            return(model.CheckNAT());
        }

        public async Task<bool> OnUsernameEntered(string username)
        {
            return await model.TryConnect(username);
        }

        public IPEndPoint OnAdressRequest()
        {
            return model.GetAdress();
        }

        public async Task<string[]> OnListOfSessionsRequested()
        {
            return await model.GetListOfSessions();
        }

        public async Task<bool> OnCreateSession(string password)
        {
            return await model.CreateSession(password);
        }

        public async Task<bool> OnJoinSession(string sessionName, string password)
        {
            return await model.JoinSession(sessionName, password);
        }
    }
}
