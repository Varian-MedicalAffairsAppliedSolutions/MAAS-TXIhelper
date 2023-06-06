using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using System.ComponentModel;
using Autofac;
using MAAS_TXIStandalone.Views;
using MAAS_TXIStandalone.ViewModels;

namespace MAAS_TXIStandalone.Startup
{
    public class Bootstrapper
    {
        public Autofac.IContainer Bootstrap(User user, Application app, Patient pat, Course crs, PlanSetup plan)
        {
            var container = new ContainerBuilder();
            //esapi components.            
            container.RegisterInstance<User>(user);
            container.RegisterInstance<Application>(app);
            container.RegisterInstance<Patient>(pat);
            container.RegisterInstance<Course>(crs);
            container.RegisterInstance<PlanSetup>(plan);

            //startup components.
            container.RegisterType<MainView>().AsSelf();
            container.RegisterType<MainViewModel>().AsSelf();

            return container.Build();
        }
    }
}