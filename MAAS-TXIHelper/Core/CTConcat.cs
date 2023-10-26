using MAAS_TXIHelper.Models;
using SimpleProgressWindow;
using System;
using System.IO;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using I = itk.simple;
using V = VMS.TPS.Common.Model.API;

namespace MAAS_TXIHelper.Core
{
    public class CTConcat : SimpleMTbase
    {
        private readonly Patient _patient;
        private readonly ImageModel _imagePrimary;
        private readonly ImageModel _imageSecondary;
        private readonly Registration _registration;
        private readonly string _saveDir;
        private readonly double _spacingMM;

        public CTConcat(V.Patient patient, ImageModel imagePrimary, ImageModel imageSecondary, V.Registration registration, string saveDir, double spacingMM)
        {
            _patient = patient;
            _imagePrimary = imagePrimary;
            _imageSecondary = imageSecondary;
            _registration = registration;
            _saveDir = saveDir;
            _spacingMM = spacingMM;
        }

        public override bool Run()
        {
            ProvideUIUpdate($"Starting CT Concatenation.");
            double workloadPercent1 = ((double)_imagePrimary.ZSize) / (_imagePrimary.ZSize + _imageSecondary.ZSize) * 50;
            ProvideUIUpdate($"Converting the primary image.");
            var itkImagePrimary = ConvertToItkImage(_imagePrimary.VImage, 0, workloadPercent1);
            ProvideUIUpdate($"Processing of the primary image is complete.");

            double workloadPercent2 = workloadPercent1 + ((double)_imageSecondary.ZSize) / (_imagePrimary.ZSize + _imageSecondary.ZSize) * 50;
            ProvideUIUpdate($"Converting the secondary image.");
            var itkImageSecondary = ConvertToItkImage(_imageSecondary.VImage, workloadPercent1, workloadPercent2);
            ProvideUIUpdate($"Processing of the secondary image is complete.");

            ProvideUIUpdate("Transforming images.");
            I.Image itkImageSecondaryTransformed = TransformImage(itkImageSecondary);
            ProvideUIUpdate("Image transformation is completes.");

            double workloadPercent3 = workloadPercent2 + 25;
            ProvideUIUpdate("Concatenating images.");
            I.Image itkImageMerged = MergeImages(itkImagePrimary, itkImageSecondaryTransformed, workloadPercent2, workloadPercent3);
            ProvideUIUpdate("Images are concatenated.");

            ProvideUIUpdate("Creating concatenated image files in the specified file location.");
            SaveImagesDICOM(itkImageMerged, _imagePrimary.VImage, _patient, workloadPercent3, 100);
            ProvideUIUpdate(100, "Complete");
            return true;
        }

        private I.Image ConvertToItkImage(V.Image vImage, double workCompleted, double workToDo)
        {
            I.PixelIDValueEnum pixelType = I.PixelIDValueEnum.sitkFloat32;
            I.VectorUInt32 image3DSize = new I.VectorUInt32(new uint[] { (uint)vImage.XSize, (uint)vImage.YSize, (uint)vImage.ZSize });
            I.Image itkImage = new I.Image(image3DSize, pixelType);
            I.VectorDouble spacing3D = new I.VectorDouble(new double[] { vImage.XRes, vImage.YRes, vImage.ZRes });
            itkImage.SetSpacing(spacing3D);
            I.VectorDouble origin = new I.VectorDouble(new double[] { vImage.Origin.x, vImage.Origin.y, vImage.Origin.z });
            itkImage.SetOrigin(origin);

            int[,] voxelPlane = new int[vImage.XSize, vImage.YSize];
            ProvideUIUpdate((int)workCompleted, $"Total {vImage.ZSize} image slices to process.");
            I.VectorUInt32 index = new I.VectorUInt32(new uint[] { 0, 0, 0 });
            for (int z = 0; z < vImage.ZSize; z++)
            {
                //                var point = itkImage.TransformIndexToPhysicalPoint(new VectorInt64(new Int64[] { 0, 0, z }));
                double progDec = (double)z / vImage.ZSize;
                int progInt = (int)(workCompleted + progDec * (workToDo - workCompleted));
                var msg = $"Processing image slice #{z + 1}";
                if ((z + 1) % 10 == 0)
                {
                    // Slice is a multiple of 10, show message update
                    ProvideUIUpdate(progInt, msg);
                }
                else
                {
                    ProvideUIUpdate(progInt);
                }
                UpdateUILabel(msg);

                vImage.GetVoxels(z, voxelPlane);
                for (int x = 0; x < vImage.XSize; x++)
                {
                    for (int y = 0; y < vImage.YSize; y++)
                    {
                        index[0] = (uint)x;
                        index[1] = (uint)y;
                        index[2] = (uint)z;
                        itkImage.SetPixelAsFloat(index, (float)(vImage.VoxelToDisplayValue(voxelPlane[x, y])));
                    }
                }
            }
            return itkImage;
        }

