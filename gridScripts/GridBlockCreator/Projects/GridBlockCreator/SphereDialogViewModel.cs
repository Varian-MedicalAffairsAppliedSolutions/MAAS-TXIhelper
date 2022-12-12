using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using Prism.Modularity;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

// TODO 
// Add user input for cleanup threshold for deleting fractional spheres
// Hexagonal = staggered layers
// Target Border
// Console for output (lower priority)
// Get eclipse v16
// 

namespace GridBlockCreator
{
    public class Spacing:BindableBase
    {
        private double value_;

        public double Value
        {
            get { return value_; }
            set { SetProperty(ref value_, value); }
        }

        private double hex_spacing;

        public double Hex_Spacing
        {
            get { return hex_spacing; }
            set { SetProperty(ref hex_spacing, value); }
        }

        public override string ToString()
        {
            string v = $"{Math.Round(value_, 2)} (Rect) / {Math.Round(hex_spacing, 2)} (Hex)";
            return v;
        }

        private string stringRep;

        public string StringRep
        {
            get { return stringRep; }
            set { SetProperty(ref stringRep, value); }
        }


        public Spacing(double rect_spacing)
        {
            this.value_ = rect_spacing;
            this.Hex_Spacing = rect_spacing * Math.Sqrt(3);
            this.StringRep = this.ToString();
  
            
        }
    }
  
    public class SphereDialogViewModel : BindableBase
    {
        public ObservableCollection<string> LogMsgs;

        private string _LogText;

        public string LogText
        {
            get { return _LogText; }
            set { 
                SetProperty(ref _LogText, value);
                System.Threading.Thread.Sleep(1000);
            }
        }

        private bool progress;

        public bool Progress
        {
            get { return progress; }
            set { SetProperty(ref progress, value); }
        }



        private float _MinSpacing;

        public float MinSpacing
        {
            get { return _MinSpacing; }
            set { SetProperty(ref _MinSpacing, value); }
        }


        private bool _IsHex;

        public bool IsHex
        {
            get { return _IsHex; }
            set { SetProperty(ref _IsHex, value); }
        }

        private bool _IsRect;

        public bool IsRect
        {
            get { return _IsRect; }
            set { SetProperty(ref _IsRect, value); }
        }


        private float _Radius;

        public float Radius
        {
            get { return _Radius; }
            set { SetProperty(ref _Radius, value); }
        }


        private List<string> targetStructures;
        public List<string> TargetStructures
        {
            get { return targetStructures; }
            set
            {
                SetProperty(ref targetStructures, value);
            }
        }

        private int targetSelected;
        public int TargetSelected
        {
            get { return targetSelected; }
            set
            {
                SetProperty(ref targetSelected, value);
            }
        }

        private float vThresh;
        public float VThresh
        {
            get { return vThresh; }
            set { SetProperty(ref vThresh, value); }
        }

        private int zStart;
        public int ZStart
        {
            get { return zStart; }
            set { SetProperty(ref zStart, value); }

        }

        public Structure target;

        int zEnd;
        public int ZEnd
        {
            get { return zEnd; }
            set { SetProperty(ref zEnd, value); }

        }

        private List<Spacing> validSpacings;
        public List<Spacing> ValidSpacings
        {
            get { return validSpacings; }
            set
            {
                SetProperty(ref validSpacings, value);
            }
        }

        private Spacing spacingSelected;
        public Spacing SpacingSelected
        {
            get { return spacingSelected; }
            set
            {
                SetProperty(ref spacingSelected, value);
            }
        }




        private ScriptContext context;

        public SphereDialogViewModel(ScriptContext context)
        {

            VThresh = 0;
            this.context = context;

            // Set valid spacings
            ValidSpacings = new List<Spacing>();
            var spacing = context.Image.ZRes;
            for (int i = 1; i<30; i++) {
                ValidSpacings.Add(new Spacing(spacing * i));
            }

            // Default to first value
            //SpacingSelected = ValidSpacings.FirstOrDefault().Item1;

          
            // Set zStart and zEnd
            zStart = 0;
            zEnd = context.Image.ZSize;


            //target structures
            targetStructures = new List<string>();
            targetSelected = -1;
            //plan target
            string planTargetId = null;

            foreach (var i in context.StructureSet.Structures)
            {
                if (i.DicomType != "PTV") continue;
                targetStructures.Add(i.Id);
                if (planTargetId == null) continue;
                if (i.Id == planTargetId) targetSelected = targetStructures.Count() - 1;
            }


        }


        private void BuildSphere(Structure parentStruct, VVector center, float R)
        {
            for (int z = zStart; z < zEnd; ++z)
            {
                double zCoord = (double)(z) * (context.Image.ZRes) + context.Image.Origin.z;

                // For each slice find in plane radius
                var z_diff = Math.Abs(zCoord - center.z);
                if (z_diff > R) // If we are out of range of the sphere continue
                {
                    continue;
                }

                // Otherwise do the thing (make spheres)
                var r_z = Math.Sqrt(Math.Pow(R, 2) - Math.Pow(z_diff, 2));
                var contour = CreateContour(center, r_z, 500);
                parentStruct.AddContourOnImagePlane(contour, z);

            }
        }

