using CommunityToolkit.Mvvm.Input;
using MAAS_TXIHelper.Views;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using I = itk.simple;
using System.Windows.Forms;

namespace MAAS_TXIHelper.ViewModels
{
    internal class ConcatViewModel : INotifyPropertyChanged
    {
        private readonly EsapiWorker _worker;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private readonly object _collectionLock;
        private ObservableCollection<string> _primaryImages;
        public ObservableCollection<string> PrimaryImages
        {
            get { return _primaryImages; }
            set
            {
                _primaryImages = value;
                BindingOperations.EnableCollectionSynchronization(_primaryImages, _collectionLock);
            }
        }

        private ObservableCollection<string> _secondaryImages;
        public ObservableCollection<string> SecondaryImages
        {
            get { return _secondaryImages; }
            set
            {
                _secondaryImages = value;
                BindingOperations.EnableCollectionSynchronization(_secondaryImages, _collectionLock);
            }
        }
        private ObservableCollection<string> _registrations;
        public ObservableCollection<string> Registrations
        {
            get { return _registrations; }
            set
            {
                _registrations = value;
                BindingOperations.EnableCollectionSynchronization(_registrations, _collectionLock);
            }
        }
        private string _PrimaryImageSelected;
        public string PrimaryImageSelected
        {
            get { return _PrimaryImageSelected; }
            set
            {
                _PrimaryImageSelected = value;
                PopulateSecondaryImages();
                _worker.Run(scriptContext =>
                {
                    var seriesID = PrimaryImageSelected.Split('(')[0].Remove(PrimaryImageSelected.Split('(')[0].Length - 1);
                    var imageID = PrimaryImageSelected.Split('(')[1].Split(')')[0];
                    var primary = scriptContext.Patient.Studies.SelectMany(study => study.Images3D).ToList().Where(image =>
                    (image.Series.Id == seriesID && image.Id == imageID)).FirstOrDefault();
                    InputText = string.Format("{0:F1}", primary.ZRes);
                });
                IsConcatBtnEnabled = false;
                TextBox = "Please select a secondary image.";
            }
        }
        private string _SecondaryImageSelected;
        public string SecondaryImageSelected
        {
            get { return _SecondaryImageSelected; }
            set
            {
                _SecondaryImageSelected = value;
                PopulateRegistrations();
                IsConcatBtnEnabled = false;
                TextBox = "Please select a proper image registration between the primary and secondary images.";
            }
        }

        private string _RegistrationSelected;
        public string RegistrationSelected
        {
            get { return _RegistrationSelected; }
            set
            {
                _RegistrationSelected = value;
                IsConcatBtnEnabled = true;
                TextBox = "Please click on the Concatenate CT Images button to concatenate the primary and secondary images.";
            }
        }
        private bool _isPrimaryImageSelectionEnabled;
        public bool isPrimaryImageSelectionEnabled
        {
            get => _isPrimaryImageSelectionEnabled;
            set
            {
                if (_isPrimaryImageSelectionEnabled != value)
                {
                    _isPrimaryImageSelectionEnabled = value;
                }
                OnPropertyChanged(nameof(isPrimaryImageSelectionEnabled));
            }
        }
        private bool _isSecondaryImageSelectionEnabled;
        public bool isSecondaryImageSelectionEnabled
        {
            get => _isSecondaryImageSelectionEnabled;
            set
            {
                if (_isSecondaryImageSelectionEnabled != value)
                {
                    _isSecondaryImageSelectionEnabled = value;
                }
                OnPropertyChanged(nameof(isSecondaryImageSelectionEnabled));
            }
        }
        private bool _isRegistrationSelectionEnabled;
        public bool isRegistrationSelectionEnabled
        {
            get => _isRegistrationSelectionEnabled;
            set
            {
                if (_isRegistrationSelectionEnabled != value)
                {
                    _isRegistrationSelectionEnabled = value;
                }
                OnPropertyChanged(nameof(isRegistrationSelectionEnabled));
            }
        }
        private string _SecondaryLabelColor;
        public string SecondaryLabelColor
        {
            get { return _SecondaryLabelColor; }
            set
            {
                if (_SecondaryLabelColor != value)
                {
                    _SecondaryLabelColor = value;
                    OnPropertyChanged(nameof(SecondaryLabelColor));
                }
            }
        }

