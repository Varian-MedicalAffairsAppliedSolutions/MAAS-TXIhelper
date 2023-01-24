using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml;
using System.Collections.ObjectModel;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Threading;
using System.Net;
using System.IO;

using System.Runtime.CompilerServices;
using System.Windows.Media;


namespace ViewModels
{

    public class BaseObject : INotifyPropertyChanged
    {
        private double x;
        public double X
        {
            get { return x; }
            set { x = value;
                NotifyPropertyChanged();
            }
        }

        private double y;
        public double Y
        {
            get { return y; }
            set
            {
                y = value;
                NotifyPropertyChanged();
            }
        }

        public double XTilted
        {
            get { return x + Math.Tan(tiltX / 180.0 * Math.PI) * zCoordStarX; }
            set
            {
                x = value - Math.Tan(tiltX / 180.0 * Math.PI) * zCoordStarX;
                NotifyPropertyChanged();
            }
        }


        public double YTilted
        {
            get { return y + Math.Tan(tiltY / 180.0 * Math.PI) *  zCoordStarY; }
            set
            {
                y = value - Math.Tan(tiltY / 180.0 * Math.PI) * zCoordStarY;
                NotifyPropertyChanged();
            }
        }

        public double tiltX;
        public double tiltY;
        public double zCoordStarX;
        public double zCoordStarY;



        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    } 

    public class Circle : BaseObject
    {
        double r;      
        public double R
        {
            get { return r; }
            set { 
                r = value;
                NotifyPropertyChanged(); }
        } //{  } 

        public int xGrid { get; set; }
        public int yGrid { get; set; }

        bool selected;
        public bool Selected
        {
            get { return selected; }
            set { selected = value; NotifyPropertyChanged(); }
        } //{  } 
    };

    public class Polygon : BaseObject
    {
        PointCollection points;
        public PointCollection Points
        {
            get { return points; }
            set { points = value; NotifyPropertyChanged(); }
        }
    };