        private List<double> Arange(double start, double stop, double step)
        {
            //log.Debug($"Arange with start stop step = {start} {stop} {step}\n");
            var retval = new List<double>();
            var currentval = start;
            while (currentval < stop)
            {
                retval.Add(currentval);
                currentval += step;
            }
            
            

            return retval;
        }

        private List<VVector> BuildGrid(List<double> xcoords, List<double> ycoords, List<double> zcoords)
        {

            var retval = new List<VVector>();
            foreach(var x in xcoords)
            {
                foreach(var y in ycoords)
                {
                    foreach(var z in zcoords)
                    {
                        var pt = new VVector(x, y, z);
                        
                        retval.Add(pt);
                    }
                }
            }
            return retval;
        }

        private List<VVector> BuildHexGrid(double Xstart, double Xsize, double Ystart, double Ysize, double Zstart, double Zsize)
        {
            double A = SpacingSelected.Value * (Math.Sqrt(3) / 2);
            var retval = new List<VVector>();

            void CreateLayer(double zCoord, double x0, double y0)
            {
                // create planar hexagonal sphere packing grid
                var yeven = Arange(y0, y0 + Ysize, 2 * A);
                var xeven = Arange(x0, x0 + Xsize, SpacingSelected.Value);
                foreach (var y in yeven)
                {
                    foreach(var x in xeven)
                    {
                        retval.Add(new VVector(x, y, zCoord));
                        retval.Add(new VVector(x + (SpacingSelected.Value/2), y + A, zCoord));
                    }
                }

              
            }

            foreach(var z in Arange(Zstart, Zstart + Zsize, 2 * A))
            {
                CreateLayer(z, Xstart, Ystart);
                CreateLayer(z + A, Xstart + (SpacingSelected.Value/2), Ystart + (A/2));
 
            }

            return retval;
        }

        

        public void BuildSpheres(bool makeIndividual, bool alignGrid)
        {
            Structure structMain;
            Progress = true;
            // Check vol thresh for spheres
            if (VThresh > 100 || VThresh < 0)
            {
                MessageBox.Show("Volume threshold must be between 0 and 100");
            }



            // Check target
            if (targetSelected == -1)
            {
                MessageBox.Show("Must have target selected, canceling operation.");
                return;
            }
            var target_name = targetStructures[targetSelected];
            //log.Info($"Target selected with ID: {target_name}");
            var target = context.StructureSet.Structures.Where(x => x.Id == target_name).First();

            //(4/3) * Math.PI * Math.Pow((Radius/10), 3) * (VThresh / 100);

            // Generate a regular grid accross the dummie bounding box 
            var bounds = target.MeshGeometry.Bounds;
         
            // Get points that are not in the image
            List<VVector> Grid;
            if (IsHex)
            {
                Grid = BuildHexGrid(bounds.X, bounds.SizeX, bounds.Y, bounds.SizeY, bounds.Z, bounds.SizeZ);
                structMain = CreateStructure("LatticeHex", true);
                //log.Info($"Hexagonal grid built with {Grid.Count} points.");
            }
            else if (IsRect)
            {
                if(alignGrid)
                {
                    // Snap z to nearest z slice
                    // where z slices = img.origin.z + (c * zres)
                    // x, y, z --> dropdown all equal
                    // z0 --> rounded to nearest grid slice
                    var zSlices = new List<double>();
                    var plane_idx = (bounds.Z - context.Image.Origin.z) / context.Image.ZRes;
                    int plane_int = (int)Math.Round(plane_idx);

                    var z0 = context.Image.Origin.z + (plane_int * context.Image.ZRes);
                    MessageBox.Show($"Original z | Snapped z = {bounds.Z} | {z0}");

                    var xcoords = Arange(bounds.X, bounds.X + bounds.SizeX, SpacingSelected.Value);
                    var ycoords = Arange(bounds.Y, bounds.Y + bounds.SizeY, SpacingSelected.Value);
                    var zcoords = Arange(z0, z0 + bounds.SizeZ, SpacingSelected.Value);
                    Grid = BuildGrid(xcoords, ycoords, zcoords);

                }
                else
                {
                    var xcoords = Arange(bounds.X, bounds.X + bounds.SizeX, MinSpacing);
                    var ycoords = Arange(bounds.Y, bounds.Y + bounds.SizeY, MinSpacing);
                    var zcoords = Arange(bounds.Z, bounds.Z + bounds.SizeZ, MinSpacing);
                    Grid = BuildGrid(xcoords, ycoords, zcoords);
                }

                structMain = CreateStructure("LatticeRect", true);
                //log.Info($"Rectangular grid built with {Grid.Count} points.");
            }
            else
            {
                MessageBox.Show("No pattern selected. Returning.");
                Progress = false;
                return;
            }



            // 4. Make spheres
            int sphere_count = 0;

            var prevSpheres = context.StructureSet.Structures.Where(x => x.Id.Contains("Sphere")).ToList();
            int deleted_spheres = 0;
            foreach(var sp in prevSpheres)
            {
                context.StructureSet.RemoveStructure(sp);
                deleted_spheres++;
            }
            if (deleted_spheres > 0) { MessageBox.Show($"{deleted_spheres} pre-existing spheres deleted "); }


            // Hold on to single sphere ids
            var singleIds = new List<string>();
            var singleVols = new List<double>();

            // Create all individual spheres
            foreach (VVector ctr in Grid)
            { 
                if (makeIndividual)
                {
                    // Create a new structure and build sphere on that
                    var singleId = $"Sphere_{sphere_count}";
                    var singleSphere = CreateStructure(singleId, false);
                    BuildSphere(singleSphere, ctr, Radius);
                    
                    // Crop to target
                    singleSphere.SegmentVolume = singleSphere.SegmentVolume.And(target);

                    sphere_count++;

                    singleIds.Add(singleId);
                    singleVols.Add(singleSphere.Volume);
                }
            }

            var volThresh = singleVols.Max() * (VThresh / 100);



            foreach(string id_ in singleIds)
            { 
                // delete small spheres
                var singleSphere = context.StructureSet.Structures.Where(x => x.Id == id_).FirstOrDefault();
                if (singleSphere != null)
                {
                    if(singleSphere.Volume <= volThresh)
                    {
                        // Delete
                        //MessageBox.Show($"Deleted sphere based on volume threshold: {singleSphere.Volume} >= {volThresh}");
                        context.StructureSet.RemoveStructure(singleSphere);
                        continue;
                    }
                }

                // If here sphere is big enough
                structMain.SegmentVolume = structMain.SegmentVolume.Or(singleSphere);
                singleSphere.ConvertToHighResolution();

            }

            


            // And the main structure with target
            structMain.SegmentVolume = structMain.SegmentVolume.And(target);

            structMain.ConvertToHighResolution();

            MessageBox.Show("Created Spheres");

            Progress = false;


        }

