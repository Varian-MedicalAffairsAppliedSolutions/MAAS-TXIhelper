﻿using MAAS_TXIHelper.Views;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace MAAS_TXIHelper.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConcatView _concatView;
        private RotateView _rotateView;
        private OverrideView _overrideView;
        private FinalizeView _finalizeView;
        public MainWindow(EsapiWorker esapiWorker)
        {
            InitializeComponent();
            _concatView = new ConcatView(esapiWorker);
            _rotateView = new RotateView(esapiWorker);
            _overrideView = new OverrideView(esapiWorker);
            _finalizeView = new FinalizeView(esapiWorker);
            //            _rotateView = new CPFlipView(esapiWorker);
            Tab1.Content = _concatView;
            Tab2.Content = _rotateView;
            Tab3.Content = _overrideView;
            Tab4.Content = _finalizeView;
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
