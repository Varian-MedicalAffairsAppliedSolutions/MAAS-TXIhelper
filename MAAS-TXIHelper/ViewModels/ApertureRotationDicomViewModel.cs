using Prism.Commands;
using Prism.Common;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;

namespace MAAS_TXIHelper.ViewModels
{
    public class IsoGroup
    {
        public bool IsChecked { get; set; }
        public string Name { get; set; }
    }


    public class ApertureRotationDicomViewModel :BindableBase
    {
        public string TempDicomDir { get; set; }    
        public DelegateCommand OnRotateCmd { get; set; }

        public DelegateCommand CreatePlanCmd { get; set; }

        public ExternalPlanSetup Plan { get; set; }

        public ObservableCollection<IsoGroup> IsoGroups { get; set; }
        public ApertureRotationDicomViewModel(ScriptContext context) {

            Plan = context.ExternalPlanSetup;
            OnRotateCmd = new DelegateCommand(onRotate);
            CreatePlanCmd = new DelegateCommand(onCreatePlan);
            TempDicomDir = @"C:\Temp";

            // Populate iso groups
            
            // TEST
            IsoGroups = new ObservableCollection<IsoGroup>();
            for (int i = 0; i < 10; i++)
            {
                var ig = new IsoGroup();
                ig.Name = $"Group {i}";
                ig.IsChecked = true;
                IsoGroups.Add(ig); 
            }

            // TODO: populate with actual iso groups and by default check the lower half

            
        }

        private void onCreatePlan()
        {
            // Copy current plan
            // Paste plan and delete all fields not in a selected iso group
            // Paste plan again and delete all fields in selected iso group
            // TODO - autocreate plan sum
            MessageBox.Show($"Please export plan <PlanName_Inf> to selected directory {TempDicomDir}");

        }

        private void onRotate()
        {
            // 
            MessageBox.Show("ON rotate clicked");
            // DO roatation
            MessageBox.Show("Rotation complete. New plan saved to <newfile_rotated_plan>. Import plan and create plan sum to verify.");
        }
    }
}
