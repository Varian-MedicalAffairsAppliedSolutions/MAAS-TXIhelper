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
    /// Interaction logic for FinalizeView.xaml
    /// </summary>
    public partial class FinalizeView : UserControl
    {
        public FinalizeView(EsapiWorker esapiWorker)
        {
            InitializeComponent();
            DataContext = new FinalizeViewModel(esapiWorker);
        }
        private void TextChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            OutputTextBox.SelectionStart = OutputTextBox.Text.Length;
            OutputTextBox.ScrollToEnd();
        }
    }
}
