using MAAS_TXIHelper.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for OverrideView.xaml
    /// </summary>
    public partial class OverrideView : UserControl
    {
        public OverrideView(EsapiWorker esapiWorker)
        {
            InitializeComponent();
            DataContext = new OverrideViewModel(esapiWorker);
        }
        private void TextChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            OutputTextBox.SelectionStart = OutputTextBox.Text.Length;
            OutputTextBox.ScrollToEnd();
        }
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs args)
        {
            args.Handled = !IsTextAllowed(args.Text);
        }
        private static readonly Regex _regex = new Regex("[^0-9.-]+");
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }
    }
}
