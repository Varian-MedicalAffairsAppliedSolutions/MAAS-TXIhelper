using CommunityToolkit.Mvvm.Input;
using MAAS_TXIHelper.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using I = itk.simple;

namespace MAAS_TXIHelper.ViewModels
{
    internal class SetupViewModel : INotifyPropertyChanged
    {
        private readonly EsapiWorker _worker;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private readonly object _collectionLock;
        private ObservableCollection<string> _setupTypes;
        public ObservableCollection<string> SetupTypes
        {
            get { return _setupTypes; }
            set
            {
                _setupTypes = value;
                BindingOperations.EnableCollectionSynchronization(_setupTypes, _collectionLock);
            }
        }
        private string _SetupTypeSelected;
        public string SetupTypeSelected
        {
            get { return _SetupTypeSelected; }
            set
            {
                _SetupTypeSelected = value;
                IsSetupBtnEnabled = true;
                TextBox = _SetupTypeSelected;
            }
        }
        private bool _IsSetupTypeSelectionEnabled;
        public bool IsSetupTypeSelectionEnabled
        {
            get { return _IsSetupTypeSelectionEnabled; }
            set
            {
                if (_IsSetupTypeSelectionEnabled != value)
                {
                    _IsSetupTypeSelectionEnabled = value;
                    OnPropertyChanged(nameof(IsSetupTypeSelectionEnabled));
                }
            }
        }

