using NLog.Layouts;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
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
            MessageBox.Show($"Is Debug == {isDebug}");
            Footer = "Bound by the terms of the Varian LUSLA";
            //var hlink = new Hyperlink() { NavigateUri = new Uri("http://medicalaffairs.varian.com/download/VarianLUSLA.pdf") };
            //Footer += hlink;
            if (isDebug)
            {
                Footer += " *** Not Validated for clinical use ***";
            }
            
        }

    }
}
