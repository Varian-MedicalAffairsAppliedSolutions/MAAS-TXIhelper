using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using MAAS_TXIHelper.Views;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using System.Reflection;
using System.Windows;
using System.Data;
using FellowOakDicom;
using System.Windows.Forms;

namespace MAAS_TXIHelper.ViewModels
{
    internal class RotateViewModel : INotifyPropertyChanged
    {
        private readonly EsapiWorker _worker;

        private static object obj = new object();

        private string _RotateBtnText;
        public string RotateBtnText
        {
            get => _RotateBtnText;

            set
            {
                if (_RotateBtnText != value)
                {
                    _RotateBtnText = value;
                    OnPropertyChanged(nameof(RotateBtnText));
                }
            }
        }
        private bool _IsRotateBtnEnabled;
        public bool IsRotateBtnEnabled
        {
            get => _IsRotateBtnEnabled;
            set
            {
                if (_IsRotateBtnEnabled != value)
                {
                    _IsRotateBtnEnabled = value;
                }
                OnPropertyChanged(nameof(IsRotateBtnEnabled));
            }
        }
        public ICommand RotateCmd { get; }

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
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public RotateViewModel(EsapiWorker esapiWorker)
        {
            _worker = esapiWorker;
            RotateCmd = new RelayCommand(RotatePlan);
            TextBox = "Please first export the plan to rotate to a DICOM file, then use the button to select this file.";
            IsRotateBtnEnabled = true;
            RotateBtnText = "Select DICOM plan file for rotation of patient orientation.";
        }
        private void RotatePlan()
        {
            string dllDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string logfilePath = Path.Combine(dllDirectory, "TXIlog.log");
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.DefaultExt = ".dcm";
            openFileDialog.Filter = "DICOM Files (*.dcm)|*.dcm";
            bool? result = openFileDialog.ShowDialog() ?? false;
            string filename = "";
            if (result == true)
            {
                filename = openFileDialog.FileName;
            }
            if (File.Exists(filename) == false)
            {
                System.Windows.MessageBox.Show("Input file does not exist.");
                return;
            }
            // here open a folder dialogue ask the user to select an output folder.
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select the directory where new files will be saved.";
            DialogResult openFolderResult = dialog.ShowDialog();
            if (openFolderResult != DialogResult.OK)
            {
                System.Windows.MessageBox.Show("Please select a directory for output.");
                return;
            }
            var folderPath = dialog.SelectedPath;
            TextBox = $"The new DICOM plan file will be saved in: {folderPath}";
            IsRotateBtnEnabled = false;
            TextBox += "Starting plan rotation.\n";
            // Loop through beams
            bool isTrueBeam = false;
            bool isHalcyon = false;
            DicomDataset dataset = DicomFile.Open(filename).Dataset;
            foreach (DicomDataset beam in dataset.GetSequence(DicomTag.BeamSequence))
            {
                if (beam.GetString(DicomTag.TreatmentDeliveryType) == "TREATMENT")
                {
                    foreach (DicomDataset cp in beam.GetSequence(DicomTag.ControlPointSequence))
                    {
                        foreach (DicomDataset pos in cp.GetSequence(DicomTag.BeamLimitingDevicePositionSequence))
                        {
                            string deviceType = pos.GetString(DicomTag.RTBeamLimitingDeviceType);
                            if (deviceType == "MLCX")
                            {
                                isTrueBeam = true; break;
                            }
                            if (deviceType == "MLCX1" || deviceType == "MLCX2")
                            {
                                isHalcyon = true; break;
                            }
                        }
                        if (isTrueBeam || isHalcyon)
                            break;
                    }
                }
            }
            if (isTrueBeam)
            {
                TextBox += "A TrueBeam plan is found.\n";
                _ = PlanFlipVMAT(filename, folderPath);
            }
            else if (isHalcyon)
            {
                TextBox += "A Halcyon/Ethod plan is found.\n";
                _ = PlanFlipHalcyon(filename, folderPath);
            }
            TextBox += "Plan rotation is complete.\n";
            TextBox += "Please find the new DICOM plan file in the newly created folder and import the data into Eclipse for verification.\n";
            IsRotateBtnEnabled = true;
        }
        public static int PlanFlipVMAT(string filename, string folderPath)
        {
            if (File.Exists(filename) == false)
            {
                System.Windows.MessageBox.Show("Input file does not exist.");
                return -1;
            }
            DicomFile file = DicomFile.Open(filename);
            DicomDataset dataset = file.Dataset;

            string dllDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string logfilePath = Path.Combine(dllDirectory, "TXIlog.log");
            /* string outputDir = Path.Combine(Path.GetDirectoryName(filename), "CP_FLIP_OUTPUT");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            } */
            // Loop through beams
            foreach (DicomDataset beam in dataset.GetSequence(DicomTag.BeamSequence))
            {
                if (beam.GetString(DicomTag.TreatmentDeliveryType) == "TREATMENT")
                {
                    lock (obj)
                    {
                        File.AppendAllText(logfilePath, $"Field type: {beam.GetString(DicomTag.TreatmentDeliveryType)}\n");
                        File.AppendAllText(logfilePath, $"# of CP: {beam.GetSequence(DicomTag.ControlPointSequence).Count()}\n");
                    }
                    foreach (DicomDataset cp in beam.GetSequence(DicomTag.ControlPointSequence))
                    {
                        lock (obj)
                        {
                            File.AppendAllText(logfilePath, $"Direction: {cp.GetString(DicomTag.GantryRotationDirection)}\n");
                        }
                        if (cp.GetString(DicomTag.GantryRotationDirection) == "CC")
                        {
                            cp.AddOrUpdate(DicomTag.GantryRotationDirection, "CW");
                        }
                        else if (cp.GetString(DicomTag.GantryRotationDirection) == "CW")
                        {
                            cp.AddOrUpdate(DicomTag.GantryRotationDirection, "CC");
                        }
                        cp.AddOrUpdate(DicomTag.GantryAngle, 360 - cp.GetSingleValueOrDefault<double>(DicomTag.GantryAngle, 0.0));

                        foreach (DicomDataset pos in cp.GetSequence(DicomTag.BeamLimitingDevicePositionSequence))
                        {
                            string deviceType = pos.GetString(DicomTag.RTBeamLimitingDeviceType);
                            if (deviceType == "ASYMX" || deviceType == "ASYMY")
                            {
                                double[] jawPositions = pos.GetValues<double>(DicomTag.LeafJawPositions);
                                double temp = jawPositions[0];
                                jawPositions[0] = -jawPositions[1];
                                jawPositions[1] = -temp;
                                pos.AddOrUpdate(DicomTag.LeafJawPositions, jawPositions);
                            }
                            else if (deviceType == "MLCX")
                            {
                                double[] leafPositions = pos.GetValues<double>(DicomTag.LeafJawPositions);
                                for (int i = 0; i < leafPositions.Length / 2; i++)
                                {
                                    double temp = leafPositions[i];
                                    leafPositions[i] = -leafPositions[leafPositions.Length - i - 1];
                                    leafPositions[leafPositions.Length - i - 1] = -temp;
                                }
                                pos.AddOrUpdate(DicomTag.LeafJawPositions, leafPositions);
                            }
                        }
                    }
                }
            }
            // next, change patient setup orientation
            foreach (DicomDataset beam in dataset.GetSequence(DicomTag.PatientSetupSequence))
            {
                if (beam.GetString(DicomTag.PatientPosition) == "HFS")
                {
                    beam.AddOrUpdate(DicomTag.PatientPosition, "FFS");
                }
            }
            // next, assign a new SOP instance UID to the new plan
            string oldUID = dataset.GetString(DicomTag.SOPInstanceUID);
            string newUID = MakeNewUID(oldUID);
            dataset.AddOrUpdate(DicomTag.SOPInstanceUID, newUID);
            // update instance creation date and time
            dataset.AddOrUpdate(DicomTag.InstanceCreationDate, DateTime.Now.ToString("yyyyMMdd"));
            dataset.AddOrUpdate(DicomTag.InstanceCreationTime, DateTime.Now.ToString("HHmmss"));
            // assign a new plan ID
            string oldPlanLabel = dataset.GetString(DicomTag.RTPlanLabel);
            dataset.AddOrUpdate(DicomTag.RTPlanLabel, oldPlanLabel + '1');
            string newFilename = Path.Combine(folderPath, Path.GetFileName(filename));
            file.Save(newFilename);
            return 0;
        }

