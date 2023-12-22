// Copied from https://github.com/esimiele/SimpleProgressWindow
// Can't use as NuGet package, because it's targeting higher .NET Framework

using System;
using System.Text;
using System.Windows.Threading;

namespace SimpleProgressWindow
{
    public class SimpleMTbase
    {
        public StringBuilder GetLogOutput() { return _logOutput; }
        protected string GetElapsedTime() { return _pw.GetElapsedTime(); }

        private Dispatcher _dispatch;
        private SimpleMTProgress _pw;
        private StringBuilder _logOutput;
        private bool _closeOnSuccessfulFinish = false;
        //timeout in msec before closing window
        private int _closeTimeOut = 0;

        public virtual bool Run()
        {
            return false;
        }

        public bool Execute()
        {
            ESAPIWorker slave = new ESAPIWorker();
            //create a new frame (multithreading jargon)
            DispatcherFrame frame = new DispatcherFrame();
            slave.RunOnNewThread(() =>
            {
                //pass the progress window the newly created thread and this instance of the optimizationLoop class.
                SimpleMTProgress pw = new SimpleMTProgress();
                pw.SetCallerClass(slave, this);
                pw.SetCloseOnFinish(_closeOnSuccessfulFinish, _closeTimeOut);
                pw.ShowDialog();

                //tell the code to hold until the progress window closes.
                frame.Continue = false;
            });
            Dispatcher.PushFrame(frame);
            return slave.isError;
        }

        public void SetDispatcherAndUIInstance(Dispatcher d, SimpleMTProgress p)
        {
            _dispatch = d;
            _pw = p;
            _logOutput = new StringBuilder();
        }

        protected void UpdateUILabel(string message)
        {
            _logOutput.AppendLine(message);
            _dispatch.BeginInvoke((Action)(() => { _pw.UpdateLabel(message); }));
        }

        protected void ProvideUIUpdate(int percentComplete, string message = "", bool fail = false)
        {
            if (!string.IsNullOrEmpty(message)) _logOutput.AppendLine(message);
            _dispatch.BeginInvoke((Action)(() => { _pw.ProvideUpdate(percentComplete, message, fail); }));
        }

        protected void ProvideUIUpdate(string message, bool fail = false)
        {
            _logOutput.AppendLine(message);
            _dispatch.BeginInvoke((Action)(() => { _pw.ProvideUpdate(message, fail); }));
        }

        protected void SetCloseOnFinish(bool closeOnFinish, int timeout = 0)
        {
            if (closeOnFinish)
            {
                _closeOnSuccessfulFinish = true;
                _closeTimeOut = timeout;
            }
        }
    }
}
