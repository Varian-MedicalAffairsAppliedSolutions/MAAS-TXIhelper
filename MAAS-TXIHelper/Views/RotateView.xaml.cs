using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
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
        private void RotateImageLoaded(object sender, RoutedEventArgs e)
        {
            BitmapImage pic = new BitmapImage();
            pic.BeginInit();
            string dllDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string imageFilePath = System.IO.Path.Combine(dllDirectory, "Resources\\rotate.png");
            pic.UriSource = new Uri(imageFilePath);
            pic.EndInit();

            // ... Get Image reference from sender.
            var image = sender as Image;
            image.Source = pic;
        }
    }
}
