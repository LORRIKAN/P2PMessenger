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
        }

        private void ViewEventsSubscribe()
        {
            view.FormLoad += OnFormLoad;
            view.UsernameEntered += OnUsernameEntered;
            view.AdressRequested += OnAdressRequest;
            view.ListOfSessionsRequested += OnListOfSessionsRequested;
            view.CreateSession += OnCreateSession;
        }

        public bool OnFormLoad()
        {
            return(model.CheckNAT());
        }

        public bool OnUsernameEntered(string username)
        {
            return model.TryConnect(username);
        }

        public IPEndPoint OnAdressRequest()
        {
            return model.GetAdress();
        }

        public string[] OnListOfSessionsRequested()
        {
            return model.GetListOfSessions();
        }

        public async Task<bool> OnCreateSession(string password)
        {
            return await model.CreateSession(password);
        }
    }
}