        private I.Image TransformImage(I.Image itkImageSecondary)
        {
            // Read image registration data. It is a 4 x 4 matrix.
            double[,] rMatrix = _registration.TransformationMatrix;
            Console.WriteLine($"Registration matrix (rank: {rMatrix.Rank} length: {rMatrix.Length})");
            for (int i = 0; i < rMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < rMatrix.GetLength(1); j++)
                {
                    Console.Write($"{rMatrix[i, j]}\t");
                }
                Console.WriteLine();
            }
            // first rotate the image
            // According to SimpleITK documentation, Euler3DTransform is a rigid 3D transform with rotation in radians around the fixed center with translation.
            I.Euler3DTransform eulerTransform = new I.Euler3DTransform();
            // First set the center of rotations.
            I.VectorDouble imageCenter = new I.VectorDouble(new double[] { 0, 0, 0 });
            eulerTransform.SetCenter(imageCenter);  // This is the center for rotation transformations.
            // Then define the rotation angles
            // calcualate the rotation angles from the rotation matrix
            // 8/18/2023: reserse the sign for y_theta. I think that it is due to the orientation of the y-axis in the SimpleItk coordinate system.
            // In it, y-axis points upwards in an imaging plane, while in Eclipse, y-axis points downwards.
            double x_theta, y_theta, z_theta;
            y_theta = -Math.Asin(-rMatrix[2, 0]);
            x_theta = -Math.Asin(rMatrix[2, 1] / Math.Cos(y_theta));
            z_theta = -Math.Asin(rMatrix[1, 0] / Math.Cos(y_theta));
            eulerTransform.SetRotation(x_theta, y_theta, z_theta);
            var rotated = I.SimpleITK.Resample(itkImageSecondary, eulerTransform, I.InterpolatorEnum.sitkLinear, -1000);

            I.VectorDouble originOld = itkImageSecondary.GetOrigin();
            originOld[0] = originOld[0] + rMatrix[0, 3];
            originOld[1] = originOld[1] + rMatrix[1, 3];
            originOld[2] = originOld[2] + rMatrix[2, 3];
            rotated.SetOrigin(originOld);
            return rotated;
        }

