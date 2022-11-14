using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace GridBlockCreator
{
    public class SphereDialogViewModel : Notifier
    {
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
                updateFullState();
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

            updateFullState();

        }

        void CRTest(ref Structure gridStructure, float R)
        {
            if (target == null) return;

            double zCenter = (double)(zEnd + zStart) / 2.0 * context.Image.ZRes + context.Image.Origin.z;
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
                var r_z = Math.Pow(R, 2) - Math.Pow(z_diff, 2);

                // Just make one sphere at target center for now
                var contour = CreateContour(gridStructure.CenterPoint, 2, 64);
                gridStructure.AddContourOnImagePlane(contour, z);

            }

            gridStructure.SegmentVolume = gridStructure.SegmentVolume.And(target);
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

        public void updateFullState()
        {
            //selectTarget();
            //if (target != null)
            //{
                //scanTarget();
                //create2Dgrid();
            //}
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
            var grid = context.StructureSet.AddStructure("PTV", "Grid");
            CRTest(ref grid, 1);


        }

        public void CreateGridAndInverse()
        {
            //Start prepare the patient
            /*context.Patient.BeginModifications();
            var grid = context.StructureSet.AddStructure("PTV", "Grid");
            createGridStructure(ref grid);
            var inverse = context.StructureSet.AddStructure("PTV", "GridInv");
            inverse.SegmentVolume = target.Sub(grid);*/
            return;

        }


    }
}
