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
using System.Windows.Shapes;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace GridBlockCreator
{


    /// <summary>
    /// Interaction logic for GridDialog.xaml
    /// </summary>
    public partial class GridDialog : Window
    {

        public GridDialogViewModel vm;


        public GridDialog(ScriptContext context)
        {
            InitializeComponent();
            vm = new GridDialogViewModel(context);
            this.DataContext = vm;

            //if (vm.CloseAction == null)
            //    vm.CloseAction = new Action(this.Close);
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
            this.Close();
        }

        private void CreateGridAndInverse(object sender, RoutedEventArgs e)
        {
            vm.CreateGridAndInverse();
            this.Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();

        }

        private void TargetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
