using MAAS_TXIHelper.ViewModels;
using System.Windows.Controls;

namespace MAAS_TXIHelper.Views
{
    /// <summary>
    /// Interaction logic for FinalPlanPrepView.xaml
    /// </summary>
    public partial class FinalPlanPrepView : UserControl
    {
        public FinalPlanPrepView(VMS.TPS.Common.Model.API.ScriptContext context)
        {
            DataContext = new FinalPlanPrepViewModel(context);
            InitializeComponent();
        }
    }
}
