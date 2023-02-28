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
using ModulationComplexity.Models;
using Prism.Commands;
using System.Numerics;
using JR.Utils.GUI.Forms;
using System.Windows.Controls;
using System.Resources;
using ModulationComplexity.CustomWidgets;

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
       // public DelegateCommand ExeCmd { get; set; }

        private string ResultStr;

        public ObservableCollection<ListItem> ListItems { get; set; }

        internal ComplexityModel ComplexityModel { get; }

        private bool showWindow;

        public bool ShowWindow
        {
            get { return showWindow; }
            set { SetProperty(ref showWindow, value); }
        }


        public View1Model(ScriptContext currentContext)
        {

            this.context = currentContext;
            this.ComplexityModel = new ComplexityModel(context);
            var retval = this.ComplexityModel.Execute();
            this.ListItems = retval.Item1;
            this.ResultStr = retval.Item2;

            //ExeCmd = new DelegateCommand(OnExe);
            SaveCmd = new DelegateCommand(OnSave);
            AboutCmd = new DelegateCommand(OnAbout);

            SubWindow = new Window();
            SubWindow.Height = 500;
            SubWindow.Width = 500;
            SubWindow.Title = "About";
            SubWindow.Content = new BindableRichTextBox()
            {
                IsReadOnly= true,
                Source = new Uri(@"pack://application:,,,/ModulationComplexity.esapi;component/Resources/About.rtf"),
                
            };
            

            
        }

        private void OnAbout()
        {

            SubWindow.Show();
        }


        private void OnSave()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                var result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var outputpath = dialog.SelectedPath;
                    Directory.CreateDirectory(outputpath);

                    var savepath = Path.Combine(outputpath, string.Format("{0}-complexity.csv", context.Patient.Name));
                    File.WriteAllText(savepath, ResultStr);

                    // MessageBox.Show(msg);
                    MessageBox.Show(string.Format("CSV saved in {0}", savepath));

                }

            }

        }

    }
}
