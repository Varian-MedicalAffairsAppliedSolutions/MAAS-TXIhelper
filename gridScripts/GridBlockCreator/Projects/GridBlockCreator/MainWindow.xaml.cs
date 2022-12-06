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

namespace GridBlockCreator
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(ScriptContext context, bool showTerms)
        {
            InitializeComponent();
            SphereLatticeTab.Content = new SphereDialog(context);
            RapidRodsTab.Content = new GridDialog(context);

            if(showTerms ) { // TODO show banner here
                Footer.Text = "*** Not Valid For Clinical Use ***";
          
            }


        }
    }
}
