using System;
using System.IO;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using VMS.TPS.Common.Model.API;
using I = itk.simple;
using VMS.TPS.Common.Model.Types;
using System.Windows.Data;
using MAAS_TXIHelper.Views;
using System.Collections.Generic;
using System.Windows.Threading;

namespace MAAS_TXIHelper.ViewModels
{
    internal class OverrideViewModel : INotifyPropertyChanged
    {
        private void AnalyzeStructureVoxels()
        {
            _worker.Run(scriptContext =>
            {
                TextBox = $"Reading image data...";
                ImageSelectionEnabled = false;
                StructureSelectionEnabled = false;
                IsOverrideBtnEnabled = false;
                var seriesId = ImageSelected.Split('(')[0].Remove(ImageSelected.Split('(')[0].Length - 1);
                var imageId = ImageSelected.Split('(')[1].Split(')')[0];
                var CurrentImage3D = scriptContext.Patient.Studies.SelectMany(study => study.Images3D).ToList().Where(img => (img.Series.Id == seriesId && img.Id == imageId)).FirstOrDefault();
                var structureset = scriptContext.Patient.StructureSets.Where(s => (s.Image.Id == imageId && s.Image.Series.Id == seriesId)).FirstOrDefault();
                Structure structure = structureset.Structures.Where(s => s.Id == StructureSelected).FirstOrDefault();
                int ZSlices = CurrentImage3D.ZSize;
                System.Collections.BitArray segmentStride = new System.Collections.BitArray((int)CurrentImage3D.XSize);
                double[] imagePixels = new double[((int)CurrentImage3D.XSize)];
                List<double> numbers = new List<double>();
                for (int Z = 0; Z < ZSlices; Z++)
                {
                    int[,] voxelPlane = new int[CurrentImage3D.XSize, CurrentImage3D.YSize];
                    CurrentImage3D.GetVoxels(Z, voxelPlane);
                    for (int Y = 0; Y < CurrentImage3D.YSize; Y++)
                    {
                        var start = CurrentImage3D.Origin + CurrentImage3D.YDirection * Y * CurrentImage3D.YRes + CurrentImage3D.ZDirection * Z * CurrentImage3D.ZRes;
                        var end = start + CurrentImage3D.XDirection * CurrentImage3D.XRes * CurrentImage3D.XSize;
                        var structProfile = structure.GetSegmentProfile(start, end, segmentStride);
                        CurrentImage3D.GetImageProfile(start, end, imagePixels);
                        for (int X = 0; X < CurrentImage3D.XSize; X++)
                        {
                            if (segmentStride[X])
                            {
                                numbers.Add(imagePixels[X]);
                            }
                        }
                    }
                    int percent = (int)((float)(Z + 1) / ZSlices * 100);
                    ProgressBarValue = percent;
                }
                int count = numbers.Count;
                double avg = numbers.Average();
                double sum = numbers.Sum(d => (d - avg) * (d - avg));
                double stddev = Math.Sqrt(sum / count);
                TextBox = $"This structure includes {numbers.Count} voxels.\n";
                TextBox += $"Average CT number for this structure: {string.Format("{0:0.0} HU", avg)} with StdDev: {string.Format("{0:0.0}", stddev)}\n\n";
                TextBox += "Next, please enter the intended CT number for this structure and click the Convert button to start image conversion.";
                ImageSelectionEnabled = true;
                StructureSelectionEnabled = true;
                IsOverrideBtnEnabled = true;
            });
        }






        private readonly object _collectionLock;
        private ObservableCollection<string> _Images;
        public ObservableCollection<string> Images
        {
            get { return _Images; }
            set
            {
                _Images = value;
                BindingOperations.EnableCollectionSynchronization(_Images, _collectionLock);
            }
        }

        private string _ImageSelected;
        public string ImageSelected
        {
            get { return _ImageSelected; }
            set
            {
                if (value != null)
                {
                    _ImageSelected = value;
                    TextBox = "Please select a structure.";
                    IsOverrideBtnEnabled = false;
                    PopulateStructureList();
                }
            }
        }
        private ObservableCollection<string> _StructureList;
        public ObservableCollection<string> StructureList
        {
            get { return _StructureList; }
            set
            {
                if (value != null)
                {
                    _StructureList = value;
                    BindingOperations.EnableCollectionSynchronization(_StructureList, _collectionLock);
                }
            }
        }

