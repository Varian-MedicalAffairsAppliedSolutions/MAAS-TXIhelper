using MAAS_TXIStandalone.Startup;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using VMS.TPS.Common.Model.API;
using Application = VMS.TPS.Common.Model.API.Application;

namespace MAAS_TXIStandalone
{
    public partial class App : System.Windows.Application
    {
        private Application _app;
        private string _patientId;
        private string _courseId;
        private string _planId;
        private Patient _patient;
        private Course _course;
        private PlanSetup _plan;
        private void shutdown()
        {
            _app.Dispose();

            App.Current.Shutdown();

        }

        private void start(object sender, StartupEventArgs e)
        {
            using (_app = VMS.TPS.Common.Model.API.Application.CreateApplication())
            {

                if (e.Args.Count() > 0 && !String.IsNullOrWhiteSpace(e.Args.First()))
                {

                    _patientId = e.Args.First().Split(';').First().Trim('\"');
                }
                else
                {
                    MessageBox.Show("Patient not specified at application start.");
                    App.Current.Shutdown();
                    return;

                }
                if (e.Args.First().Split(';').Count() > 1)
                {
                    _courseId = e.Args.First().Split(';').ElementAt(1).TrimEnd('\"');
                }
                if (e.Args.First().Split(';').Count() > 2)
                {
                    _planId = e.Args.First().Split(';').ElementAt(2).TrimEnd('\"');
                }
                if (String.IsNullOrWhiteSpace(_patientId) || String.IsNullOrWhiteSpace(_courseId))
                {
                    MessageBox.Show("Patient and/or Course not specified at application start. Please open a patient and course.");
                    App.Current.Shutdown();
                    return;
                }
                _patient = _app.OpenPatientById(_patientId);

                if (!String.IsNullOrWhiteSpace(_courseId))
                {
                    _course = _patient.Courses.FirstOrDefault(x => x.Id == _courseId);
                }
                if (!String.IsNullOrEmpty(_planId))
                {
                    _plan = _course.PlanSetups.FirstOrDefault(x => x.Id == _planId);
                }

                var bootstrap = new Bootstrapper();

                var container = bootstrap.Bootstrap(_app.CurrentUser, _app, _patient, _course, _plan);

                //MV = container.Resolve<MainView>();

                //MV.DataContext = container.Resolve<MainViewModel>();

                //MV.ShowDialog();

                _app.ClosePatient();
            }
        }
    }


}
