using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client;
using Client.Model;

namespace Client.Presenter
{
    class Presenter
    {
        MainWindow view;
        Model.Model model;

        public Presenter(MainWindow view)
        {
            this.view = view;
            model = new Model.Model();
        }
    }
}
