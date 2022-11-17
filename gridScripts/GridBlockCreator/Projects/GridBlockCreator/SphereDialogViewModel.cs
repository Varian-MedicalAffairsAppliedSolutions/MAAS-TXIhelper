using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
// https://mathworld.wolfram.com/CirclePacking.html

namespace GridBlockCreator
{
  
    public class SphereDialogViewModel : BindableBase
    {

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



            this.context = context;
            Console.WriteLine("Log output:\n");
            Console.WriteLine("TestLine\n");

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

            //updateFullState();

        }


        private void BuildSphere(Structure parentStruct, VVector center, float R)
        {
            for (int z = zStart; z < zEnd; ++z)
            {
                double zCoord = (double)(z) * context.Image.ZRes + context.Image.Origin.z;

                // For each slice find in plane radius
                var z_diff = Math.Abs(zCoord - center.z);
                if (z_diff > R) // If we are out of range of the sphere continue
                {
                    continue;
                }

                // Otherwise do the thing (make spheres)
                var r_z = Math.Sqrt(Math.Pow(R, 2) - Math.Pow(z_diff, 2));
                var contour = CreateContour(center, r_z, 200);
                parentStruct.AddContourOnImagePlane(contour, z);

            }
        }

        private List<double> Arange(double start, double stop, double step)
        {
            Console.WriteLine($"Arange with start stop step = {start} {stop} {step}\n");
            var retval = new List<double>();
            var currentval = start;
            while (currentval < stop)
            {
                retval.Add(currentval);
                currentval += step;
            }
            Console.WriteLine($"Arange created with {retval.Count} points\n");
            

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

        public void BuildSpheres(ref Structure structHi)
        {
            // 1. Generate target plus margin
            Console.WriteLine($"Finding target\n");
            var target = context.StructureSet.Structures.Where(x => x.Id == "PTV").First();
            Console.WriteLine($"Target here {target.Id}\n");
            var dummie = context.StructureSet.AddStructure("PTV", "dummie"); 
            dummie.Margin(50);
            LogText +=$"Dummie created with margin\n";

            // 2. Generate a regular grid accross the dummie bounding box 
            var bounds = target.MeshGeometry.Bounds;
            LogText +=$"Bounds = {bounds}\n";
            var xcoords = Arange(bounds.X, bounds.X + bounds.SizeX, MinSpacing);
            var ycoords = Arange(bounds.Y, bounds.Y + bounds.SizeY, MinSpacing);
            var zcoords = Arange(bounds.Z, bounds.Z + bounds.SizeZ, MinSpacing);

            LogText +=$"About to create grid\n";

            // 3. Get points that are not in the image
            var Grid = BuildGrid(xcoords, ycoords, zcoords);

            LogText +=$"Created grid with {Grid.Count} pts\n";

            // 4. Make spheres
            foreach(VVector ctr in Grid)
            {
                BuildSphere(structHi, ctr, Radius);
            }

            LogText += "Created Spheres\n";
            System.Threading.Thread.Sleep(10000);

            // And with the target
            // structLo.SegmentVolume = structHi.And(structHi);

            //structLo.SegmentVolume = structLo.SegmentVolume.And(target);

            //var targetHiRes = context.StructureSet.AddStructure("PTV", "targetHiRes");
            //targetHiRes.SegmentVolume = targetHiRes.And(target);
            //targetHiRes.ConvertToHighResolution();

            structHi.SegmentVolume = structHi.SegmentVolume.And(target);
            structHi.ConvertToHighResolution();

            //HightoLow(context.StructureSet, structHi);

            // Cleanup from step 1
            //context.StructureSet.RemoveStructure(dummie);
            //LogText +="Deleted dummie struct");
            MessageBox.Show("Here");
              
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
         
       
            var LatticeHiRes = context.StructureSet.AddStructure("PTV", "LatticeHiRes");
            //LatticeHiRes.ConvertToHighResolution();

            //grid.ConvertToHighResolution();
            //CRTest(ref grid, Radius);
            BuildSpheres(ref LatticeHiRes);
            //MessageBox.Show(LogText);

        }


    }
}
