using MAAS_TXIHelper.ViewModels;
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

namespace MAAS_TXIHelper.Views
{
    /// <summary>
    /// Interaction logic for ConcatView.xaml
    /// </summary>
    public partial class ConcatView : UserControl
    {
        public ConcatView(EsapiWorker esapiWorker)
        {
            InitializeComponent();
            DataContext = new ConcatViewModel(esapiWorker);
        }
        private void textChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            OutputTextBox.SelectionStart = OutputTextBox.Text.Length;
            OutputTextBox.ScrollToEnd();
        }
    }
}
