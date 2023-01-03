using NLog.Layouts;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace GridBlockCreator
{
	

	public class MainViewModel: BindableBase
    {
        private string footer;

        public string Footer
        {
            get { return footer; }
            set { SetProperty(ref footer, value); }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri)
             );
            e.Handled = true;
        }


        public MainViewModel(bool isDebug)
        {
            // Get app.config value
            /*
            string pth = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            MessageBox.Show($"CWD = {pth}");
            var strConfigPath = Path.Combine(pth, "App.config");
            System.Configuration.ConfigurationFileMap fileMap = new ConfigurationFileMap(strConfigPath); //Path to your config file
            System.Configuration.Configuration configuration = System.Configuration.ConfigurationManager.OpenMappedMachineConfiguration(fileMap);
            var x = configuration.AppSettings.Settings.AllKeys[0];
            //var showTerms = configuration.AppSettings.Settings["DisplayTerms"].Value;
            MessageBox.Show($"Is Debug == {x}");
            */
            //MessageBox.Show($"{AppDomain.CurrentDomain.SetupInformation.ConfigurationFile}");
            /*
            MessageBox.Show($"Is Debug == {isDebug}");
            Footer = "Bound by the terms of the Varian LUSLA";
            var hlink = new Hyperlink() { NavigateUri = new Uri() };
            Footer += hlink;
            if (isDebug)
            {
                Footer += " *** Not Validated for clinical use ***";
            }*/

        }

    }
}
