using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using Prism.Modularity;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


namespace GridBlockCreator
{
    public class Spacing:BindableBase
    {
        // Helper class for representing Rectagonal vs Hexagonal Spacing
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

        private bool deleteIndividual;

        public bool DeleteIndividual
        {
            get { return deleteIndividual; }
            set { SetProperty(ref deleteIndividual, value); }
        }

        private bool isHex;

        public bool IsHex
        {
            get { return isHex; }
            set { SetProperty(ref isHex, value); }
        }

        private bool isRect;

        public bool IsRect
        {
            get { return isRect; }
            set { SetProperty(ref isRect, value); }
        }

        private double xShift;

        public double XShift
        {
            get { return xShift; }
            set { SetProperty(ref xShift, value); }
        }

        private double yShift;

        public double YShift
        {
            get { return yShift; }
            set { SetProperty(ref yShift, value); }
        }


        private float radius;

        public float Radius
        {
            get { return radius; }
            set { SetProperty(ref radius, value); }
        }


        private List<string> targetStructures;
        public List<string> TargetStructures
        {
            get { return targetStructures; }
            set { SetProperty(ref targetStructures, value); }
        }

        private int targetSelected;
        public int TargetSelected
        {
            get { return targetSelected; }
            set { SetProperty(ref targetSelected, value); }
        }

        private float vThresh;
        public float VThresh
        {
            get { return vThresh; }
            set { SetProperty(ref vThresh, value); }
        }

  
        private List<Spacing> validSpacings;
        public List<Spacing> ValidSpacings
        {
            get { return validSpacings; }
            set { SetProperty(ref validSpacings, value); }
        }

        private Spacing spacingSelected;
        public Spacing SpacingSelected
        {
            get { return spacingSelected; }
            set { SetProperty(ref spacingSelected, value); }    
        }

        private ScriptContext context;

        public SphereDialogViewModel(ScriptContext context)
        {
            // ctor
            this.context = context;
            
            // Set UI value defaults
            VThresh = 0;
            IsHex = true; // default to hex
            DeleteIndividual = false; // default to keeping individual structures
            XShift = 0;
            YShift = 0;


            // Set valid spacings based on CT img z resolution
            ValidSpacings = new List<Spacing>();
            var spacing = context.Image.ZRes;
            for (int i = 1; i<30; i++) {
                ValidSpacings.Add(new Spacing(spacing * i));
            }

            // Default to first value
            SpacingSelected = ValidSpacings.FirstOrDefault();

            // Target structures
            targetStructures = new List<string>();
            targetSelected = -1;
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
            for (int z = 0; z < context.Image.ZSize; ++z)
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

        private bool PreSpheres()
        {
            // Check if we are ready to make spheres
            if (!IsHex && !IsRect)
            {
                MessageBox.Show("No pattern selected. Returning.");
                return false;
            }

            // Check vol thresh for spheres
            if (VThresh > 100 || VThresh < 0)
            {
                MessageBox.Show("Volume threshold must be between 0 and 100");
                return false;
            }

            // Check target
            if (targetSelected == -1)
            {
                MessageBox.Show("Must have target selected, canceling operation.");
                return false;
            }

            if (Radius <= 0)
            {
                MessageBox.Show("Radius must be greater than zero.");
                return false;
            }

            if (SpacingSelected.Value < Radius * 2)
            {
                MessageBox.Show($"WARNING: Sphere center spacing is less than sphere diameter ({Radius * 2}) mm.");
            }

            return true;
        } 

        public void BuildSpheres(bool makeIndividual, bool alignGrid)
        {

            if (!PreSpheres())
            {
                return;
            }

            // Total lattice structure with all spheres
            Structure structMain = null;

            var target_name = targetStructures[targetSelected];
            var target = context.StructureSet.Structures.Where(x => x.Id == target_name).First();

            if (target == null)
            {
                MessageBox.Show($"Could not find target with Id: {target_name}");
                return;
            }

            // Generate a regular grid accross the dummie bounding box 
            var bounds = target.MeshGeometry.Bounds;

            // If alignGrid calculate z to snap to
            double z0 = bounds.Z;
            double zf = bounds.Z + bounds.SizeZ;
            if (alignGrid)
            {
                // Snap z to nearest z slice
                // where z slices = img.origin.z + (c * zres)
                // x, y, z --> dropdown all equal
                // z0 --> rounded to nearest grid slice
                var zSlices = new List<double>();
                var plane_idx = (bounds.Z - context.Image.Origin.z) / context.Image.ZRes;
                int plane_int = (int)Math.Round(plane_idx);

                z0 = context.Image.Origin.z + (plane_int * context.Image.ZRes);
                MessageBox.Show($"Original z | Snapped z = {bounds.Z} | {Math.Round(z0, 2)}");
            }

            // Get points that are not in the image
            List<VVector> grid = null;

            if (IsHex)
            {
                grid = BuildHexGrid(bounds.X + XShift, bounds.SizeX, bounds.Y + YShift, bounds.SizeY, z0, bounds.SizeZ);
                structMain = CreateStructure("LatticeHex", true, target.IsHighResolution);
            }
            else if (IsRect)
            {
                var xcoords = Arange(bounds.X + XShift, bounds.X + bounds.SizeX + XShift, SpacingSelected.Value);
                var ycoords = Arange(bounds.Y + XShift, bounds.Y + bounds.SizeY + YShift, SpacingSelected.Value);
                var zcoords = Arange(z0, zf, SpacingSelected.Value);

                grid = BuildGrid(xcoords, ycoords, zcoords);
                structMain = CreateStructure("LatticeRect", true, target.IsHighResolution);
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
            foreach (VVector ctr in grid)
            { 
                if (makeIndividual)
                {
                    // Create a new structure and build sphere on that
                    var singleId = $"Sphere_{sphere_count}";
                    var singleSphere = CreateStructure(singleId, false, structMain.IsHighResolution);
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
                    if(singleSphere.Volume <= volThresh || singleSphere.Volume == 0)
                    {
                        // Delete
                        //MessageBox.Show($"Deleted sphere based on volume threshold: {singleSphere.Volume} >= {volThresh}");
                        context.StructureSet.RemoveStructure(singleSphere);
                        continue;
                    }
                }

                // If here sphere is big enough

                structMain.SegmentVolume = structMain.SegmentVolume.Or(singleSphere);

                // If delete individual delete no
                if (deleteIndividual) { 
                    context.StructureSet.RemoveStructure(singleSphere);
                }


            }

            // And the main structure with target

            MessageBox.Show("Created Spheres");
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

        private Structure CreateStructure(string structName, bool showMessage, bool makeHiRes)
        {
            string msg = $"New structure ({structName}) created.";
            var prevStruct = context.StructureSet.Structures.Where(x => x.Id == structName).FirstOrDefault();
            if (prevStruct != null)
            {
                context.StructureSet.RemoveStructure(prevStruct);
                msg += " Old structure overwritten.";
            }

            var structure = context.StructureSet.AddStructure("PTV", structName);
            if (makeHiRes)
            {
                structure.ConvertToHighResolution();
                msg += " Converted to Hi-Res";
            }

            if (showMessage) { MessageBox.Show(msg); } 
            return structure;   
        }
        
        public void CreateLattice()
        {
            context.Patient.BeginModifications();
            BuildSpheres(true, true);
        }

    }
}
