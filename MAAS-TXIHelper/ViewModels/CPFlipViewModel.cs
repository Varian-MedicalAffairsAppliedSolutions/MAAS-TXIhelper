﻿using MAAS_TXIHelper.Core;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using VMS.TPS.Common.Model.API;

// TODO
// Button that shows formula

namespace ViewModels
{

    public class CPFlipViewModel : BindableBase
    {
        private bool _CPFlipEnabled;

        public bool CPFlipEnabled
        {
            get { return _CPFlipEnabled; }
            set { SetProperty(ref _CPFlipEnabled, value); }
        }


        private Patient _Patient;
        public Patient Patient
        {
            get { return _Patient; }
            set { SetProperty(ref _Patient, value); }
        }

        private Course _Course;
        public Course Course
        {
            get { return _Course; }
            set { SetProperty(ref _Course, value); }
        }


        private ExternalPlanSetup _Plan;
        public ExternalPlanSetup Plan
        {
            get { return _Plan; }
            set { SetProperty(ref _Plan, value); }
        }

        public DelegateCommand FlipCmd { get; set; }
        public DelegateCommand SelectLogPathCmd { get; set; }

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

        private string logPath;

        public string LogPath
        {
            get { return logPath; }
            set { SetProperty(ref logPath, value); }
        }




        public CPFlipViewModel(ScriptContext currentContext)
        {

            _Patient = currentContext.Patient;
            _Course = currentContext.Course;
            CPFlipEnabled = true;

            if (currentContext.PlanSetup == null)
            {
                MessageBox.Show("Warning: no plan loaded, plan related tabs are not active.");
                CPFlipEnabled = false;
            }
            else
            {
                _Plan = currentContext.PlanSetup as ExternalPlanSetup;

                /*
                var dirInfo = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
                logPath = Path.Combine(dirInfo.Parent.Name, "TXILog.log");
                MessageBox.Show($"Logpath debug is: {logPath}");*/

                // Get the directory path of the current DLL
                string dllDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                // Build the log file path
                LogPath = Path.Combine(dllDirectory, "TXILog.log");

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
        }

        private void OnSelectLogPath()
        {
            MessageBox.Show("Select log path placeholder");
        }

        private void OnFlip()
        {
            MessageBox.Show("Starting flip CPs per COH code");

            _Patient.BeginModifications();
            bool flipComplete = false;

            if (_IsHalcyon && _IsArcBeamPlan)
            {
                MessageBox.Show("Flipping arc halcyon plan");
                Core.FlipHalcyonArc(_Course, _Plan, LogPath, true, true, true);
                flipComplete = true;
            }

            if (_IsHalcyon && _IsStaticBeamPlan)
            {
                MessageBox.Show("Flipping static halcyon plan");
                Core.FlipHalcyonStatic(_Course, _Plan, LogPath);
                flipComplete = true;
            }

            if (_IsSX2MLC && _IsStaticBeamPlan)
            {
                MessageBox.Show("Flipping static halcyon plan");
                Core.FlipHalcyonStatic(_Course, _Plan, LogPath); // Check if this is the same case
                flipComplete = true;
            }

            if (_IsSX2MLC == false && _IsStaticBeamPlan)
            {
                MessageBox.Show("Flipping static non-halcyon plan");
                Core.FlipStatic(_Course, _Plan, LogPath);
                flipComplete = true;
            }

            if (_IsSX2MLC == false && _IsArcBeamPlan)
            {
                MessageBox.Show("Flipping arc non-halcyon plan");
                Core.FlipArc(_Course, _Plan, LogPath, true, true, true);
                flipComplete = true;
            }

            // Check to make sure one of the cases was triggered
            if (!flipComplete)
            {
                throw new Exception("No flip case caught: Please check plan type and try again.");
            }


            MessageBox.Show("Finished flip CPs");
        }

    }
}