        private I.Image MergeImages(I.Image itkImagePrimary, I.Image itkImageSecondaryTransformed, double workCompleted, double workToDo)
        {
            // first define the merged image
            int newSlices = (int)((itkImagePrimary.GetOrigin()[2] + itkImagePrimary.GetSize()[2] * itkImagePrimary.GetSpacing()[2]
                - itkImageSecondaryTransformed.GetOrigin()[2]) / _spacingMM);
            I.PixelIDValueEnum pixelType = I.PixelIDValueEnum.sitkFloat32;
            I.VectorUInt32 image3DSize = new I.VectorUInt32(new uint[] { (uint)itkImagePrimary.GetSize()[0], (uint)itkImagePrimary.GetSize()[1], (uint)newSlices });
            I.Image itkImageMerged = new I.Image(image3DSize, pixelType);
            itkImageMerged.SetSpacing(new I.VectorDouble(new double[] { itkImagePrimary.GetSpacing()[0], itkImagePrimary.GetSpacing()[1], _spacingMM }));
            double newOriginZ = itkImagePrimary.GetOrigin()[2] + itkImagePrimary.GetSize()[2] * itkImagePrimary.GetSpacing()[2] - newSlices * _spacingMM;
            itkImageMerged.SetOrigin(new I.VectorDouble(new double[] { itkImagePrimary.GetOrigin()[0], itkImagePrimary.GetOrigin()[1], newOriginZ }));
            // here we first construct a SimpleITK 3D image from the image data
            // 1. Based on ESAPI manual, the DICOM origin is the DICOM coordinate for the point at the upper left corner of the first imaging plane.
            //    Note that this DICOM origin does not have [0, 0, 0] as coordinates.
            //    The [0, 0, 0] point in the DICOM coordinate is a different point that was set during imaging scan, usually denoted by the BBs.
            //    In Eclipse, the displayed coordinates are relative to the user origin.
            // 2. The user origin (image.UserOrigin) is the user origin offset from DICOM origin. You can find the coordinates in Eclipse by looking at the property
            //    of the User Origin in the External Beam Planning workspace.
            ProvideUIUpdate((int)workCompleted, $"Total {itkImageMerged.GetSize()[2]} image slices to process.");
            I.VectorInt64 pixelIndex64 = new I.VectorInt64(new long[] { 0, 0, 0 });
            I.VectorUInt32 pixelIndex32 = new I.VectorUInt32(new uint[] { 0, 0, 0 });
            I.VectorInt64 indexOriginal64;
            I.VectorUInt32 indexOriginal32 = new I.VectorUInt32(new uint[] { 0, 0, 0 });

            for (int z = 0; z < itkImageMerged.GetSize()[2]; z++)
            {
                double progDec = (double)z / itkImageMerged.GetSize()[2];
                int progInt = (int)(workCompleted + progDec * (workToDo - workCompleted));
                var msg = $"Creating concatenated image slice #{z + 1}";
                if ((z + 1) % 10 == 0)
                {
                    // Slice is a multiple of 10, show message update
                    ProvideUIUpdate(progInt, msg);
                }
                else
                {
                    ProvideUIUpdate(progInt);
                }
                UpdateUILabel(msg);
                Console.Write($"\rCreating merged image for slice #{z + 1}");
                for (int x = 0; x < itkImageMerged.GetSize()[0]; x++)
                {
                    for (int y = 0; y < itkImageMerged.GetSize()[1]; y++)
                    {
                        pixelIndex64[0] = x;
                        pixelIndex64[1] = y;
                        pixelIndex64[2] = z;
                        pixelIndex32[0] = (uint)x;
                        pixelIndex32[1] = (uint)y;
                        pixelIndex32[2] = (uint)z;
                        using (var physicalCoordinate = itkImageMerged.TransformIndexToPhysicalPoint(pixelIndex64))
                        {
                            if (physicalCoordinate[2] >= itkImagePrimary.GetOrigin()[2])
                            {
                                indexOriginal64 = itkImagePrimary.TransformPhysicalPointToIndex(physicalCoordinate);
                                indexOriginal32[0] = (uint)indexOriginal64[0];
                                indexOriginal32[1] = (uint)indexOriginal64[1];
                                indexOriginal32[2] = (uint)indexOriginal64[2];
                                var pixelValue = itkImagePrimary.GetPixelAsFloat(indexOriginal32);
                                itkImageMerged.SetPixelAsFloat(pixelIndex32, pixelValue);
                            }
                            else
                            {
                                using (var index = itkImageSecondaryTransformed.TransformPhysicalPointToIndex(physicalCoordinate))
                                {
                                    if (PixelIndexOutofBound(index, itkImageSecondaryTransformed))
                                    {
                                        itkImageMerged.SetPixelAsFloat(pixelIndex32, -1000);
                                    }
                                    else
                                    {
                                        indexOriginal32[0] = (uint)index[0];
                                        indexOriginal32[1] = (uint)index[1];
                                        indexOriginal32[2] = (uint)index[2];
                                        var pixelValue = itkImageSecondaryTransformed.GetPixelAsFloat(indexOriginal32);
                                        itkImageMerged.SetPixelAsFloat(pixelIndex32, pixelValue);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine($"\nMerged image was created with {itkImageMerged.GetSize()[2]} slices.");
            return itkImageMerged;
        }
        private void SaveImagesDICOM(I.Image itkImageMerged, V.Image imagePrimary,
            Patient patient, double workCompleted, double workToDo)
        {
            I.PixelIDValueEnum pixelTypeDCM = I.PixelIDValueEnum.sitkInt16;
            I.VectorUInt32 imageSize = new I.VectorUInt32(new uint[] { itkImageMerged.GetSize()[0], itkImageMerged.GetSize()[1] });
            I.Image itkImageDCM = new I.Image(imageSize, pixelTypeDCM);
            I.VectorDouble spacingDCM = new I.VectorDouble(new double[] { itkImageMerged.GetSpacing()[0], itkImageMerged.GetSpacing()[1] });
            itkImageDCM.SetSpacing(spacingDCM);
            I.ImageFileWriter writer = new I.ImageFileWriter();
            writer.KeepOriginalImageUIDOn();
            // DICOM metadata that are common to each slice
            itkImageDCM.SetMetaData("0010|0010", $"{patient.LastName}^{patient.FirstName}^{patient.MiddleName}");
            itkImageDCM.SetMetaData("0010|0020", patient.Id);
            itkImageDCM.SetMetaData("0008|0008", "ORIGINAL\\PRIMARY\\AXIAL");
            itkImageDCM.SetMetaData("0008|0070", imagePrimary.Series.ImagingDeviceManufacturer);
            itkImageDCM.SetMetaData("0008|0020", DateTime.Now.ToString("yyyyMMdd"));  // study date
            itkImageDCM.SetMetaData("0008|0030", DateTime.Now.ToString("HHmmss.ffffff")); // study time
            itkImageDCM.SetMetaData("0018|0050", imagePrimary.ZRes.ToString()); // slice thickness
                                                                                // itkImageDCM.SetMetaData("0020|0012", ?); // acquisition number
            string newStudyUID = MakeNewUID(imagePrimary.Series.Study.UID);
            itkImageDCM.SetMetaData("0020|000D", newStudyUID);   // study UID.
            string newSeriesUID = MakeNewUID(imagePrimary.Series.UID);
            itkImageDCM.SetMetaData("0020|000E", newSeriesUID);  // series UID.
            itkImageDCM.SetMetaData("0020|0052", imagePrimary.Series.FOR);  // use the same frame of reference UID as the original image series.
            itkImageDCM.SetMetaData("0020|1040", "BB"); // position reference indicator
            itkImageDCM.SetMetaData("0020|0012", "1"); // acquisition number
            ProvideUIUpdate($"Total {itkImageMerged.GetSize()[2]} image slices to process.");
            I.VectorUInt32 index = new I.VectorUInt32(new uint[] { 0, 0, 0 });
            I.VectorUInt32 indexPlane = new I.VectorUInt32(new uint[] { 0, 0 });
            for (int z = 0; z < itkImageMerged.GetSize()[2]; z++)
            {
                double progDec = (double)z / itkImageMerged.GetSize()[2];
                int progInt = (int)(workCompleted + progDec * (workToDo - workCompleted));
                var msg = $"Saving image slice #{z + 1}";
                if ((z + 1) % 10 == 0)
                {
                    // Slice is a multiple of 10, show message update
                    ProvideUIUpdate(progInt, msg);
                }
                else
                {
                    ProvideUIUpdate(progInt);
                }
                UpdateUILabel(msg);

                for (int x = 0; x < itkImageMerged.GetSize()[0]; x++)
                {
                    for (int y = 0; y < itkImageMerged.GetSize()[1]; y++)
                    {
                        indexPlane[0] = (uint)x;
                        indexPlane[1] = (uint)y;
                        index[0] = (uint)x;
                        index[1] = (uint)y;
                        index[2] = (uint)z;
                        itkImageDCM.SetPixelAsInt16(indexPlane, (Int16)itkImageMerged.GetPixelAsFloat(index));
                    }
                }
                if (imagePrimary.Series.Modality == SeriesModality.CT)
                {
                    itkImageDCM.SetMetaData("0008|0060", "CT");
                    itkImageDCM.SetMetaData("0018|0060", "120");
                }
                string positionPatient, sliceLocation;
                if (itkImageMerged.GetOrigin()[0] % 1 == 0)
                {
                    positionPatient = $"{(int)itkImageMerged.GetOrigin()[0]}";
                }
                else
                {
                    positionPatient = string.Format("{0:0.00}", itkImageMerged.GetOrigin()[0]);
                }
                if (itkImageMerged.GetOrigin()[1] % 1 == 0)
                {
                    positionPatient += $"\\{(int)itkImageMerged.GetOrigin()[0]}";
                }
                else
                {
                    positionPatient += "\\" + string.Format("{0:0.00}", itkImageMerged.GetOrigin()[0]);
                }
                double zPosition = itkImageMerged.GetOrigin()[2] + z * itkImageMerged.GetSpacing()[2];
                if (zPosition % 1 == 0)
                {
                    positionPatient += $"\\{(int)zPosition}";
                    sliceLocation = $"{(int)zPosition}";
                }
                else
                {
                    positionPatient += "\\" + string.Format("{0:0.00}", zPosition);
                    sliceLocation = string.Format("{0:0.00}", zPosition);
                }
                itkImageDCM.SetMetaData("0020|0032", positionPatient);  // image position patient
                itkImageDCM.SetMetaData("0020|1041", sliceLocation);  // slice location
                Console.Write($"\rSaving DICOM file for slice index: {z}   ");

                writer.SetFileName(Path.Combine(_saveDir, $"{_patient.Id}_merged_{z}.DCM"));
                writer.Execute(itkImageDCM);
            }
            Console.WriteLine($"All DICOM files were saved.");
        }
        private bool PixelIndexOutofBound(I.VectorInt64 indexPrimary, itk.simple.Image itkImageSecondaryTransformed)
        {
            int x = (int)indexPrimary[0];
            int y = (int)indexPrimary[1];
            int z = (int)indexPrimary[2];
            if (x < 0 || x >= itkImageSecondaryTransformed.GetSize()[0])
                return true;
            if (y < 0 || y >= itkImageSecondaryTransformed.GetSize()[1])
                return true;
            if (z < 0 || z >= itkImageSecondaryTransformed.GetSize()[2])
                return true;
            return false;
        }
        private string MakeNewUID(string uid)
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
                int deltaLength = 64 - newUID.Length;
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
                mid += '.';
                mid = mid.Replace("..", ".");
                newUID = root + mid + tail;
            }
            return newUID;
        }

    }
}
