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
        private ScriptContext _Context;
        private Patient _Patient {get; set;}
        private Course _Course { get; set;}
        private ExternalPlanSetup _Plan { get; set;}

        public DelegateCommand FlipCmd { get; set;} 
        public DelegateCommand SelectLogPathCmd { get; set;}

        public View1Model(ScriptContext currentContext)
        {

            _Context = currentContext;
            _Patient = currentContext.Patient;
            _Course = currentContext.Course;
            _Plan = currentContext.PlanSetup as ExternalPlanSetup;

            FlipCmd = new DelegateCommand(OnFlip);
            SelectLogPathCmd = new DelegateCommand(OnSelectLogPath);

        }

        private void OnSelectLogPath()
        {
            MessageBox.Show("Select log path placeholder");
        }

        private void OnFlip()
        {
            MessageBox.Show("Starting flip CPs per COH code");
            MessageBox.Show("Finished flip CPs");
        }

        

       

    }
}
