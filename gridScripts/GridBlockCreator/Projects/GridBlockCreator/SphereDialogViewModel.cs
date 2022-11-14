using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace GridBlockCreator
{
    public class SphereDialogViewModel : Notifier
    {

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
