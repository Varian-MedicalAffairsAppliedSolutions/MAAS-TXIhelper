using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using NLog;
using System.Net.NetworkInformation;
using System.ComponentModel;

namespace GridBlockCreator
{


    public class BoolToBlue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo language)
        {
            bool selectionFlag = (bool)value;
            return selectionFlag ? "Blue" : "Gray";
            //return selectionFlag ? System.Windows.Media.Colors.Blue : System.Windows.Media.Colors.Green;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo language)
        {

            System.Windows.Media.Color color = (System.Windows.Media.Color)value;
            return color == System.Windows.Media.Colors.Blue;

        }
    }

    public class BoolToStrike : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo language)
        {
            bool selectionFlag = (bool)value;
            return selectionFlag ? "" : "1";
            //return selectionFlag ? System.Windows.Media.Colors.Blue : System.Windows.Media.Colors.Green;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo language)
        {

            System.Windows.Media.Color color = (System.Windows.Media.Color)value;
            return color == System.Windows.Media.Colors.Blue;

        }
    }

    public class RadiusToDiameter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo language)
        {
            return 2.0 * (double)value;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo language)
        {

            return 0.5 * (double)value;

        }
    }


    /// <summary>
    /// Interaction logic for GridDialog.xaml
    /// </summary>
    public partial class SphereDialog : Window
    {

        public SphereDialogViewModel vm;

        public TextBoxOutputter outputter;

        public SphereDialog(ScriptContext context)
        {
            InitializeComponent();
            vm = new SphereDialogViewModel(context);
            DataContext = vm;
        }

        void TimerTick(object state)
        {
            var who = state as string;
            Console.WriteLine(who);
        }

        private void ToggleCircle(object sender, MouseButtonEventArgs e)
        {
            var selectedEllipse = (System.Windows.Shapes.Ellipse)sender;
            Circle selectedCircle = (Circle)selectedEllipse.DataContext;
            selectedCircle.Selected = !selectedCircle.Selected;
        }

        private void CreateGrid(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += vm.CreateGrid2;
            worker.ProgressChanged += vm.worker_ProgressChanged;

            worker.RunWorkerAsync();

        }

        

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

    }
}

