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
using MAAS_TXIHelper.Models;
using Prism.Commands;
using System.Numerics;
using JR.Utils.GUI.Forms;
using System.Windows.Controls;
using System.Resources;
using MAAS_TXIHelper.CustomWidgets;
using System.Windows.Controls.Primitives;
using Views;

// TODO
// Button that shows formula

namespace ViewModels
{

    public class View1Model : BindableBase
    {
        private ScriptContext _Context;
        private Patient _Patient {get; set;}
        private Course _Course { get; set;}
        private ExternalPlanSetup _Plan { get; set;}

        public DelegateCommand FlipCmd { get; set;} 
        public DelegateCommand SelectLogPathCmd { get; set;}

        private bool _IsStaticBeamPlan;

        public bool IsStaticBeamPlan
        {
            get { return _IsStaticBeamPlan; }
            set { SetProperty(ref _IsStaticBeamPlan, value); }
        }

        private bool _IsDynamicBeamPlan;

        public bool IsDynamicBeamPlan
        {
            get { return _IsDynamicBeamPlan; }
            set { SetProperty(ref _IsDynamicBeamPlan, value); }
        }

        private bool _IsArcBeamPlan;

        public bool IsArcBeamPlan
        {
            get { return _IsArcBeamPlan; }
            set { SetProperty(ref _IsArcBeamPlan, value); }
        }

        private bool _IsSX2MLC;

        public bool IsSX2MLC
        {
            get { return _IsSX2MLC; }
            set { SetProperty(ref _IsSX2MLC, value); }
        }

        private bool _IsHalcyon;

        public bool IsHalcyon
        {
            get { return _IsHalcyon; }
            set { SetProperty(ref _IsHalcyon, value); }
        }



        public View1Model(ScriptContext currentContext)
        {

            _Context = currentContext;
            _Patient = currentContext.Patient;
            _Course = currentContext.Course;
            _Plan = currentContext.PlanSetup as ExternalPlanSetup;

            FlipCmd = new DelegateCommand(OnFlip);
            SelectLogPathCmd = new DelegateCommand(OnSelectLogPath);

            // Determine what type of mlc and technique we are using
            _IsStaticBeamPlan = false;
            _IsDynamicBeamPlan = false;
            _IsArcBeamPlan = false;
            _IsSX2MLC = false;
            _IsHalcyon = false;
            foreach (var beam in _Plan.Beams)
            {
                if (beam.IsSetupField == false && beam.IsImagingTreatmentField == false)
                {
                    if (beam.Technique.ToString() == "STATIC")
                    {
                        // Here we assume that if one treatment beam is a static beam, then all the beams
                        // in this treatment plan must be static beams.
                        _IsStaticBeamPlan = true;
                    }
                    if (beam.Technique.ToString().ToLower().Contains("arc"))
                    {
                        // Here we assume that if one treatment beam is a VMAT or arc beam, then all the beams
                        // in this treatment plan must be VMAT or arc beams.
                        _IsArcBeamPlan = true;
                    }
                    if (beam.MLC.Model == "SX2")
                    {
                        // Here we assume that it is a Halcyon or Ethos linac, because the "SX2" MLC only
                        // appears on such linacs.
                        _IsSX2MLC = true;
                    }
                    if (beam.TreatmentUnit.MachineModel == "RDS")
                    {
                        // Here we assume that it is a Halcyon or Ethos linac
                        _IsHalcyon = true;
                    }
                }
            }

        }

        private void OnSelectLogPath()
        {
            MessageBox.Show("Select log path placeholder");
        }

        private void OnFlip()
        {
            MessageBox.Show("Starting flip CPs per COH code");
            MessageBox.Show("Finished flip CPs");
        }

        

       

    }
}