    public class Notifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    };

    public class View1Model : Notifier
    {

        double radius;
        public double Radius
        {
            get { return radius; }
            set { radius = value; updateRadius(radius);  OnPropertyChanged("Radius"); }
        }

        double spacingX;
        public double SpacingX
        {
            get { return spacingX; }
            set { spacingX = value; create2Dgrid(); OnPropertyChanged("SpacingX"); }
        }

        double spacingY;
        public double SpacingY
        {
            get { return spacingY; }
            set { spacingY = value; create2Dgrid(); OnPropertyChanged("SpacingY"); }
        }


         ObservableCollection<BaseObject> drawingObjects;

         public ObservableCollection<BaseObject> DrawingObjects
        {
            get { return drawingObjects; }
            set { drawingObjects = value; OnPropertyChanged("DrawingObjects"); }
        }

         double bbXs;
         double bbYs;
         double bbXe;
         double bbYe;

         double centerX;
         double centerY;

         double offsetX;
         public double OffsetX
         {
             get { return offsetX; }
             set { offsetX = value; create2Dgrid(); OnPropertyChanged("OffsetX"); }
         }
         double offsetY;
         public double OffsetY
         {
             get { return offsetY; }
             set { offsetY = value; create2Dgrid(); OnPropertyChanged("OffsetY"); }
         }

         double tiltX;
         public double TiltX
         {
             get { return tiltX; }
             set { tiltX = value; create2Dgrid(); OnPropertyChanged("TiltX"); }
         }
         double tiltY;
         public double TiltY
         {
             get { return tiltY; }
             set { tiltY = value; create2Dgrid(); OnPropertyChanged("TiltY"); }
         }

         int zStart;
         public int ZStart
         {
             get { return zStart; }
             set { zStart = value; OnPropertyChanged("ZStart"); }

         }

         int zEnd;
         public int ZEnd
         {
             get { return zEnd; }
             set { zEnd = value; OnPropertyChanged("ZEnd"); }

         }

         int zShown;
         public int ZShown
         {
             get { return zShown; }
             set { zShown = value; updateContours(zShown); create2Dgrid();  OnPropertyChanged("ZShown"); }

         }

         Circle getOrCreateForInGrid(int xG, int yG, ObservableCollection<BaseObject> newDrawingObjects)
         {
             foreach (var s in newDrawingObjects)
            { 
                if (s is Circle)
                { 
                    var c = (Circle)s;
                    if (c.xGrid == xG && c.yGrid == yG)
                        return c;
                }
            }
             return new Circle { X = 0, Y = 0, R = xConv.LengthToCanvas(radius), Selected = true, xGrid = xG, yGrid = yG, tiltX = tiltX, tiltY = tiltY, zCoordStarX = xConv.LengthToCanvas((double)(2 * zShown - zStart - zEnd) * 0.5 * context.Image.ZRes), zCoordStarY = yConv.LengthToCanvas((double)(2 * zShown - zStart - zEnd) * 0.5 * context.Image.ZRes) };
        }

         void create2Dgrid()
         {

             ObservableCollection<BaseObject> newDrawingObjects = new ObservableCollection<BaseObject>(drawingObjects); //new ObservableCollection<BaseObject>();
             drawingObjects.Clear();
             drawingObjects.Add(newDrawingObjects.First());
             int startX = (int)(-Math.Floor((centerX + offsetX - bbXs + radius) / (spacingX))) - 2;
             int endX = (int)(1 + Math.Floor((bbXe + Radius - centerX - offsetX) / (spacingX))) + 2;
             int startY = (int)(-Math.Floor((centerY + offsetY - bbYs + radius) / (spacingY))) - 2;
             int endY = (int)(1 + Math.Floor((bbYe + Radius - centerY - offsetY) / (spacingY))) + 2;


             for (int x = startX; x < endX; ++x)
                 for (int y = startY; y < endY; ++y)
                 {
                     Circle curCircle = getOrCreateForInGrid(x, y, newDrawingObjects);
                     curCircle.X = xConv.PointToCanvas(centerX + offsetX + (double)(x) * (spacingX) - radius);
                     curCircle.Y = yConv.PointToCanvas(centerY + offsetY + (double)(y) * (spacingY) - radius);
                     curCircle.tiltX = TiltX;
                     curCircle.tiltY = tiltY;
                     curCircle.zCoordStarX = xConv.LengthToCanvas((double)(2 * zShown - zStart - zEnd) * 0.5 * context.Image.ZRes);
                     curCircle.zCoordStarY = yConv.LengthToCanvas((double)(2 * zShown - zStart - zEnd) * 0.5 * context.Image.ZRes);

                     drawingObjects.Add(curCircle);
                 }
             updateRadius(radius);

        }
        void updateRadius(double radius)
         {
             foreach (var s in drawingObjects)
             {
                 if (s is Circle)
                 {
                     var c = (Circle)s;
                     double oldR = c.R;
                     c.R = xConv.LengthToCanvas(radius);
                     c.X += oldR - c.R;
                     c.Y += oldR - c.R;
                 }
             }
         }

        double canvasHeight;
        public double CanvasHeight
        {
            get { return canvasHeight; }
            set { canvasHeight = value; OnPropertyChanged("CanvasHeight"); }
        }
        double canvasWidth;
        public double CanvasWidth
        {
            get { return canvasWidth; }
            set { canvasWidth = value; OnPropertyChanged("CanvasWidth"); }
        }

        public class coordConv   
         {

            public double PointToCanvas(double coordinate)
            {
                return (coordinate + constant) * multiplier;
            }

            public double PointFromCanvas(double canvasPoint)
            {
                return canvasPoint / multiplier -constant;
            }

            public double LengthToCanvas(double length)
            {
                return length * multiplier;
            }

            public coordConv(double minVal, double maxVal, double canvasWidth)
            {
                constant = -minVal;
                multiplier = canvasWidth / (maxVal - minVal);
            }

            double multiplier;
            double constant;

         }

        public coordConv xConv;
        public coordConv yConv;

        public ScriptContext context;

        public Structure target;

        public void updateCanvasScaling()
        {
            double fullWidth = bbXe - bbXs;
            double fullHeight = bbYe - bbYs;

            double largerScaler = fullWidth / canvasWidth > fullHeight / canvasHeight ? fullWidth / canvasWidth : fullHeight / canvasHeight;
            double widthMargin = 0.5 * (largerScaler - fullWidth / canvasWidth);
            double heightMargin = 0.5 * (largerScaler - fullHeight / canvasHeight);

            xConv = new coordConv(bbXs - widthMargin * canvasWidth, bbXe + widthMargin * canvasWidth, canvasWidth);

            yConv = new coordConv(bbYs - heightMargin * canvasHeight, bbYe + heightMargin * canvasHeight, canvasHeight);
        }

        public void selectTarget()
        {
            if (targetSelected == -1)
            {
                target = null;
                drawingObjects = new ObservableCollection<BaseObject>();
                return;
            }

            string targetName = targetStructures.ElementAt(TargetSelected);

            var structures = context.StructureSet.Structures;
            foreach(var s in structures)
            {
                if (s.Id == targetName)
                {
                    target = s;
                    return;
                }
            }
            target = null;
            drawingObjects = new ObservableCollection<BaseObject>(); //clear objects
            return;
        }



        private void updateContours(int z)
        {
            var conts = target.GetContoursOnImagePlane(z);

            PointCollection points = new PointCollection();
            foreach (var contour in conts)
                foreach (var p in contour)
                {
                    points.Add(new Point(xConv.PointToCanvas(p.x), yConv.PointToCanvas(p.y)));
                }
            //check if we already have objects
            Polygon polygon;
            if (drawingObjects.Count() > 0)
            {
                polygon = (Polygon)drawingObjects.ElementAt(0);
                polygon.Points = points;
            }
            else
            {
                polygon = new Polygon { Points=points };
                drawingObjects.Add(polygon);
            }
        }

        public void scanTarget()
        {
            int zCount = context.Image.ZSize;
            int zStartTemp = zCount;
            int zEndTemp = 0;

            double highVal = 1.0e+10;
            bbXs = bbYs  = highVal;
            bbXe = bbYe  = -highVal;

            for (int z = 0; z < zCount; ++z)
            {
                var contours =  target.GetContoursOnImagePlane(z);
                if (contours.Count() > 0) 
                { //layer has some contours
                    //TODO: handle cases when there are multiple contours
                    zStartTemp = zStartTemp > z ? z : zStartTemp;
                    zEndTemp = z + 1;

                    //bb
                    foreach (var contour in contours)
                        foreach (var p in contour)
                        {
                            if (bbXs > p.x) bbXs = p.x;
                            if (bbYs > p.y) bbYs = p.y;
                            
                            if (bbXe < p.x) bbXe = p.x;
                            if (bbYe < p.y) bbYe = p.y;
                           
                        }
                }

            }

            updateCanvasScaling();

            //prepareFirst Layer
            ZStart = zStartTemp;
            ZEnd = zEndTemp;
            int zMid = (zEnd - zStart) / 2 + zStart;
            var conts = target.GetContoursOnImagePlane(zMid);
            while (conts.Count() == 0)
            {
                ++zMid;
                conts = target.GetContoursOnImagePlane(zMid);
            }
            ZShown = zMid;
            //updateContours(zMid);

            create2Dgrid();

            

        }

        public void updateFullState()
        {
            selectTarget();
            if (target != null)
            {
                scanTarget();
                create2Dgrid();
            }

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
                updateFullState();
                OnPropertyChanged("TargetSelected");
            }
        }


        //creating the structure

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

        void addContoursToStructure(VVector[] gridPoints, Image image, ref Structure gridStructure, double radius, int zLayer)
        {
            double zCoord = (double)(zLayer) * image.ZRes + image.Origin.z;

            const int contourSegmentCount = 32;

            foreach (var contourCenter in gridPoints)
            {
                VVector center = new VVector(contourCenter.x, contourCenter.y, zCoord);
                var contour = CreateContour(contourCenter, radius, contourSegmentCount);
                gridStructure.AddContourOnImagePlane(contour, zLayer);
            }
        }

        // CR This seems to be the method called to create the grid structure
        void createGridStructure( ref Structure gridStructure)
        {
            if (target == null) return;

            double zCenter = (double)(zEnd + zStart) / 2.0 * context.Image.ZRes + context.Image.Origin.z;

            for (int z = zStart; z < zEnd; ++z)
            {
                double zCoord = (double)(z) * context.Image.ZRes + context.Image.Origin.z;
                double tiltXOffset = (zCoord - zCenter) * Math.Tan(tiltX / 180.0 * Math.PI);
                double tiltYOffset = (zCoord - zCenter) * Math.Tan(tiltY / 180.0 * Math.PI);
                const int contourSegmentCount = 32;
                
                foreach (var s in drawingObjects)
                {
                    if (s is Circle)
                    {
                        var c = (Circle)s;
                        if (c.Selected == false) continue;
                        VVector center = new VVector(xConv.PointFromCanvas(c.X) + radius + tiltXOffset, 
                            yConv.PointFromCanvas(c.Y) + radius + tiltYOffset, zCoord);
                        var contour = CreateContour(center, radius, contourSegmentCount);
                        gridStructure.AddContourOnImagePlane(contour, z);
                    }
                }
            }

            gridStructure.SegmentVolume = gridStructure.And(target);

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
            createGridStructure(ref grid);

            
        }

        public void CreateGridAndInverse()
        {
            //Start prepare the patient
            context.Patient.BeginModifications();
            var grid = context.StructureSet.AddStructure("PTV", "Grid");
            createGridStructure(ref grid);
            var inverse = context.StructureSet.AddStructure("PTV", "GridInv");
            inverse.SegmentVolume = target.Sub(grid);

        }


        public View1Model(ScriptContext currentContext)
        {

            context = currentContext;

            //ui 'consts'
            canvasHeight = 300;
            canvasWidth = 400;

            //ui defaults
            radius = 10;
            spacingX = 30;
            spacingY = 30;
            drawingObjects = new ObservableCollection<BaseObject>();
            offsetX = 0;
            offsetY = 0; 

            //hidden defaults
            bbXs = 50;
            bbXe = 250;
            bbYs = 50;
            bbYe = 200;

            //plan isocenter
            var firstBeam = context.PlanSetup.Beams.First();
            centerX = firstBeam.IsocenterPosition.x;
            centerY = firstBeam.IsocenterPosition.y;

            //target structures
            targetStructures = new List<string>();
            targetSelected = -1;
            //plan target
            string planTargetId = null;
            //only for 15.x or later
            //List<ProtocolPhasePrescription> p = new List<ProtocolPhasePrescription>();
            //List<ProtocolPhaseMeasure> m = new List<ProtocolPhaseMeasure>();
            //context.PlanSetup.GetProtocolPrescriptionsAndMeasures(ref p, ref m);
            
            //if (p.Count() > 0) planTargetId = p.First().StructureId;
            //..plan target selection ends
            
            foreach(var i in context.StructureSet.Structures)
            {
                if (i.DicomType != "PTV") continue;
                targetStructures.Add(i.Id);
                if (planTargetId == null) continue;
                if (i.Id == planTargetId) targetSelected = targetStructures.Count() - 1;
            }

            updateFullState();



            //drawingObjects.Add(new Circle { X = 50, Y = 50, R = 15, Selected = true });
            //drawingObjects.Add(new Circle { X = 100, Y = 50, R = 15, Selected = false });
            //drawingObjects.Add(new Line { StartX = 70,StartY = 50, EndX = 80, EndY = 50  });



        }
                  

    }



}
