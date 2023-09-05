using MAAS_TXIHelper.ViewModels;
using System.Windows.Controls;
using VMS.TPS.Common.Model.API;

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
    }
}