        public string CTNumber { get; set; }
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
        public string _structureSelected;
        public string StructureSelected
        {
            get { return _structureSelected; }
            set
            {
                _structureSelected = value;
                if (value != null)
                {
                    DialogResult dialogResult = MessageBox.Show("Do you want to calculate imaging statistics for this structure?", "", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        AnalyzeStructureVoxels();
                    }
                    else
                    {
                        TextBox = "Please enter the intended CT number for this structure and click the Convert button to start image conversion.";
                    }

                }
            }
        }
        private bool _ImageSelectionEnabled;
        public bool ImageSelectionEnabled
        {
            get => _ImageSelectionEnabled;
            set
            {
                if (_ImageSelectionEnabled != value)
                {
                    _ImageSelectionEnabled = value;
                }
                OnPropertyChanged(nameof(ImageSelectionEnabled));
            }
        }
        private bool _StructureSelectionEnabled;
        public bool StructureSelectionEnabled
        {
            get => _StructureSelectionEnabled;
            set
            {
                if (_StructureSelectionEnabled != value)
                {
                    _StructureSelectionEnabled = value;
                }
                OnPropertyChanged(nameof(StructureSelectionEnabled));
            }
        }
        private bool _IsHUInputTextBoxReadOnly;
        public bool IsHUInputTextBoxReadOnly
        {
            get => _IsHUInputTextBoxReadOnly;
            set
            {
                if (_IsHUInputTextBoxReadOnly != value)
                {
                    _IsHUInputTextBoxReadOnly = value;
                }
                OnPropertyChanged(nameof(IsHUInputTextBoxReadOnly));
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
        private bool _IsOverrideBtnEnabled;
        public bool IsOverrideBtnEnabled
        {
            get => _IsOverrideBtnEnabled;
            set
            {
                if (_IsOverrideBtnEnabled != value)
                {
                    _IsOverrideBtnEnabled = value;
                }
                OnPropertyChanged(nameof(IsOverrideBtnEnabled));
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

        private readonly EsapiWorker _worker;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public OverrideViewModel(EsapiWorker esapiWorker)
        {
            _worker = esapiWorker;
            _collectionLock = new object();
            ConvertCmd = new RelayCommand(ConvertImages);
            ProgressBarValue = 0;
            Images = new ObservableCollection<string>();
            StructureList = new ObservableCollection<string>();
            ImageSelectionEnabled = true;
            StructureSelectionEnabled = true;
            IsHUInputTextBoxReadOnly = false;
            ImageSelected = null;
            InputText = "0";
            TextBox = "Please start by selecting a 3D CT image.";
            _worker.Run(scriptContext =>
            {
                if (scriptContext.Patient == null)
                {
                    return;
                }
                foreach (var image3D in scriptContext.Patient.Studies.SelectMany(study => study.Images3D).ToList())
                {
                    Images.Add($"{image3D.Series.Id} ({image3D.Id})");
                }
            });
        }
        public ICommand ConvertCmd { get; }
        private void ConvertImages()
        {
            if (ImageSelected == null)
            {
                System.Windows.MessageBox.Show("Please first select a CT image from the image selection drop-down list.");
                return;
            }
            if (StructureSelected == null)
            {
                System.Windows.MessageBox.Show("Please first select a structure from the structure selection drop-down list.");
                return;
            }
            // Check if the textbox entry is valid.
            short inputNumber = 0;
            try
            {
                inputNumber = Int16.Parse(InputText);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }
            if (inputNumber > 3000 || inputNumber < -3000)
            {
                System.Windows.MessageBox.Show("Input out of range. Please correct the input.");
                return;
            }
            // here open a folder dialogue ask the user to select an output folder.
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select the directory where new files will be saved.";
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                TextBox = string.Empty;
                var folderPath = dialog.SelectedPath;
                _worker.Run(scriptContext =>
                {
                    ImageSelectionEnabled = false;
                    StructureSelectionEnabled = false;
                    IsOverrideBtnEnabled = false;
                    IsHUInputTextBoxReadOnly = true;
                    TextBox += "Task is running...\n";
                    var seriesId = ImageSelected.Split('(')[0].Remove(ImageSelected.Split('(')[0].Length - 1);
                    var imageId = ImageSelected.Split('(')[1].Split(')')[0];
                    var CurrentImage3D = scriptContext.Patient.Studies.SelectMany(study => study.Images3D).ToList().Where(img =>
                    (img.Series.Id == seriesId && img.Id == imageId)).FirstOrDefault();
                    var structureset = scriptContext.Patient.StructureSets.Where(s => (s.Image.Id == imageId && s.Image.Series.Id == seriesId)).FirstOrDefault();
                    Structure structure = structureset.Structures.Where(s => s.Id == StructureSelected).FirstOrDefault();
                    int ZSlices = CurrentImage3D.ZSize;
                    I.PixelIDValueEnum pixelType = I.PixelIDValueEnum.sitkFloat32;
                    I.VectorUInt32 image3DSize = new I.VectorUInt32(new uint[] { (uint)CurrentImage3D.XSize, (uint)CurrentImage3D.YSize, (uint)CurrentImage3D.ZSize });
                    I.Image itkImage = new I.Image(image3DSize, pixelType);
                    I.VectorDouble spacing3D = new I.VectorDouble(new double[] { CurrentImage3D.XRes, CurrentImage3D.YRes, CurrentImage3D.ZRes });
                    itkImage.SetSpacing(spacing3D);
                    I.VectorDouble origin = new I.VectorDouble(new double[] { CurrentImage3D.Origin.x, CurrentImage3D.Origin.y, CurrentImage3D.Origin.z });
                    itkImage.SetOrigin(origin);

                    I.PixelIDValueEnum pixelTypeDCM = I.PixelIDValueEnum.sitkInt16;
                    I.VectorUInt32 imageSize = new I.VectorUInt32(new uint[] { itkImage.GetSize()[0], itkImage.GetSize()[1] });
                    I.Image itkImageDCM = new I.Image(imageSize, pixelTypeDCM);
                    I.VectorDouble spacingDCM = new I.VectorDouble(new double[] { itkImage.GetSpacing()[0], itkImage.GetSpacing()[1] });
                    itkImageDCM.SetSpacing(spacingDCM);
                    I.ImageFileWriter writer = new I.ImageFileWriter();
                    writer.KeepOriginalImageUIDOn();
                    // DICOM metadata that are common to each slice
                    var patient = scriptContext.Patient;
                    itkImageDCM.SetMetaData("0010|0010", $"{patient.LastName}^{patient.FirstName}^{patient.MiddleName}");
                    itkImageDCM.SetMetaData("0010|0020", patient.Id);
                    itkImageDCM.SetMetaData("0008|0008", "ORIGINAL\\PRIMARY\\AXIAL");
                    itkImageDCM.SetMetaData("0008|0070", CurrentImage3D.Series.ImagingDeviceManufacturer);
                    itkImageDCM.SetMetaData("0008|0020", DateTime.Now.ToString("yyyyMMdd"));  // study date
                    itkImageDCM.SetMetaData("0008|0030", DateTime.Now.ToString("HHmmss.ffffff")); // study time
                    string seriesDescription = "Corrected on " + DateTime.Now.ToString("MMddyyyy");
                    itkImageDCM.SetMetaData("0008|103E", seriesDescription);  // series description
                    itkImageDCM.SetMetaData("0008|1090", CurrentImage3D.Series.ImagingDeviceModel);
                    itkImageDCM.SetMetaData("0018|0050", CurrentImage3D.ZRes.ToString()); // slice thickness
                                                                                          // itkImageDCM.SetMetaData("0020|0012", ?); // acquisition number
                    itkImageDCM.SetMetaData("0018|5100", "HFS");
                    string newStudyUID = MakeNewUID(CurrentImage3D.Series.Study.UID);
                    itkImageDCM.SetMetaData("0020|000D", newStudyUID);   // study UID.
                    string newSeriesUID = MakeNewUID(CurrentImage3D.Series.UID);
                    itkImageDCM.SetMetaData("0020|000E", newSeriesUID);  // series UID.
                    itkImageDCM.SetMetaData("0020|0052", CurrentImage3D.Series.FOR);  // use the same frame of reference UID as the original image series.
                    itkImageDCM.SetMetaData("0020|1040", "BB"); // position reference indicator
                    itkImageDCM.SetMetaData("0020|0012", "1"); // acquisition number
                    itkImageDCM.SetMetaData("0028|1054", "HU"); // rescale type (Hounsfield Units or not)
                    I.VectorUInt32 index = new I.VectorUInt32(new uint[] { 0, 0, 0 });
                    I.VectorUInt32 indexPlane = new I.VectorUInt32(new uint[] { 0, 0 });
                    int[,] voxelPlane = new int[CurrentImage3D.XSize, CurrentImage3D.YSize];

                    System.Collections.BitArray segmentStride = new System.Collections.BitArray((int)CurrentImage3D.XSize);
                    for (int Z = 0; Z < ZSlices; Z++)
                    {
                        CurrentImage3D.GetVoxels(Z, voxelPlane);
                        for (int Y = 0; Y < CurrentImage3D.YSize; Y++)
                        {
                            var start = CurrentImage3D.Origin + CurrentImage3D.YDirection * Y * CurrentImage3D.YRes + CurrentImage3D.ZDirection * Z * CurrentImage3D.ZRes;
                            var end = start + CurrentImage3D.XDirection * CurrentImage3D.XRes * CurrentImage3D.XSize;
                            var profile = structure.GetSegmentProfile(start, end, segmentStride);
                            for (int X = 0; X < CurrentImage3D.XSize; X++)
                            {
                                indexPlane[0] = (uint)X;
                                indexPlane[1] = (uint)Y;
                                itkImageDCM.SetPixelAsInt16(indexPlane, (Int16)CurrentImage3D.VoxelToDisplayValue(voxelPlane[X, Y]));
                                if (segmentStride[X])
                                {
                                    itkImageDCM.SetPixelAsInt16(indexPlane, inputNumber);
                                }
                            }
                        }
                        if (CurrentImage3D.Series.Modality == SeriesModality.CT)
                        {
                            itkImageDCM.SetMetaData("0008|0060", "CT");
                            itkImageDCM.SetMetaData("0018|0060", "120");
                        }
                        string positionPatient, sliceLocation;
                        if (itkImage.GetOrigin()[0] % 1 == 0)
                        {
                            positionPatient = $"{(int)itkImage.GetOrigin()[0]}";
                        }
                        else
                        {
                            positionPatient = string.Format("{0:0.00}", itkImage.GetOrigin()[0]);
                        }
                        if (itkImage.GetOrigin()[1] % 1 == 0)
                        {
                            positionPatient += $"\\{(int)itkImage.GetOrigin()[1]}";
                        }
                        else
                        {
                            positionPatient += "\\" + string.Format("{0:0.00}", itkImage.GetOrigin()[1]);
                        }
                        double zPosition = itkImage.GetOrigin()[2] + Z * itkImage.GetSpacing()[2];
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
                        writer.SetFileName(Path.Combine(folderPath, $"{scriptContext.Patient.Id}_updated_{Z}.DCM"));
                        writer.Execute(itkImageDCM);
                        int percent = (int)((float)(Z + 1) / ZSlices * 100);
                        ProgressBarValue = percent;
                        if ((Z + 1) / 10 * 10 == (Z + 1))
                        {
                            TextBox += $"Processing slice #{Z + 1}\n";
                        }
                    }
                    TextBox += "Task is complete.\n";
                    TextBox += $"New files were saved in this location: {folderPath}";
                    ImageSelectionEnabled = true;
                    StructureSelectionEnabled = true;
                    IsHUInputTextBoxReadOnly = false;
                    IsOverrideBtnEnabled = true;
                });
            }
            else if (result == DialogResult.Cancel)
            {
                System.Windows.MessageBox.Show("Please select an output folder.");
            }
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
        private void PopulateStructureList()
        {
            StructureList.Clear();
            _worker.Run(scriptContext =>
            {
                if (ImageSelected != null)
                {
                    var seriesId = ImageSelected.Split('(')[0].Remove(ImageSelected.Split('(')[0].Length - 1);
                    var imageId = ImageSelected.Split('(')[1].Split(')')[0];
                    Image imageToUse = scriptContext.Patient.Studies.SelectMany(study => study.Images3D).ToList().Where(img =>
                    (img.Series.Id == seriesId && img.Id == imageId)).FirstOrDefault();
                    if (imageToUse != null)
                    {
                        var ss = scriptContext.Patient.StructureSets.Where(s => (s.Image.Id == imageId && s.Image.Series.Id == seriesId)).FirstOrDefault();
                        if (ss == null)
                        {
                            TextBox = "No structures are found for this image. Please select another image.";
                        }
                        else
                        {
                            foreach (var s in ss.Structures)
                            {
                                StructureList.Add(s.Id);
                            }
                        }
                    }

                }
            });
        }


    }
}