        public static int PlanFlipHalcyon(string filename, string folderPath)
        {
            if (File.Exists(filename) == false)
            {
                System.Windows.MessageBox.Show("Input file does not exist.");
                return -1;
            }
            DicomFile file = DicomFile.Open(filename);
            DicomDataset dataset = file.Dataset;

            string dllDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string logfilePath = Path.Combine(dllDirectory, "TXIlog.log");
            /* string outputDir = Path.Combine(Path.GetDirectoryName(filename), "CP_FLIP_OUTPUT");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            } */
            lock (obj)
            {
                File.AppendAllText(logfilePath, $"Start of Halcyon plan rotation.\n");
            }
            foreach (DicomDataset beam in dataset.GetSequence(DicomTag.BeamSequence))
            {
                if (beam.GetString(DicomTag.TreatmentDeliveryType) == "TREATMENT")
                {
                    lock (obj)
                    {
                        File.AppendAllText(logfilePath, $"Field type: {beam.GetString(DicomTag.TreatmentDeliveryType)}\n");
                        File.AppendAllText(logfilePath, $"# of CP: {beam.GetSequence(DicomTag.ControlPointSequence).Count()}\n");
                    }
                    int cpIndex = 0;
                    foreach (DicomDataset cp in beam.GetSequence(DicomTag.ControlPointSequence))
                    {
                        cpIndex++;
                        if (cp.GetString(DicomTag.GantryRotationDirection) == "CC")
                        {
                            cp.AddOrUpdate(DicomTag.GantryRotationDirection, "CW");
                        }
                        else if (cp.GetString(DicomTag.GantryRotationDirection) == "CW")
                        {
                            cp.AddOrUpdate(DicomTag.GantryRotationDirection, "CC");
                        }
                        cp.AddOrUpdate(DicomTag.GantryAngle, 360 - cp.GetSingleValueOrDefault<double>(DicomTag.GantryAngle, 0.0));
                        foreach (DicomDataset pos in cp.GetSequence(DicomTag.BeamLimitingDevicePositionSequence))
                        {
                            string deviceType = pos.GetString(DicomTag.RTBeamLimitingDeviceType);
                            if (deviceType == "MLCX1" || deviceType == "MLCX2")
                            {
                                double[] leafPositions = pos.GetValues<double>(DicomTag.LeafJawPositions);
                                double[] temp = new double[leafPositions.Length];
                                Array.Copy(leafPositions, temp, leafPositions.Length);
                                int leafPairNum = (int)leafPositions.Length / 2;
                                for (int i = 0; i < leafPairNum; i++)
                                {
                                    if (Math.Abs(temp[leafPositions.Length - 1 - i]) > 139.5 && Math.Abs(temp[leafPairNum - 1 - i]) > 139.5 &&
                                        temp[leafPositions.Length - 1 - i] == temp[leafPairNum - 1 - i])
                                    {
                                        leafPositions[i] = temp[leafPositions.Length - 1 - i];
                                        leafPositions[leafPairNum + i] = temp[leafPairNum - 1 - i];
                                    }
                                    else
                                    {
                                        leafPositions[i] = -temp[leafPositions.Length - 1 - i];
                                        leafPositions[leafPairNum + i] = -temp[leafPairNum - 1 - i];
                                    }
                                }
                                pos.AddOrUpdate(DicomTag.LeafJawPositions, leafPositions);
                            }
                        }
                    }
                }
            }
            // next, change patient setup orientation
            foreach (DicomDataset beam in dataset.GetSequence(DicomTag.PatientSetupSequence))
            {
                if (beam.GetString(DicomTag.PatientPosition) == "HFS")
                {
                    beam.AddOrUpdate(DicomTag.PatientPosition, "FFS");
                }
            }
            // next, assign a new SOP instance UID to the new plan
            string oldUID = dataset.GetString(DicomTag.SOPInstanceUID);
            string newUID = MakeNewUID(oldUID);
            dataset.AddOrUpdate(DicomTag.SOPInstanceUID, newUID);
            // update instance creation date and time
            dataset.AddOrUpdate(DicomTag.InstanceCreationDate, DateTime.Now.ToString("yyyyMMdd"));
            dataset.AddOrUpdate(DicomTag.InstanceCreationTime, DateTime.Now.ToString("HHmmss"));
            // assign a new plan ID
            string oldPlanLabel = dataset.GetString(DicomTag.RTPlanLabel);
            dataset.AddOrUpdate(DicomTag.RTPlanLabel, oldPlanLabel + '1');
            string newFilename = Path.Combine(folderPath, Path.GetFileName(filename));
            file.Save(newFilename);
            return 0;
        }
        private static string MakeNewUID(string uid)
        {
            string newUID = "";
            string timeTag = DateTime.Now.ToString("yyMMddHHmmssff");
            string[] segments = uid.Split('.');
            if (segments.Length > 5)
            {
                segments[segments.Length - 2] = timeTag;
            }
            else
            {
                segments[segments.Length - 1] = timeTag;
            }
            foreach (string segment in segments)
            {
                newUID += segment + '.';
            }
            newUID = newUID.Substring(0, newUID.Length - 1);
            if (segments.Length == 5)
            {
                return newUID;
            }
            if (segments.Length == 6 && newUID.Length > 64)
            {
                int deltaLength = newUID.Length - 64;
                segments[5] = segments[deltaLength];
                foreach (string segment in segments)
                {
                    newUID += segment + '.';
                }
                newUID = newUID.Substring(0, newUID.Length - 1);
                return newUID;
            }
            if (segments.Length > 6 && newUID.Length > 64)  // truncate some digits since the UID length needs to be no more than 64.
            {
                segments = newUID.Split('.');
                int deltaLength = newUID.Length - 64;
                string root = "";
                for (int i = 0; i < 4; i++)
                {
                    root += segments[i] + ".";
                }
                string mid = "";
                for (int i = 4; i < segments.Length - 2; i++)
                {
                    mid += segments[i] + '.';
                }
                string tail = "";
                for (int i = segments.Length - 2; i < segments.Length; i++)
                {
                    tail += segments[i] + '.';
                }
                tail = tail.Substring(0, tail.Length - 1);
                mid = mid.Substring(0, mid.Length - 1);
                if (mid.Length > deltaLength)
                {
                    mid = mid.Substring(0, mid.Length - deltaLength);
                }
                else
                {
                    tail = tail.Substring(0, tail.Length - deltaLength);
                }
                mid += '.';
                mid = mid.Replace("..", ".");
                newUID = root + mid + tail;
            }
            return newUID;
        }
    }
}
