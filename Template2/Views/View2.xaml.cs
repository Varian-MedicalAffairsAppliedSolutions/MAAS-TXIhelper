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
using ViewModels;
 
namespace Views
{

    /// <summary>
    /// Interaction logic for GridDialog.xaml
    /// </summary>
    public partial class View2 : UserControl
    {

        public View2Model vm;

        public View2(ScriptContext context)
        {
            InitializeComponent();
            vm = new View2Model(context);
            DataContext = vm;
        }


    }
}

