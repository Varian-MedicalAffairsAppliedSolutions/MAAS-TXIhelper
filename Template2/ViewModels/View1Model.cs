using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml;
using System.Collections.ObjectModel;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Threading;
using System.Net;
using System.IO;

using System.Runtime.CompilerServices;
using System.Windows.Media;
using Prism.Mvvm;

namespace ViewModels
{


    public class View1Model : BindableBase
    {
        private ScriptContext context;

        public View1Model(ScriptContext currentContext)
        {

            this.context = currentContext;

        }
                  
    }
}
