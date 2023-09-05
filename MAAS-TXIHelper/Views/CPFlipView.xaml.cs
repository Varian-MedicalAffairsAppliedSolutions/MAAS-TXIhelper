using System.Windows.Controls;
using ViewModels;
using VMS.TPS.Common.Model.API;

namespace Views
{

    /// <summary>
    /// Interaction logic for GridDialog.xaml
    /// </summary>
    public partial class CPFlipView : UserControl
    {

        public CPFlipView(ScriptContext context)
        {
            InitializeComponent();
            DataContext = new CPFlipViewModel(context); //this.vm;

        }



    }
}
