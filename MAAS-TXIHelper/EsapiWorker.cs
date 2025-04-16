using System.Windows.Threading;
using System;
using VMS.TPS.Common.Model.API;

namespace MAAS_TXIHelper.Views
{
    public class EsapiWorker
    {
        private readonly ScriptContext _scriptContext;
        private readonly Dispatcher _dispatcher;

        public EsapiWorker(ScriptContext scriptContext)
        {
            _scriptContext = scriptContext;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void Run(Action<ScriptContext> a)
        {
            // The BeginkInvoke method executes the delegate asynchronously
            _dispatcher.BeginInvoke(a, _scriptContext);
        }
    }
}