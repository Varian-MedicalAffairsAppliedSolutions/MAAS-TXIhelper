using MAAS_TXIHelper.ViewModels;
using System.Windows;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VMS.TPS.Common.Model.API;
using System.Reflection;

namespace MAAS_TXIHelper.Views
{
    /// <summary>
    /// Interaction logic for ApertureRotationDicomView.xaml
    /// </summary>
    public partial class ApertureRotationDicomView : UserControl
    {
        public ApertureRotationDicomView(ScriptContext context)
        {
            this.DataContext = new ApertureRotationDicomViewModel(context);
            InitializeComponent();
        }
        private void RotateImageLoaded(object sender, RoutedEventArgs e)
        {
            BitmapImage b = new BitmapImage();
            b.BeginInit();
            string dllDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string imageFilePath = Path.Combine(dllDirectory, "Resources\\rotate.png");
            b.UriSource = new Uri(imageFilePath);
            b.EndInit();

            // ... Get Image reference from sender.
            var image = sender as System.Windows.Controls.Image;
            image.Source = b;
        }
    }
}
