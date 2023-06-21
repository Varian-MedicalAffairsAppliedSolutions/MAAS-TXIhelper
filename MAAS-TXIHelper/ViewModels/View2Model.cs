
using Prism.Modularity;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


namespace ViewModels
{
    public class View2Model : BindableBase
    {
       
        private ScriptContext context;

        public View2Model(ScriptContext context)
        {
            // ctor
            this.context = context;

        }

    }
}
