using FellowOakDicom;
using System;
using System.Windows;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace MAAS_TXIHelper.Core
{
    public static class CPFlipper
    {
        private static object obj = new object();

        public static int PlanFlip()
        {
            string dllDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string logfilePath = Path.Combine(dllDirectory, "TXIlog.log");
            // This method checks if it is a TrueBeam or Halcyon/Ethos plan.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".dcm";
            openFileDialog.Filter = "DICOM Files (*.dcm)|*.dcm";
            bool? result = openFileDialog.ShowDialog() ?? false;
            string filename = "";
            if(result == true)
            {
                filename = openFileDialog.FileName;
            }
            if (File.Exists(filename) == false)
            {
                MessageBox.Show("Input file does not exist.");
                return -1;
            }
            bool isTrueBeam = false;
            bool isHalcyon = false;
            DicomDataset dataset = DicomFile.Open(filename).Dataset;
            // Loop through beams
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
                return PlanFlipVMAT(filename);
            }
            else if (isHalcyon)
            {
                return PlanFlipHalcyon(filename);
            }
            else
            {
                return -1;
            }
        }
        public static int PlanFlipVMAT(string filename)
        {
            if(File.Exists(filename) == false)
            {
                MessageBox.Show("Input file does not exist.");
                return -1;
            }
            DicomFile file = DicomFile.Open(filename);
            DicomDataset dataset = file.Dataset;

            string dllDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string logfilePath = Path.Combine(dllDirectory, "TXIlog.log");
            string outputDir = Path.Combine(Path.GetDirectoryName(filename), "CP_FLIP_OUTPUT");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
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
            string[] segments = oldUID.Split('.');
            string timeTag = DateTime.Now.ToString("yyyyMMddHHmmssff");
            segments[segments.Length - 1] = timeTag;
            string newUID = "";
            foreach (string segment in segments)
            {
                newUID += segment + '.';
            }
            newUID = newUID.Substring(0, newUID.Length - 1);
            dataset.AddOrUpdate(DicomTag.SOPInstanceUID, newUID);
            // update instance creation date and time
            dataset.AddOrUpdate(DicomTag.InstanceCreationDate, DateTime.Now.ToString("yyyyMMdd"));
            dataset.AddOrUpdate(DicomTag.InstanceCreationTime, DateTime.Now.ToString("HHmmss"));
            // assign a new plan ID
            string oldPlanLabel = dataset.GetString(DicomTag.RTPlanLabel);
            dataset.AddOrUpdate(DicomTag.RTPlanLabel, oldPlanLabel + '1');
            string newFilename = Path.Combine(outputDir, Path.GetFileName(filename));
            file.Save(newFilename);
            return 0;
        }

        public static int PlanFlipHalcyon(string filename)
        {
            if (File.Exists(filename) == false)
            {
                MessageBox.Show("Input file does not exist.");
                return -1;
            }
            DicomFile file = DicomFile.Open(filename);
            DicomDataset dataset = file.Dataset;

            string dllDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string logfilePath = Path.Combine(dllDirectory, "TXIlog.log");
            string outputDir = Path.Combine(Path.GetDirectoryName(filename), "CP_FLIP_OUTPUT");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
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
                                int leafPairNum = (int) leafPositions.Length / 2;
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
            string[] segments = oldUID.Split('.');
            string timeTag = DateTime.Now.ToString("yyyyMMddHHmmssff");
            segments[segments.Length - 1] = timeTag;
            string newUID = "";
            foreach (string segment in segments)
            {
                newUID += segment + '.';
            }
            newUID = newUID.Substring(0, newUID.Length - 1);
            dataset.AddOrUpdate(DicomTag.SOPInstanceUID, newUID);
            // update instance creation date and time
            dataset.AddOrUpdate(DicomTag.InstanceCreationDate, DateTime.Now.ToString("yyyyMMdd"));
            dataset.AddOrUpdate(DicomTag.InstanceCreationTime, DateTime.Now.ToString("HHmmss"));
            // assign a new plan ID
            string oldPlanLabel = dataset.GetString(DicomTag.RTPlanLabel);
            dataset.AddOrUpdate(DicomTag.RTPlanLabel, oldPlanLabel + '1');
            string newFilename = Path.Combine(outputDir, Path.GetFileName(filename));
            file.Save(newFilename);
            return 0;
        }
    }
}
