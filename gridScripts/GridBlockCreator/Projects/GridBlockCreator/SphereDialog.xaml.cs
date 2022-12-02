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





    /// <summary>
    /// Interaction logic for GridDialog.xaml
    /// </summary>
    public partial class SphereDialog : UserControl
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
            vm.CreateGrid();
        }




        private void Cancel(object sender, RoutedEventArgs e)
        {
            //this.Close();

        }

    }
}

