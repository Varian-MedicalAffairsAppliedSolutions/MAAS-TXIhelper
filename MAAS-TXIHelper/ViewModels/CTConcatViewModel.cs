
using MAAS_TXIHelper.Core;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using V = VMS.TPS.Common.Model.API;

// TODO 7.25
// Add "work in progress" to the two empty tabs
// Change sampling slice thickness to something different
// Create dropdown for spacing (original, 1,2,2.5,3,4,5,6,8,10 mm)
// Add series.ID (Image.ID) to the xaml
// Write save directory to config file
// Add selected registration checks (determine if the primary is HFS, and the secondary is FFS)


namespace MAAS_TXIHelper.ViewModels
{
    
    public class CTConcatViewModel: BindableBase
    {
        public DelegateCommand ConcatenateCmd { get; set; } 
        public ObservableCollection<Image> PrimaryImages { get; set; }
        public ObservableCollection<Image> SecondaryImages { get; set; }
        public ObservableCollection<Registration> Registrations { get; set; }

        private double _SelectedSpacing;

        public double SelectedSpacing
        {
            get { return _SelectedSpacing; }
            set { SetProperty(ref _SelectedSpacing, value); }
        }

        public ObservableCollection<double> ResampleSpacings { get; set; }

        private Patient _patient;

        private string _SaveDir;
        public string SaveDir
        {
            get { return _SaveDir; }
            set
            {
                SetProperty(ref _SaveDir, value);
            }
        }

        private Image _PrimaryImage;
        public Image PrimaryImage
        {
            get { return _PrimaryImage; }
            set { 
                SetProperty(ref _PrimaryImage, value);
                //ConcatenateCmd.RaiseCanExecuteChanged();
                PopulateSecondaryImages();
                PopulateSelectedSpacings();
            }
        }

        private Image _SecondaryImage;
        public Image SecondaryImage
        {
            get { return _SecondaryImage; }
            set { 
                SetProperty(ref _SecondaryImage, value);

                //ConcatenateCmd.RaiseCanExecuteChanged();
            }
        }

        private Registration _Registration;
        public Registration Registration
        {
            get { return _Registration; }
            set { 
                SetProperty(ref _Registration, value);
                //ConcatenateCmd.RaiseCanExecuteChanged();
            }
        }

        private void PopulateSelectedSpacings()
        {
            if (PrimaryImage != null)
            {
                ResampleSpacings = new ObservableCollection<double>
                {
                    PrimaryImage.ZRes, 2.5, 5, 7.5, 10
                };

                SelectedSpacing = ResampleSpacings.First();
            }
        }


        private void PopulateSecondaryImages()
        {
            SecondaryImages.Clear();
            SecondaryImage = null;
            Registrations.Clear();
            Registration = null;

            if (PrimaryImage != null)
            {
                foreach (Registration registration in _patient.Registrations)
                {
                    if (registration.SourceFOR == PrimaryImage.FOR)
                    {
                        var secondaryImage = PrimaryImages.FirstOrDefault(image => image.FOR == registration.RegisteredFOR);
                        if (secondaryImage != null && !SecondaryImages.Contains(secondaryImage))
                            SecondaryImages.Add(secondaryImage);
                    }
                    else if (registration.RegisteredFOR == PrimaryImage.FOR)
                    {
                        var secondaryImage = PrimaryImages.FirstOrDefault(image => image.FOR == registration.SourceFOR);
                        if (secondaryImage != null && !SecondaryImages.Contains(secondaryImage))
                            SecondaryImages.Add(secondaryImage);
                    }

                    Registrations.Add(registration);
                }
            }
        }

        private void AssertDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new Exception($"Directory {directoryPath} not found!");
            }
        }
       
        private void OnConcatenate()
        {
            if (_Registration != null && _PrimaryImage != null && _SecondaryImage != null)
            {

                AssertDirectoryExists(SaveDir);
                var core = new CTConcat(_patient, PrimaryImage, SecondaryImage, Registration, SaveDir);
                core.Execute();
                MessageBox.Show("Concatenation finished. Please close window and import new concatenated series.");
            }
            else
            {
                MessageBox.Show("Must have Primary, secondary, and registration selected to concatenate.");
            }
        }

        private bool CanConcatenate()
        {
            return true;
        }

        public CTConcatViewModel(ScriptContext context) {

            SaveDir = @"C:\Temp";

            PrimaryImages = new ObservableCollection<Image>();
            SecondaryImages = new ObservableCollection<Image>();
            Registrations = new ObservableCollection<Registration>();
            ConcatenateCmd = new DelegateCommand(OnConcatenate, CanConcatenate);

            ResampleSpacings = new ObservableCollection<double>
            {
                2.5, 5, 7.5, 10
            };

            _patient = context.Patient;

            // Populate primary images
            foreach (var pi in context.Patient.Studies.SelectMany(study => study.Images3D).ToList())
            {
                PrimaryImages.Add(pi);
            }
            
        }
    }
}
