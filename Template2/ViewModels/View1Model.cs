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
using Template2.Models;
using Prism.Commands;

namespace ViewModels
{


    public class View1Model : BindableBase
    {
        private ScriptContext context;

        public DelegateCommand SaveCmd { get; set; }  
        public DelegateCommand ExeCmd { get; set; } 

        internal ComplexityModel ComplexityModel { get; }

        public View1Model(ScriptContext currentContext)
        {

            this.context = currentContext;
            this.ComplexityModel = new ComplexityModel(context);

            ExeCmd = new DelegateCommand(OnExe);
            SaveCmd = new DelegateCommand(OnSave);
            
        }

        private void OnExe()
        {
            MessageBox.Show("On Exe called");
        }

        private void OnSave()
        {
            MessageBox.Show("On Save called");
        }
                  
    }
}
