using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using VMS.TPS.Common.Model.API;

namespace MAAS_TXIHelper.ViewModels
{
    public class IsoGroup
    {
        public bool IsChecked { get; set; }
        public string Name { get; set; }
    }

    public class ApertureRotationDicomViewModel : BindableBase, INotifyPropertyChanged
    {
        private bool _CPFlipEnabled;

        private string _RotateButtonText = "Rotate the plan";

        public string RotateButtonText
        {
            get => _RotateButtonText;

            set
            {
                if(_RotateButtonText != value)
                {
                    _RotateButtonText = value;
                }
            }
        }

        public bool CPFlipEnabled
        {
            get { return _CPFlipEnabled; }
            set { SetProperty(ref _CPFlipEnabled, value); }
        }
        public string TempDicomDir { get; set; }
        public DelegateCommand OnRotateCmd { get; set; }

        public DelegateCommand CreatePlanCmd { get; set; }

        public ExternalPlanSetup Plan { get; set; }

        public ObservableCollection<IsoGroup> IsoGroups { get; set; }
        public ApertureRotationDicomViewModel(ScriptContext context)
        {
            CPFlipEnabled = true;
            Plan = context.ExternalPlanSetup;
            OnRotateCmd = new DelegateCommand(OnRotate);
            CreatePlanCmd = new DelegateCommand(OnCreatePlan);
            RotateButtonText = "Click to select a plan file for field rotation.";

            // Populate iso groups
            // TEST
            /*
            IsoGroups = new ObservableCollection<IsoGroup>();
            for (int i = 0; i < 10; i++)
            {
                var ig = new IsoGroup();
                ig.Name = $"Group {i}";
                ig.IsChecked = true;
                IsoGroups.Add(ig); 
            }*/
            // TODO: populate with actual iso groups and by default check the lower half
        }

        private void OnCreatePlan()
        {
            // Copy current plan
            // Paste plan and delete all fields not in a selected iso group
            // Paste plan again and delete all fields in selected iso group
            // TODO - autocreate plan sum
            MessageBox.Show($"Please export plan <PlanName_Inf> to selected directory {TempDicomDir}");

        }

        private void OnRotate()
        {
            RotateButtonText = "Creating a rotated plan. Please wait...";
            int result = MAAS_TXIHelper.Core.CPFlipper.PlanFlip();
            if(result >= 0)
            {
                MessageBox.Show("Rotation is complete. New plan saved under folder \"CP_FLIP_OUTPUT\". Please import the new plan and calculate plan dose to verify.");
            }
//            RotateButtonText = "Create a new DICOM plan file with rotated fields.";
        }
    }
}