        private string _RegistrationLabelColor;
        public string RegistrationLabelColor
        {
            get { return _RegistrationLabelColor; }
            set
            {
                if (_RegistrationLabelColor != value)
                {
                    _RegistrationLabelColor = value;
                    OnPropertyChanged(nameof(RegistrationLabelColor));
                }
            }
        }
        private bool _IsSpacingTextBoxReadOnly;
        public bool IsSpacingTextBoxReadOnly
        {
            get => _IsSpacingTextBoxReadOnly;
            set
            {
                if (_IsSpacingTextBoxReadOnly != value)
                {
                    _IsSpacingTextBoxReadOnly = value;
                }
                OnPropertyChanged(nameof(IsSpacingTextBoxReadOnly));
            }
        }
        private string _InputText;
        public string InputText
        {
            get => _InputText;
            set
            {
                if (_InputText != value)
                {
                    _InputText = value;
                }
                OnPropertyChanged(nameof(InputText));
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
        private bool _IsConcatBtnEnabled;
        public bool IsConcatBtnEnabled
        {
            get { return _IsConcatBtnEnabled; }
            set
            {
                if (_IsConcatBtnEnabled != value)
                {
                    _IsConcatBtnEnabled = value;
                    OnPropertyChanged(nameof(IsConcatBtnEnabled));
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
        public ICommand ConcatCmd { get; }

        public ConcatViewModel(EsapiWorker esapiWorker)
        {
            _worker = esapiWorker;
            _collectionLock = new object();
            PrimaryImages = new ObservableCollection<string>();
            SecondaryImages = new ObservableCollection<string>();
            Registrations = new ObservableCollection<string>();
            ConcatCmd = new RelayCommand(ConcatImges);
            InputText = string.Empty;
            IsSpacingTextBoxReadOnly = false;
            ProgressBarValue = 0;
            TextBox = "Please start by selecting the primary 3D image.";
            isPrimaryImageSelectionEnabled = true;
            isSecondaryImageSelectionEnabled = true;
            isRegistrationSelectionEnabled = true;
            SecondaryLabelColor = "Gray";
            RegistrationLabelColor = "Gray";
            IsConcatBtnEnabled = false;
            _worker.Run(scriptContext =>
            {
                if (scriptContext.Patient == null)
                {
                    return;
                }
                foreach (var pi in scriptContext.Patient.Studies.SelectMany(study => study.Images3D).ToList())
                {
                    PrimaryImages.Add($"{pi.Series.Id} ({pi.Id})");
                }
            });
        }
        private void PopulateSecondaryImages()
        {
            SecondaryImages.Clear();
            SecondaryImageSelected = null;
            Registrations.Clear();
            RegistrationSelected = null;
            RegistrationLabelColor = "Gray";

            if (PrimaryImageSelected != null)
            {
                _worker.Run(scriptContext =>
                {
                    var seriesID = PrimaryImageSelected.Split('(')[0].Remove(PrimaryImageSelected.Split('(')[0].Length - 1);
                    var imageID = PrimaryImageSelected.Split('(')[1].Split(')')[0];
                    var primary = scriptContext.Patient.Studies.SelectMany(study => study.Images3D).ToList().Where(image =>
                    (image.Series.Id == seriesID && image.Id == imageID)).FirstOrDefault();
                    foreach (Registration registration in scriptContext.Patient.Registrations)
                    {
                        if (registration.RegisteredFOR == primary.FOR)
                        {
                            foreach (var candidate in scriptContext.Patient.Studies.SelectMany(study => study.Images3D).ToList())
                            {
                                if (candidate.FOR == registration.SourceFOR &&
                                !SecondaryImages.Contains($"{candidate.Series.Id} ({candidate.Id})"))
                                {
                                    SecondaryImages.Add($"{candidate.Series.Id} ({candidate.Id})");
                                }
                            }
                        }
                    }
                    if (SecondaryImages.Count > 0)
                    {
                        SecondaryLabelColor = "Black";
                    }
                    else
                    {
                        SecondaryLabelColor = "Gray";
                    }
                });
            }
        }

        private void PopulateRegistrations()
        {
            Registrations.Clear();
            RegistrationSelected = null;
            RegistrationLabelColor = "Gray";
            if (PrimaryImageSelected != null && SecondaryImageSelected != null)
            {
                _worker.Run(scriptContext =>
                {
                    var seriesID = PrimaryImageSelected.Split('(')[0].Remove(PrimaryImageSelected.Split('(')[0].Length - 1);
                    var imageID = PrimaryImageSelected.Split('(')[1].Split(')')[0];
                    var primary = scriptContext.Patient.Studies.SelectMany(study => study.Images3D).ToList().Where(image =>
                    (image.Series.Id == seriesID && image.Id == imageID)).FirstOrDefault();
                    seriesID = SecondaryImageSelected.Split('(')[0].Remove(SecondaryImageSelected.Split('(')[0].Length - 1);
                    imageID = SecondaryImageSelected.Split('(')[1].Split(')')[0];
                    var secondary = scriptContext.Patient.Studies.SelectMany(study => study.Images3D).ToList().Where(image =>
                    (image.Series.Id == seriesID && image.Id == imageID)).FirstOrDefault();
                    foreach (Registration registration in scriptContext.Patient.Registrations)
                    {
                        if (registration.RegisteredFOR == primary.FOR && registration.SourceFOR == secondary.FOR)
                        {
                            Registrations.Add($"{registration.Id}");
                        }
                    }
                    if (Registrations.Count > 0)
                    {
                        RegistrationLabelColor = "Black";
                    }
                    else
                    {
                        RegistrationLabelColor = "Gray";
                    }
                });
            }
            IsConcatBtnEnabled = false;
        }

        private void ConcatImges()
        {
            // here open a folder dialogue ask the user to select an output folder.
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select the directory where new files will be saved.";
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                double inputSpacing = 0;
                try
                {
                    inputSpacing = double.Parse(InputText);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                    return;
                }
                if (inputSpacing > 20 || inputSpacing < 1)
                {
                    System.Windows.MessageBox.Show("Input out of range of 1 to 20 mm. Please correct the input.");
                    return;
                }
                var folderPath = dialog.SelectedPath;
                _worker.Run(scriptContext =>
                {
                    isPrimaryImageSelectionEnabled = false;
                    isSecondaryImageSelectionEnabled = false;
                    isRegistrationSelectionEnabled = false;
                    IsSpacingTextBoxReadOnly = true;
                    IsConcatBtnEnabled = false;
                    var seriesID = PrimaryImageSelected.Split('(')[0].Remove(PrimaryImageSelected.Split('(')[0].Length - 1);
                    var imageID = PrimaryImageSelected.Split('(')[1].Split(')')[0];
                    var primary = scriptContext.Patient.Studies.SelectMany(study => study.Images3D).ToList().Where(image =>
                    (image.Series.Id == seriesID && image.Id == imageID)).FirstOrDefault();
                    seriesID = SecondaryImageSelected.Split('(')[0].Remove(SecondaryImageSelected.Split('(')[0].Length - 1);
                    imageID = SecondaryImageSelected.Split('(')[1].Split(')')[0];
                    var secondary = scriptContext.Patient.Studies.SelectMany(study => study.Images3D).ToList().Where(image =>
                        (image.Series.Id == seriesID && image.Id == imageID)).FirstOrDefault();
                    var registration = scriptContext.Patient.Registrations.Where(reg => reg.Id == RegistrationSelected).FirstOrDefault();

                    var PrimaryImageSlices = primary.ZSize;
                    var SecondaryImageSlices = secondary.ZSize;

                    TextBox = "Reading the primary image.\n";
                    // first convert the primary image
                    I.PixelIDValueEnum pixelType = I.PixelIDValueEnum.sitkFloat32;
                    I.VectorUInt32 image3DSize = new I.VectorUInt32(new uint[] { (uint)primary.XSize, (uint)primary.YSize, (uint)primary.ZSize });
                    I.Image itkImagePrimary = new I.Image(image3DSize, pixelType);
                    I.VectorDouble spacing3D = new I.VectorDouble(new double[] { primary.XRes, primary.YRes, primary.ZRes });
                    itkImagePrimary.SetSpacing(spacing3D);
                    I.VectorDouble origin = new I.VectorDouble(new double[] { primary.Origin.x, primary.Origin.y, primary.Origin.z });
                    itkImagePrimary.SetOrigin(origin);

                    int[,] voxelPlane = new int[primary.XSize, primary.YSize];
                    I.VectorUInt32 index = new I.VectorUInt32(new uint[] { 0, 0, 0 });
                    for (int z = 0; z < primary.ZSize; z++)
                    {
                        //                var point = itkImage.TransformIndexToPhysicalPoint(new VectorInt64(new Int64[] { 0, 0, z }));
                        ProgressBarValue = (int)((z + 1.0f) / PrimaryImageSlices * 25.0f);
                        var msg = $"Processing image slice #{z + 1}\n";
                        if ((z + 1) % 10 == 0)
                        {
                            // Slice is a multiple of 10, show message update
                            TextBox += msg;
                        }
                        primary.GetVoxels(z, voxelPlane);
                        for (int x = 0; x < primary.XSize; x++)
                        {
                            for (int y = 0; y < primary.YSize; y++)
                            {
                                index[0] = (uint)x;
                                index[1] = (uint)y;
                                index[2] = (uint)z;
                                itkImagePrimary.SetPixelAsFloat(index, (float)(primary.VoxelToDisplayValue(voxelPlane[x, y])));
                            }
                        }
                    }

                    // then convert the secondary image
                    TextBox += "Reading the secondary image.\n";
                    image3DSize = new I.VectorUInt32(new uint[] { (uint)secondary.XSize, (uint)secondary.YSize, (uint)secondary.ZSize });
                    I.Image itkImageSecondary = new I.Image(image3DSize, pixelType);
                    spacing3D = new I.VectorDouble(new double[] { secondary.XRes, secondary.YRes, secondary.ZRes });
                    itkImageSecondary.SetSpacing(spacing3D);
                    origin = new I.VectorDouble(new double[] { secondary.Origin.x, secondary.Origin.y, secondary.Origin.z });
                    itkImageSecondary.SetOrigin(origin);

                    voxelPlane = new int[secondary.XSize, secondary.YSize];
                    // ProvideUIUpdate((int)workCompleted, $"Total {vImage.ZSize} image slices to process.");
                    index = new I.VectorUInt32(new uint[] { 0, 0, 0 });
                    for (int z = 0; z < secondary.ZSize; z++)
                    {
                        //                var point = itkImage.TransformIndexToPhysicalPoint(new VectorInt64(new Int64[] { 0, 0, z }));
                        ProgressBarValue = 25 + (int)((z + 1.0f) / SecondaryImageSlices * 25.0f);
                        var msg = $"Processing image slice #{z + 1}";
                        if ((z + 1) % 10 == 0)
                        {
                            // Slice is a multiple of 10, show message update
                            TextBox += msg + '\n';
                        }
                        secondary.GetVoxels(z, voxelPlane);
                        for (int x = 0; x < secondary.XSize; x++)
                        {
                            for (int y = 0; y < secondary.YSize; y++)
                            {
                                index[0] = (uint)x;
                                index[1] = (uint)y;
                                index[2] = (uint)z;
                                itkImageSecondary.SetPixelAsFloat(index, (float)(secondary.VoxelToDisplayValue(voxelPlane[x, y])));
                            }
                        }
                    }

                    // transform the secondary image
                    // Read image registration data. It is a 4 x 4 matrix.
                    double[,] rMatrix = registration.TransformationMatrix;
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
                    var itkImageSecondaryTransformed = I.SimpleITK.Resample(itkImageSecondary, eulerTransform, I.InterpolatorEnum.sitkLinear, -1000);

                    I.VectorDouble originOld = itkImageSecondary.GetOrigin();
                    originOld[0] = originOld[0] + rMatrix[0, 3];
                    originOld[1] = originOld[1] + rMatrix[1, 3];
                    originOld[2] = originOld[2] + rMatrix[2, 3];
                    itkImageSecondaryTransformed.SetOrigin(originOld);

                    // merge the images
                    double _spacingMM = inputSpacing;
                    int newSlices = (int)((itkImagePrimary.GetOrigin()[2] + itkImagePrimary.GetSize()[2] * itkImagePrimary.GetSpacing()[2]
                        - itkImageSecondaryTransformed.GetOrigin()[2]) / _spacingMM);
                    pixelType = I.PixelIDValueEnum.sitkFloat32;
                    image3DSize = new I.VectorUInt32(new uint[] { (uint)itkImagePrimary.GetSize()[0], (uint)itkImagePrimary.GetSize()[1], (uint)newSlices });
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
                    // ProvideUIUpdate((int)workCompleted, $"Total {itkImageMerged.GetSize()[2]} image slices to process.");
                    I.VectorInt64 pixelIndex64 = new I.VectorInt64(new long[] { 0, 0, 0 });
                    I.VectorUInt32 pixelIndex32 = new I.VectorUInt32(new uint[] { 0, 0, 0 });
                    I.VectorInt64 indexOriginal64;
                    I.VectorUInt32 indexOriginal32 = new I.VectorUInt32(new uint[] { 0, 0, 0 });

                    for (int z = 0; z < itkImageMerged.GetSize()[2]; z++)
                    {
                        ProgressBarValue = 50 + (int)((z + 1.0f) / itkImageMerged.GetSize()[2] * 25.0f);
                        if ((z + 1) % 10 == 0)
                        {
                            var msg = $"Creating concatenated image slice #{z + 1}";
                            TextBox += msg + '\n';
                        }
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
                                        using (var indexTransformed = itkImageSecondaryTransformed.TransformPhysicalPointToIndex(physicalCoordinate))
                                        {
                                            if (PixelIndexOutofBound(indexTransformed, itkImageSecondaryTransformed))
                                            {
                                                itkImageMerged.SetPixelAsFloat(pixelIndex32, -1000);
                                            }
                                            else
                                            {
                                                indexOriginal32[0] = (uint)indexTransformed[0];
                                                indexOriginal32[1] = (uint)indexTransformed[1];
                                                indexOriginal32[2] = (uint)indexTransformed[2];
                                                var pixelValue = itkImageSecondaryTransformed.GetPixelAsFloat(indexOriginal32);
                                                itkImageMerged.SetPixelAsFloat(pixelIndex32, pixelValue);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    TextBox += "Saving DICOM files.\n";
                    // save DICOM files
                    I.PixelIDValueEnum pixelTypeDCM = I.PixelIDValueEnum.sitkInt16;
                    I.VectorUInt32 imageSize = new I.VectorUInt32(new uint[] { itkImageMerged.GetSize()[0], itkImageMerged.GetSize()[1] });
                    I.Image itkImageDCM = new I.Image(imageSize, pixelTypeDCM);
                    I.VectorDouble spacingDCM = new I.VectorDouble(new double[] { itkImageMerged.GetSpacing()[0], itkImageMerged.GetSpacing()[1] });
                    itkImageDCM.SetSpacing(spacingDCM);
                    I.ImageFileWriter writer = new I.ImageFileWriter();
                    writer.KeepOriginalImageUIDOn();
                    // DICOM metadata that are common to each slice
                    var patient = scriptContext.Patient;
                    itkImageDCM.SetMetaData("0010|0010", $"{patient.LastName}^{patient.FirstName}^{patient.MiddleName}");
                    itkImageDCM.SetMetaData("0010|0020", patient.Id);
                    itkImageDCM.SetMetaData("0008|0008", "ORIGINAL\\PRIMARY\\AXIAL");
                    itkImageDCM.SetMetaData("0008|0070", primary.Series.ImagingDeviceManufacturer);
                    itkImageDCM.SetMetaData("0008|0020", DateTime.Now.ToString("yyyyMMdd"));  // study date
                    itkImageDCM.SetMetaData("0008|0030", DateTime.Now.ToString("HHmmss.ffffff")); // study time
                    string seriesDescription = "merged on " + DateTime.Now.ToString("MMddyyyy");
                    itkImageDCM.SetMetaData("0008|103E", seriesDescription);  // series description
                    itkImageDCM.SetMetaData("0008|1090", primary.Series.ImagingDeviceModel);
                    itkImageDCM.SetMetaData("0018|0050", primary.ZRes.ToString()); // slice thickness
                                                                                   // itkImageDCM.SetMetaData("0020|0012", ?); // acquisition number
                    itkImageDCM.SetMetaData("0018|5100", "HFS");
                    string newStudyUID = MakeNewUID(primary.Series.Study.UID);
                    itkImageDCM.SetMetaData("0020|000D", newStudyUID);   // study UID.
                    string newSeriesUID = MakeNewUID(primary.Series.UID);
                    itkImageDCM.SetMetaData("0020|000E", newSeriesUID);  // series UID.
                    itkImageDCM.SetMetaData("0020|0052", primary.Series.FOR);  // use the same frame of reference UID as the original image series.
                    itkImageDCM.SetMetaData("0020|1040", "BB"); // position reference indicator
                    itkImageDCM.SetMetaData("0020|0012", "1"); // acquisition number
                    itkImageDCM.SetMetaData("0028|1054", "HU"); // rescale type (Hounsfield Units or not)
                    // ProvideUIUpdate($"Total {itkImageMerged.GetSize()[2]} image slices to process.");
                    index = new I.VectorUInt32(new uint[] { 0, 0, 0 });
                    I.VectorUInt32 indexPlane = new I.VectorUInt32(new uint[] { 0, 0 });
                    for (int z = 0; z < itkImageMerged.GetSize()[2]; z++)
                    {
                        ProgressBarValue = 75 + (int)((z + 1.0f) / itkImageMerged.GetSize()[2] * 25.0f);
                        if ((z + 1) % 10 == 0)
                        {
                            var msg = $"Saving image slice #{z + 1}";
                            TextBox += msg + '\n';
                        }
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
                        if (primary.Series.Modality == SeriesModality.CT)
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

                        writer.SetFileName(Path.Combine(folderPath, $"{patient.Id}_merged_{z}.DCM"));
                        writer.Execute(itkImageDCM);
                    }
                    TextBox += $"Image concatenation is complete.\n";
                    TextBox += $"New image files were saved in this location: {folderPath}";
                    isPrimaryImageSelectionEnabled = true;
                    isSecondaryImageSelectionEnabled = true;
                    isRegistrationSelectionEnabled = true;
                    IsSpacingTextBoxReadOnly = false;
                    IsConcatBtnEnabled = true;
                });
            }
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
                segments[5] = segments[5].Substring(0, segments[5].Length - deltaLength);
                newUID = "";
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
