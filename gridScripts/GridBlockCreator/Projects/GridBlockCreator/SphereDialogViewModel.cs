using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace GridBlockCreator
{
    public class SphereDialogViewModel : Notifier
    {
        private float _MinSpacing;

        public float MinSpacing
        {
            get { return _MinSpacing; }
            set { _MinSpacing = value; OnPropertyChanged("MinSpacing"); }
        }


        private bool _IsHex;

        public bool IsHex
        {
            get { return _IsHex; }
            set { _IsHex = value; OnPropertyChanged("IsHex"); }
        }

        private bool _IsRect;

        public bool IsRect
        {
            get { return _IsRect; }
            set { _IsRect = value; OnPropertyChanged("IsRect"); }
        }


        private float _Radius;

        public float Radius
        {
            get { return _Radius; }
            set { _Radius = value; OnPropertyChanged("Radius"); }
        }


        private List<string> targetStructures;
        public List<string> TargetStructures
        {
            get { return targetStructures; }
            set
            {
                targetStructures = value;
                OnPropertyChanged("TargetStructures");
            }
        }

        private int targetSelected;
        public int TargetSelected
        {
            get { return targetSelected; }
            set
            {
                targetSelected = value;
                //updateFullState();
                OnPropertyChanged("TargetSelected");
            }
        }

        int zStart;
        public int ZStart
        {
            get { return zStart; }
            set { zStart = value; OnPropertyChanged("ZStart"); }

        }

        public Structure target;

        int zEnd;
        public int ZEnd
        {
            get { return zEnd; }
            set { zEnd = value; OnPropertyChanged("ZEnd"); }

        }
    
        private ScriptContext context;

        public SphereDialogViewModel(ScriptContext context)
        {



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

            //updateFullState();

        }

        void CRTest(ref Structure gridStructure, float R)
        {
            MessageBox.Show($"Hex | Rect : {IsHex} | {IsRect}");

            target = context.StructureSet.Structures.Where(x => x.Id == "PTV").First();
            //MessageBox.Show($"Target selected as {target}");

            double xCenter = (double)(this.context.Image.XSize) / 2.0 * context.Image.XRes + context.Image.Origin.x;
            double yCenter = (double)(this.context.Image.YSize) / 2.0 * context.Image.YRes + context.Image.Origin.y;
            double zCenter = (double)(zEnd + zStart) / 2.0 * context.Image.ZRes + context.Image.Origin.z;


            MessageBox.Show($"Image center {xCenter} {yCenter} {zCenter}");
            MessageBox.Show($"PTV center {target.CenterPoint.x} {target.CenterPoint.y} {target.CenterPoint.z}");


            for (int z = zStart; z < zEnd; ++z)
            {
                double zCoord = (double)(z) * context.Image.ZRes + context.Image.Origin.z;

                // For each slice find in plane radius
                var z_diff = Math.Abs(zCoord - zCenter);
                if (z_diff > R) // If we are out of range of the sphere continue
                {
                    continue;
                }

                // Otherwise do the thing (make spheres)
                var r_z = Math.Sqrt(Math.Pow(R, 2) - Math.Pow(z_diff, 2));
                

                // Just make one sphere at target center for now
                //var center = new VVector(xCenter,yCenter,zCenter);
                var center = new VVector(target.CenterPoint.x, target.CenterPoint.y, target.CenterPoint.z);
                var contour = CreateContour(center, r_z, 20);
                gridStructure.AddContourOnImagePlane(contour, z);

            }

            //gridStructure.SegmentVolume = gridStructure.SegmentVolume.And(target);
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
                var contour = CreateContour(center, r_z, 20);
                parentStruct.AddContourOnImagePlane(contour, z);

            }
        }

        private List<double> Arange(double start, double stop, double step)
        {
            MessageBox.Show($"Arange with start stop step = {start} {stop} {step}");
            var retval = new List<double>();
            var currentval = start;
            while (currentval < stop)
            {
                retval.Add(currentval);
                currentval += step;
            }
            MessageBox.Show($"Arange created with {retval.Count} points");
            

            return retval;
        }

        private List<VVector> BuildGrid(List<double> xcoords, List<double> ycoords, List<double> zcoords, Structure boundingStruct)
        {
            var retval = new List<VVector>();
            foreach(var x in xcoords)
            {
                foreach(var y in ycoords)
                {
                    foreach(var z in zcoords)
                    {
                        var pt = new VVector(x, y, z);
                        /*if (boundingStruct.IsPointInsideSegment(pt))
                        {
                            retval.Add(pt);
                        }*/
                        retval.Add(pt);
                    }
                }
            }
            return retval;
        }

        public void RectSpheres(ref Structure GridStructure)
        {
            // 1. Generate target plus margin
            var target = context.StructureSet.Structures.Where(x => x.Id == "PTV").First();
            var dummie = context.StructureSet.AddStructure("PTV", "dummie"); 
            dummie.Margin(50);

            // 2. Generate a regular grid accross the image 
            var xcoords = Arange(0, context.Image.XSize * context.Image.XRes, MinSpacing);
            var ycoords = Arange(0, context.Image.YSize * context.Image.YRes, MinSpacing);
            var zcoords = Arange(0, context.Image.ZSize * context.Image.ZRes, MinSpacing);

            MessageBox.Show($"About to create grid");

            // 3. Get points that are not in the image
            var Grid = BuildGrid(xcoords, ycoords, zcoords, dummie);

            MessageBox.Show($"Created grid with {Grid.Count} pts");

            // 4. Make spheres
            foreach(VVector ctr in Grid)
            {
                BuildSphere(GridStructure, ctr, Radius);
            }

            MessageBox.Show("Created Spehres");

            // Cleanup from step 1
            context.StructureSet.RemoveStructure(dummie);
            MessageBox.Show("Deleted dummie struct");
              
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
         
            var grid = context.StructureSet.AddStructure("PTV", "myGrid");
            //CRTest(ref grid, Radius);
            RectSpheres(ref grid);


        }


    }
}
