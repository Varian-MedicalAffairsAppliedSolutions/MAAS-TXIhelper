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
using ViewModels;

namespace Views
{

    /// <summary>
    /// Interaction logic for GridDialog.xaml
    /// </summary>
    public partial class View1 : UserControl
    {

        public View1(ScriptContext context)
        {
            InitializeComponent();
            //this.vm = new View1Model(context);
            DataContext = new View1Model(context); //this.vm;

        }
        /*
        private RoutedEventHandler OnLoaded(object sender, RoutedEventArgs e)
        {

        }*/


        

     
    }
}
