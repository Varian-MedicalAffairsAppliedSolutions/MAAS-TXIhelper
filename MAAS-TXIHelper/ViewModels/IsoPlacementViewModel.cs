using Prism.Commands;
using System;

namespace MAAS_TXIHelper.ViewModels
{
    // Iso spacing (cm) defualt 14 in textbox with up, down increment buttons
    // Number of isocenters on current patient label
    // Number of arcs per isocenter - combobox dropdown (default is 4)
    // Button - Create beams
    // Okay/Cancel message box before save ("About to create X number of arcs and X number of isocenters, (x,y,z) distance appart, do you want to continue?)


    public class IsoPlacementViewModel
    {
        public DelegateCommand CreateBeamsCmd { get; set; }
        public IsoPlacementViewModel()
        {
            CreateBeamsCmd = new DelegateCommand(OnCreateBeams);
        }

        public void OnCreateBeams()
        {
            throw new NotImplementedException();
            // Find body slice that has the most area (or is the thickest)
            // Make sure isocenter linked to z position (red line on halcyon) passes through centroid where body is thickest
            // Starting at the head of the body, place first iso at 1/2 apperture size - 1cm
            //MessageBox.Show("Isocenter placed");
        }
    }
}
