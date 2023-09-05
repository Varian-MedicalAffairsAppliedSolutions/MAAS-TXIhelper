using MAAS_TXIHelper.Views;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using ViewModels;
using VMS.TPS.Common.Model.API;

namespace Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CPFlipView v1;
        private CTConcatView v2;
        private PlaceIsocentersView v3;
        private FinalPlanPrepView v4;
        private ApertureRotationDicomView v5;

        public MainWindow(ScriptContext context, MainViewModel vm)
        {
            InitializeComponent();
            this.v1 = new CPFlipView(context);
            this.v2 = new CTConcatView(context);
            this.v3 = new PlaceIsocentersView(context);
            this.v4 = new FinalPlanPrepView(context);
            this.v5 = new ApertureRotationDicomView(context);
            // Access individual TabItems by their names
            // Access the TabControl by its name



            Tab1.Content = v2;
            Tab2.Content = v3;
            Tab3.Content = v5;
            Tab4.Content = v4;

            DataContext = vm;
        }


        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // for .NET Core you need to add UseShellExecute = true
            // see https://learn.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.useshellexecute#property-value
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