        VVector[] CreateContour(VVector center, double radius, int nOfPoints)
        {
            VVector[] contour = new VVector[nOfPoints + 1];
            double angleIncrement = Math.PI * 2.0 / Convert.ToDouble(nOfPoints);
            for (int i = 0; i < nOfPoints; ++i)
            {
                double angle = Convert.ToDouble(i) * angleIncrement;
                double xDelta = radius * Math.Cos(angle);
                double yDelta = radius * Math.Sin(angle);
                VVector delta = new VVector(xDelta, yDelta, 0.0);
                contour[i] = center + delta;
            }
            contour[nOfPoints] = contour[0];

            return contour;
        }

        public int _GetSlice(double z, StructureSet SS)
        {
            var imageRes = SS.Image.ZRes;
            return Convert.ToInt32((z - SS.Image.Origin.z) / imageRes);
        }

        private Structure HightoLow(StructureSet structureset, Structure structure)
        {

            var lowresstructureId = "lowres" + structure.Id;
            var lowresstructure = structureset.AddStructure("CONTROL", lowresstructureId);

            var mesh = structure.MeshGeometry.Bounds;
            var meshLow = _GetSlice(mesh.Z, structureset);
            var meshUp = _GetSlice(mesh.Z + mesh.SizeZ, structureset) + 1;

            for (int j = meshLow; j <= meshUp; j++)
            {
                var contours = structure.GetContoursOnImagePlane(j);
                if (contours.Length > 0)
                {
                    lowresstructure.AddContourOnImagePlane(contours[0], j);
                }
            }

            lowresstructure.SegmentVolume = lowresstructure;
            return lowresstructure;

        }

        private Structure CreateStructure(string structName, bool showMessage)
        {
            string msg = $"New structure ({structName}) created.";
            var prevStruct = context.StructureSet.Structures.Where(x => x.Id == structName).FirstOrDefault();
            if (prevStruct != null)
            {
                context.StructureSet.RemoveStructure(prevStruct);
                msg += " Old structure overwritten.";
            }

            var structure = context.StructureSet.AddStructure("PTV", structName);

            if (showMessage) { MessageBox.Show(msg); } 
            return structure;   
        }
        
        public void CreateGrid()
        {
            // Caleb Summary
            // Add 'Grid' structure set base (based how? same struct?) on PTV
            // pass gridstructure to createGridStructure
            // This gets some vars related to rod center and position
            // For each slice between Z start and Z end
            // NOTE: for checking sphere touching try: https://gdbooks.gitbooks.io/3dcollisions/content/Chapter1/point_in_sphere.html
            //Start prepare the patient
            context.Patient.BeginModifications();


            
            BuildSpheres(true, true);

        }




    }
}
