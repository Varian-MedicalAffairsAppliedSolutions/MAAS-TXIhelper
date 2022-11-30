using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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

        int zStart;
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

    
        private ScriptContext context;

        public SphereDialogViewModel(ScriptContext context)
        {

            LogMsgs = new ObservableCollection<string>();
            LogMsgs.Add("Hey there");
            this.context = context;
          
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
            MessageBox.Show("Buildng hex grid.");
            double A = MinSpacing * (Math.Sqrt(3) / 2);
            var retval = new List<VVector>();

            void CreateLayer(double zCoord, double x0, double y0)
            {
                // create planar hexagonal sphere packing grid
                var yeven = Arange(y0, y0 + Ysize, 2 * A);
                var xeven = Arange(x0, x0 + Xsize, MinSpacing);
                foreach (var y in yeven)
                {
                    foreach(var x in xeven)
                    {
                        retval.Add(new VVector(x, y, zCoord));
                        retval.Add(new VVector(x + (MinSpacing/2), y + A, zCoord));
                    }
                }

              
            }

            foreach(var z in Arange(Zstart, Zstart + Zsize, 2 * A))
            {
                CreateLayer(z, Xstart, Ystart);
                CreateLayer(z + A, Xstart + (MinSpacing/2), Ystart + (A/2));
 
            }

            return retval;
        }

        

        public void BuildSpheres(ref Structure structMain, bool makeIndividual)
        {
            // Check target
            if (targetSelected == -1)
            {
                MessageBox.Show("Must have target selected, canceling operation.");
                return;
            }
            var target_name = targetStructures[targetSelected];
            //log.Info($"Target selected with ID: {target_name}");
            var target = context.StructureSet.Structures.Where(x => x.Id == target_name).First();

            // Generate a regular grid accross the dummie bounding box 
            var bounds = target.MeshGeometry.Bounds;
         

            // 3. Get points that are not in the image
            List<VVector> Grid;
            if (IsHex)
            {
                Grid = BuildHexGrid(bounds.X, bounds.SizeX, bounds.Y, bounds.SizeY, bounds.Z, bounds.SizeZ);
                structMain.Id += "Hex";
                //log.Info($"Hexagonal grid built with {Grid.Count} points.");
            }
            else if (IsRect)
            {
                var xcoords = Arange(bounds.X, bounds.X + bounds.SizeX, MinSpacing);
                var ycoords = Arange(bounds.Y, bounds.Y + bounds.SizeY, MinSpacing);
                var zcoords = Arange(bounds.Z, bounds.Z + bounds.SizeZ, MinSpacing);
                Grid = BuildGrid(xcoords, ycoords, zcoords);
                structMain.Id += "Rect";
                //log.Info($"Rectangular grid built with {Grid.Count} points.");
            }
            else
            {
                MessageBox.Show("No pattern selected. Returning.");
                return;
            }


            // 4. Make spheres
            int sphere_count = 0;
            foreach(VVector ctr in Grid)
            {
                // Add sphere to structure
                BuildSphere(structMain, ctr, Radius);

                if (makeIndividual)
                {
                    // Create a new structure and build sphere on that
                    var singleSphere = context.StructureSet.AddStructure("PTV", $"Sphere_{sphere_count}");
                    BuildSphere(singleSphere, ctr, Radius);
                    sphere_count++;
                }
            }

            //log.Info("Created Spheres");
            structMain.ConvertToHighResolution();
            MessageBox.Show("Finished");


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

            /*var oldStruct = context.StructureSet.Structures.ToList().Where(x => x.Id == "Lat").First();
            if (oldStruct != null) {
                context.StructureSet.RemoveStructure(oldStruct);
            }*/
            var LatticeHiRes = context.StructureSet.AddStructure("PTV", "Lattice");
            //LatticeHiRes.ConvertToHighResolution();

            //grid.ConvertToHighResolution();
            //CRTest(ref grid, Radius);
            BuildSpheres(ref LatticeHiRes, true);
            //MessageBox.Show(LogText);

        }


    }
}
