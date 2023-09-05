using MAAS_TXIHelper.ViewModels;
using System.Windows.Controls;
using VMS.TPS.Common.Model.API;

namespace MAAS_TXIHelper.Views
{
    /// <summary>
    /// Interaction logic for CTConcatView.xaml
    /// </summary>
    public partial class CTConcatView : UserControl
    {
        public CTConcatView(ScriptContext context)
        {
            this.DataContext = new CTConcatViewModel(context);
            InitializeComponent();
        }
    }
}
