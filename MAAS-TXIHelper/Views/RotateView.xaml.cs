using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MAAS_TXIHelper.ViewModels;

namespace MAAS_TXIHelper.Views
{
    /// <summary>
    /// Interaction logic for RotateView.xaml
    /// </summary>
    public partial class RotateView : UserControl
    {
        public RotateView(EsapiWorker esapiWorker)
        {
            InitializeComponent();
            DataContext = new RotateViewModel(esapiWorker);
        }
        private void textChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            OutputTextBox.SelectionStart = OutputTextBox.Text.Length;
            OutputTextBox.ScrollToEnd();
        }
    }
}