        private ObservableCollection<string> _allStructures;
        public ObservableCollection<string> AllStructures
        {
            get { return _allStructures; }
            set
            {
                _allStructures = value;
                BindingOperations.EnableCollectionSynchronization(_allStructures, _collectionLock);
            }
        }
        private string _structureSelected;
        public string StructureSelected
        {
            get { return _structureSelected; }
            set
            {
                _structureSelected = value;
                if(!string.IsNullOrEmpty(SetupTypeSelected) && IsSetupBtnEnabled == false)
                {
                    IsSetupBtnEnabled = true;
                }
            }
        }
        private bool _IsStructureSelectionEnabled;
        public bool IsStructureSelectionEnabled
        {
            get { return _IsStructureSelectionEnabled; }
            set
            {
                if (_IsStructureSelectionEnabled != value)
                {
                    _IsStructureSelectionEnabled = value;
                    OnPropertyChanged(nameof(IsStructureSelectionEnabled));
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
        private bool _IsSetupBtnEnabled;
        public bool IsSetupBtnEnabled
        {
            get { return _IsSetupBtnEnabled; }
            set
            {
                if (_IsSetupBtnEnabled != value)
                {
                    _IsSetupBtnEnabled = value;
                    OnPropertyChanged(nameof(IsSetupBtnEnabled));
                }
            }
        }

        private string _SetupBtnText;
        public string SetupBtnText
        {
            get => _SetupBtnText;

            set
            {
                if (_SetupBtnText != value)
                {
                    _SetupBtnText = value;
                    OnPropertyChanged(nameof(SetupBtnText));
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
        public ICommand SetupCmd { get; }

        public SetupViewModel(EsapiWorker esapiWorker)
        {
            _worker = esapiWorker;
            _collectionLock = new object();
            SetupBtnText = "Click to setup fields.";
            SetupCmd = new RelayCommand(SetupFields);
            SetupTypes = new ObservableCollection<string>();
            AllStructures = new ObservableCollection<string>();
            SetupTypeSelected = null;
            StructureSelected = null;
            ProgressBarValue = 0;
            TextBox = "Please first choose the setup type from the drop-down box above.";
            IsSetupTypeSelectionEnabled = true;
            IsStructureSelectionEnabled = true;
            IsSetupBtnEnabled = false;
            SetupTypes.Add("Isocenters with equal separation distances");
            SetupTypes.Add("Isocenters with unequal separation distances");
            _worker.Run(scriptContext =>
            {
                var ss = scriptContext.StructureSet;
                foreach(var s in ss.Structures)
                {
                    AllStructures.Add(s.Id);
                }
            });
        }
        private void SetupFields()
        {
            // here open a folder dialogue ask the user to select an output folder.
            TextBox += "\n";
            _worker.Run(scriptContext =>
            {
                var patient = scriptContext.Patient;
                var plan = scriptContext.ExternalPlanSetup;
                if (plan == null)
                {
                    TextBox += $"No plan is currently open. Please first open a plan to proceed.\n";
                    return;
                }
                int numTxBeams = 0;
                try
                {
                    foreach (var eachBeam in plan.Beams)
                    {
                        if (eachBeam.IsSetupField == false)
                            numTxBeams++;
                    }
                }
                catch
                {
                    TextBox += $"Error reading beams in this plan. Please verify if this is a valid external beam treatment plan.\n";
                    return;
                }
                if (numTxBeams == 0)
               {
                    TextBox += $"ERROR: This plan does not contain any treatment beam. Due to ESAPI limitations, the external-beam machine cannot be identified without a beam in this treatment plan.\n";
                    TextBox += $"Please include one beam in this plan and try again.\n";
                    return;
                }
                var ss = scriptContext.StructureSet;
                Structure body = ss.Structures.FirstOrDefault(s => s.DicomType == "EXTERNAL" && s.Volume > 0);
                if (body == null)
                {
                    TextBox += $"ERROR: The body contour was not found. Please create the external body structure first.\n";
                    return;
                }
                VVector bodyCenter = body.CenterPoint;
                if (string.IsNullOrEmpty(StructureSelected))
                {
                    TextBox += $"ERROR: Please select the combined target contour from the drop-down structure list above.\n";
                    return;
                }
                else
                {
                    TextBox += $"Selected structure: {StructureSelected}.\n";
                }
                Structure structurePTV = ss.Structures.FirstOrDefault(s => s.Id == StructureSelected);
                var zSize = ss.Image.ZSize;
                double maxWidth = 0;
                for (int zIndex = 0; zIndex < zSize; zIndex++)
                {
                    var points = body.GetContoursOnImagePlane(zIndex);
                    double minimum = 0.0, maximum = 0.0;
                    for (int i = 0; i < points.Length; i++)
                    {
                        for (int j = 0; j < points[i].Length; j++)
                        {
                            if (minimum > points[i][j][0])
                            {
                                minimum = points[i][j][0];
                            }
                            if (maximum < points[i][j][0])
                            {
                                maximum = points[i][j][0];
                            }
                        }
                    }
                    var width = maximum - minimum;
                    if (maxWidth < width) maxWidth = width;
                }
                TextBox += $"Body maximum body width = {string.Format("{0:F1}", maxWidth / 10.0)} cm.\n";
                maxWidth = 0;
                for (int zIndex = 0; zIndex < zSize; zIndex++)
                {
                    var points = structurePTV.GetContoursOnImagePlane(zIndex);
                    double minimum = 0.0, maximum = 0.0;
                    for (int i = 0; i < points.Length; i++)
                    {
                        for (int j = 0; j < points[i].Length; j++)
                        {
                            if (minimum > points[i][j][0])
                            {
                                minimum = points[i][j][0];
                            }
                            if (maximum < points[i][j][0])
                            {
                                maximum = points[i][j][0];
                            }
                        }
                    }
                    var width = maximum - minimum;
                    if (maxWidth < width) maxWidth = width;
                }
                TextBox += $"{structurePTV.Id} maximum width = {string.Format("{0:F1}", maxWidth / 10.0)} cm.\n";
                bool useLateralIsos = false;
                if(maxWidth > 520)
                {
                    DialogResult result = System.Windows.Forms.MessageBox.Show(
                                $"Structure {structurePTV.Id} width is > 52 cm. Use laterally shifted isocenters?",       // Message
                                "Confirmation",                   // Title
                                MessageBoxButtons.YesNo,          // Buttons
                                MessageBoxIcon.Question           // Icon
                            );
                    if (result == DialogResult.Yes)
                    {
                        useLateralIsos = true;
                    }
                }
                string machindId = plan.Beams.FirstOrDefault().TreatmentUnit.Id;
                TextBox += $"We will add beams for this machine: {machindId}.\n";
                TextBox += "Start setting up fields.\n";
                // First check existin beams:
                // beam.IsocenterPosition.x/y/z gives DICOM coordinates (independent of user origin definition)
                List<string> beamIds = new List<string>();
                foreach (var eachBeam in plan.Beams)
                {
                    beamIds.Add(eachBeam.Id);
                    if (eachBeam.IsSetupField)
                    {
                        continue;
                    }
                    TextBox += $"Beam isocenter: {eachBeam.IsocenterPosition.x} {eachBeam.IsocenterPosition.y} {eachBeam.IsocenterPosition.z}\n";
                    TextBox += $"Machine model: {eachBeam.TreatmentUnit.MachineModel} name: {eachBeam.TreatmentUnit.MachineModelName} {eachBeam.MLC.Id}\n";
                }
                var beam = plan.Beams.Where(b => b.IsSetupField == false).First();
                string energyModeId, primaryFluenceModeId;
                energyModeId = beam.EnergyModeDisplayName;
                if (energyModeId.Contains("-") && energyModeId.Split('-')[1] == "FFF")
                {
                    primaryFluenceModeId = energyModeId.Split('-')[1];
                    energyModeId = energyModeId.Split('-')[0];
                }
                else
                {
                    primaryFluenceModeId = null;
                }
                TextBox += $"Unit ID: {beam.TreatmentUnit.Id} Energy: \n";
                ExternalBeamMachineParameters machineParameters = new ExternalBeamMachineParameters(
                    beam.TreatmentUnit.Id, 
                    energyModeId,
                    beam.DoseRate, 
                    beam.Technique.ToString(), 
                    primaryFluenceModeId);
                VVector isoPosition = beam.IsocenterPosition;
                isoPosition = new VVector(0, 0, 0);
                patient.BeginModifications();
                // Remove the old treatment beams.
                foreach (string eachTxBeamId in beamIds)
                {
                    try
                    {
                        plan.RemoveBeam(plan.Beams.Where(b => b.Id == eachTxBeamId).Single());
                    }
                    catch (Exception e)
                    {
                        string log = e.ToString();
                        TextBox += $"Failed to remove one existing beam: \"{eachTxBeamId}\". Please remove it manually.\n";
                    }
                }
                // Control points
                int numCP = 20;
                List<double> metersetWeights = Enumerable.Range(0, numCP)
                    .Select(i => (double)i / (numCP - 1))
                    .ToList();
                // Analyze the geometry of the selected structure
                var zRes = ss.Image.ZRes;
                var zStart = ss.Image.Origin[2];
                var xRes = ss.Image.XRes;
                int startSlice = 0, endSlice = 0;
                bool structureExists = false;
                double firstSliceZ = 0, lastSliceZ = 0;
                for (int zIndex = 0; zIndex < zSize; zIndex++)
                {
                    double zCoordinate = zStart + zIndex * zRes;
                    var points = structurePTV.GetContoursOnImagePlane(zIndex);
                    if (points.Length > 0)
                    {
                        if (!structureExists)
                        {
                            firstSliceZ = zCoordinate;
                            startSlice = zIndex;
                            structureExists = true;
                        }
                        if (endSlice <= zIndex)
                        {
                            lastSliceZ = zCoordinate;
                            endSlice = zIndex;
                        }
                    }
                }
                double structureLength = (endSlice - startSlice) * zRes;
                TextBox += $"Structure {structurePTV.Id} contours starts at slice {startSlice} and end at {endSlice}, with a total length: {structureLength} mm.\n";
                VVector beamIso = new VVector();
                beamIso.x = bodyCenter.x;
                beamIso.y = bodyCenter.y;
                if (structureLength < 260)
                {
                    beamIso.z = structurePTV.CenterPoint.z;
                }
                else {
                    beamIso.z = lastSliceZ + 20 - 140;
                }
                TextBox += $"Zstart: {zStart} Z res: {zRes} Last Z Coord: {lastSliceZ}.\n";
                TextBox += $"Isocenter: {beamIso.z} mm.\n";
                plan.AddVMATBeamForFixedJaws(
                    machineParameters: machineParameters, 
                    metersetWeights: metersetWeights,
                    collimatorAngle: 0, 
                    gantryStartAngle: 181, 
                    gantryStopAngle: 179, 
                    gantryDir: GantryDirection.Clockwise, 
                    patientSupportAngle: 0, 
                    isocenter: beamIso);
                plan.AddVMATBeamForFixedJaws(
                    machineParameters: machineParameters,
                    metersetWeights: metersetWeights,
                    collimatorAngle: 90,
                    gantryStartAngle: 179,
                    gantryStopAngle: 181,
                    gantryDir: GantryDirection.CounterClockwise,
                    patientSupportAngle: 0,
                    isocenter: beamIso);
                VRect<double> fieldSize = new VRect<double>(-14, 14, -14, 14);
                plan.AddSetupBeam(machineParameters: machineParameters, 
                    jawPositions: fieldSize,
                    collimatorAngle: 0,
                    gantryAngle: 0,
                    patientSupportAngle: 0,
                    isocenter: beamIso);
                // continue adding beams until the PTV is covered.
                bool smallShift = false;
                while (beamIso.z - 140 > firstSliceZ - 10)
                {
                    if (smallShift)
                    {
                        beamIso.z = beamIso.z - 105;
                        smallShift = false;
                    }
                    else
                    {
                        beamIso.z = beamIso.z - 140;
                        if (SetupTypeSelected == "Isocenters with unequal separation distances")
                        {
                            smallShift = true;
                        }
                    }
                    if (useLateralIsos)
                    {
                        double xCentral = beamIso.x;
                        beamIso.x = xCentral + 70;
                        plan.AddVMATBeamForFixedJaws(
                            machineParameters: machineParameters,
                            metersetWeights: metersetWeights,
                            collimatorAngle: 0,
                            gantryStartAngle: 181,
                            gantryStopAngle: 179,
                            gantryDir: GantryDirection.Clockwise,
                            patientSupportAngle: 0,
                            isocenter: beamIso);
                        plan.AddVMATBeamForFixedJaws(
                            machineParameters: machineParameters,
                            metersetWeights: metersetWeights,
                            collimatorAngle: 90,
                            gantryStartAngle: 179,
                            gantryStopAngle: 181,
                            gantryDir: GantryDirection.CounterClockwise,
                            patientSupportAngle: 0,
                            isocenter: beamIso);
                        beamIso.x = xCentral - 70;
                        plan.AddVMATBeamForFixedJaws(
                            machineParameters: machineParameters,
                            metersetWeights: metersetWeights,
                            collimatorAngle: 0,
                            gantryStartAngle: 181,
                            gantryStopAngle: 179,
                            gantryDir: GantryDirection.Clockwise,
                            patientSupportAngle: 0,
                            isocenter: beamIso);
                        plan.AddVMATBeamForFixedJaws(
                            machineParameters: machineParameters,
                            metersetWeights: metersetWeights,
                            collimatorAngle: 90,
                            gantryStartAngle: 179,
                            gantryStopAngle: 181,
                            gantryDir: GantryDirection.CounterClockwise,
                            patientSupportAngle: 0,
                            isocenter: beamIso);
                        beamIso.x = xCentral;
                    }
                    else
                    {
                        plan.AddVMATBeamForFixedJaws(
                            machineParameters: machineParameters,
                            metersetWeights: metersetWeights,
                            collimatorAngle: 0,
                            gantryStartAngle: 181,
                            gantryStopAngle: 179,
                            gantryDir: GantryDirection.Clockwise,
                            patientSupportAngle: 0,
                            isocenter: beamIso);
                        plan.AddVMATBeamForFixedJaws(
                            machineParameters: machineParameters,
                            metersetWeights: metersetWeights,
                            collimatorAngle: 90,
                            gantryStartAngle: 179,
                            gantryStopAngle: 181,
                            gantryDir: GantryDirection.CounterClockwise,
                            patientSupportAngle: 0,
                            isocenter: beamIso);
                    }
                }
                TextBox += $"Please review changes made to this plan. If you are satisfied with the changes, you can save this plan.\n";
            });
        }
    }
}