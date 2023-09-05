using FellowOakDicom;
using System;
using System.IO;

namespace MAAS_TXIHelper.Core
{
    // Could write this file myself from eclipse input

    public static class CPFlipper
    {
        public static void PlanFlipVMAT(string filename)
        {
            DicomFile file = DicomFile.Open(filename);
            DicomDataset dataset = file.Dataset;

            string outputDir = Path.Combine(Path.GetDirectoryName(filename), "CP_FLIP_OUTPUT");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Loop through beams
            foreach (DicomDataset beam in dataset.GetSequence(DicomTag.BeamSequence))
            {

                if (beam.GetString(DicomTag.BeamTaskType) == "TREATMENT")
                {
                    foreach (DicomDataset cp in beam.GetSequence(DicomTag.ControlPointSequence))
                    {
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
                                double[] leafPositions = pos.GetValues<double>(DicomTag.LeafPositionBoundaries);
                                for (int i = 0; i < leafPositions.Length / 2; i++)
                                {
                                    double temp = leafPositions[i];
                                    leafPositions[i] = -leafPositions[leafPositions.Length - i - 1];
                                    leafPositions[leafPositions.Length - i - 1] = -temp;
                                }
                                pos.AddOrUpdate(DicomTag.LeafPositionBoundaries, leafPositions);
                            }
                        }
                    }
                }
            }


            string newFilename = Path.Combine(outputDir, Path.GetFileName("FLIPPED" + filename));
            file.Save(newFilename);
        }

        public static void PlanFlipHalcyon(string filename)
        {
            DicomFile file = DicomFile.Open(filename);
            DicomDataset dataset = file.Dataset;

            string outputDir = Path.Combine(Path.GetDirectoryName(filename), "CP_FLIP_OUTPUT");
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            foreach (DicomDataset beam in dataset.GetSequence(DicomTag.BeamSequence))
            {
                if (beam.GetString(DicomTag.BeamTaskType) == "TREATMENT")
                {
                    foreach (DicomDataset cp in beam.GetSequence(DicomTag.ControlPointSequence))
                    {

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
                                double[] leafPositions = pos.GetValues<double>(DicomTag.LeafPositionBoundaries);
                                double[] temp = new double[leafPositions.Length];
                                Array.Copy(leafPositions, temp, leafPositions.Length);
                                for (int i = 0; i < leafPositions.Length / 2; i++)
                                {
                                    if (Math.Abs(temp[leafPositions.Length - 1 - i]) > 139.5 && Math.Abs(temp[i]) > 139.5 && temp[leafPositions.Length - 1 - i] == temp[i])
                                    {
                                        leafPositions[i] = temp[leafPositions.Length - 1 - i];
                                        leafPositions[leafPositions.Length / 2 + i] = temp[i];
                                    }
                                    else
                                    {
                                        leafPositions[i] = -temp[leafPositions.Length - 1 - i];
                                        leafPositions[leafPositions.Length / 2 + i] = -temp[i];
                                    }
                                }
                                pos.AddOrUpdate(DicomTag.LeafPositionBoundaries, leafPositions);
                            }
                        }
                    }
                }
            }

            string newFilename = Path.Combine(outputDir, Path.GetFileName("FLIPPED2" + filename));
            file.Save(newFilename);
        }
    }
}
