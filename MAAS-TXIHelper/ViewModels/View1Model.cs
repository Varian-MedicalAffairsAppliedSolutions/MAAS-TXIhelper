using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml;
using System.Collections.ObjectModel;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Threading;
using System.Net;
using System.IO;

using System.Runtime.CompilerServices;
using System.Windows.Media;
using Prism.Mvvm;
using MAAS_TXIHelper.Models;
using Prism.Commands;
using System.Numerics;
using JR.Utils.GUI.Forms;
using System.Windows.Controls;
using System.Resources;
using MAAS_TXIHelper.CustomWidgets;
using System.Windows.Controls.Primitives;
using Views;

// TODO
// Button that shows formula

namespace ViewModels
{

    public class View1Model : BindableBase
    {
        private ScriptContext context;

        private Window SubWindow;

        public string AboutURI { get; set; }

        public DelegateCommand AboutCmd { get; set; }

        public DelegateCommand SaveCmd { get; set; }  

        public ObservableCollection<ListItem> ListItems { get; set; }


        public DelegateCommand ShowWindowCmd { get;set ; }


        public View1Model(ScriptContext currentContext)
        {

            this.context = currentContext;

            //ExeCmd = new DelegateCommand(OnExe);
            SaveCmd = new DelegateCommand(OnSave);
            AboutCmd = new DelegateCommand(OnAbout);
            //ShowWindowCmd = new DelegateCommand(OnShowWindow);

            SubWindow = new Window();
            SubWindow.Height = 500;
            SubWindow.Width = 500;
            SubWindow.Title = "About";
            SubWindow.Content = new BindableRichTextBox()
            {
                IsReadOnly= true,
                Source = new Uri(@"pack://application:,,,/MAAS_TXIHelper.esapi;component/Resources/About.rtf"),
               
            };

            SubWindow.Closing += OnClosing;

        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            SubWindow.Hide();
            e.Cancel = true;
        }

        private void OnAbout()
        {
            SubWindow.Show();
        }


        private void OnSave()
        {
            

        }

    }
}
