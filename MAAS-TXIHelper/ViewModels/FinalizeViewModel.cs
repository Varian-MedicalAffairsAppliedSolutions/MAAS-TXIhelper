using CommunityToolkit.Mvvm.Input;
using MAAS_TXIHelper.Views;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace MAAS_TXIHelper.ViewModels
{
    internal class FinalizeViewModel : INotifyPropertyChanged
    {
        private readonly EsapiWorker _worker;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private bool _isFinalizeBtnEnabled;
        public bool IsFinalizeBtnEnabled
        {
            get { return _isFinalizeBtnEnabled; }
            set
            {
                if (_isFinalizeBtnEnabled != value)
                {
                    _isFinalizeBtnEnabled = value;
                    OnPropertyChanged(nameof(IsFinalizeBtnEnabled));
                }
            }
        }
        private int _pbValue;
        public int ProgressBarValue
        {
            get { return _pbValue; }
            set
            {
                if (_pbValue != value)
                {
                    _pbValue = value;
                    OnPropertyChanged(nameof(ProgressBarValue));
                }
            }
        }
        public string _TextBox;
        public string TextBox
        {
            get { return _TextBox; }
            set
            {
                if (_TextBox != value)
                {
                    _TextBox = value;
                    OnPropertyChanged(nameof(TextBox));
                }
            }
        }
        public ICommand FinalizeCmd { get; }
        public FinalizeViewModel(EsapiWorker esapiWorker)
        {
            _worker = esapiWorker;
            ProgressBarValue = 0;
            FinalizeCmd = new RelayCommand(FinalizePlan);
            IsFinalizeBtnEnabled = true;
            TextBox = "Please check that the plan to finalize is currently open on Eclipse.\n";
        }
        private void FinalizePlan()
        {
            _worker.Run(scriptContext =>
            {
                var patient = scriptContext.Patient;
                if (scriptContext.PlanSetup == null && scriptContext.PlanSum == null)
                {
                    TextBox += $"No plan selected. Please select a plan or plan sum to proceed.\n";
                    return;
                }
                IList<PlanSetup> planSetups = new List<PlanSetup>();
                if (scriptContext.PlanSetup != null)
                {
                    planSetups.Add(scriptContext.PlanSetup);
                }
                else if (scriptContext.PlanSum != null)
                {
                    TextBox += $"This plan sum is currently open: {scriptContext.PlanSum.Id}. All the included plans will be processed.\n";
                    planSetups = scriptContext.PlanSum.PlanSetups.ToList();
                }
                foreach(PlanSetup planSetup in planSetups)
                {
                    var extPlanSetup = planSetup as ExternalPlanSetup;
                    var currentCourse = extPlanSetup.Course;
                    TextBox += $"Processing plan: {extPlanSetup.Id}\n";
                    // first review this plan to find out isocenter data
                    ICollection<VVector> isocenters = new List<VVector>();
                    foreach (var beam in extPlanSetup.Beams.ToList())
                    {
                        TextBox += $"Beam ID: {beam.Id}   Iso: {beam.IsocenterPosition.x:F2} {beam.IsocenterPosition.y:F2} {beam.IsocenterPosition.z:F2}\n";
                        VVector isoCoord = new VVector(beam.IsocenterPosition.x, beam.IsocenterPosition.y, beam.IsocenterPosition.z);
                        if (isocenters.Contains(isoCoord) == false)
                        {
                            isocenters.Add(isoCoord);
                            TextBox += "Added one isoenter\n";
                        }
                    }
                    TextBox += $"Number of isocenters: {isocenters.Count}\n";
                    patient.BeginModifications();
                    int plansCreated = 0;
                    foreach (var isoCoord in isocenters)
                    {
                        ExternalPlanSetup newPlan = currentCourse.CopyPlanSetup(planSetup) as ExternalPlanSetup;
                        TextBox += $"ID of the new plan: {newPlan.Id}\n";
                        foreach (var beam in newPlan.Beams.ToList())
                        {
                            if (beam.IsocenterPosition.x != isoCoord.x || beam.IsocenterPosition.y != isoCoord.y || beam.IsocenterPosition.z != isoCoord.z)
                            {
                                newPlan.RemoveBeam(beam);
                            }
                        }
                        bool imagingFieldExists = false;
                        foreach (var beam in newPlan.Beams.ToList())
                        {
                            if (beam.IsocenterPosition.x == isoCoord.x &&
                            beam.IsocenterPosition.y == isoCoord.y &&
                            beam.IsocenterPosition.z == isoCoord.z
                            && beam.IsImagingTreatmentField
                            )
                                imagingFieldExists = true;
                        }
                        // next create imaging field(s) in the new plan.
                        if (imagingFieldExists == false)
                        {
                            var param = new ExternalBeamMachineParameters(newPlan.Beams.First().TreatmentUnit.Id);
                            var imagingSetupParam = new ImagingBeamSetupParameters(ImagingSetup.kVCBCT, 0, 0, 0, 0, 28, 28);
                            newPlan.AddImagingSetup(param, imagingSetupParam, null);
                        }

                        plansCreated++;
                        int i = (int)((plansCreated + 0.0f) / (isocenters.Count + 0.0f) * 100 - 2);
                        if (i < 0)
                        {
                            i = 0;
                        }
                        ProgressBarValue = i;
                    }
                    ProgressBarValue = 100;
                }
            });
        }
    }
}