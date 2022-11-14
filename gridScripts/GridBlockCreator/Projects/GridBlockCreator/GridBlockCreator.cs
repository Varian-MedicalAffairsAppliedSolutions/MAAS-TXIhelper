using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

// TODO: Uncomment the following line if the script requires write access.
//15.x or later:
[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
  public class Script
  {
    /*
    public Script()
    {

    }

    

    int getImagePlaneFromZCoordinate(Image image, double zCoord)
    {
        return (int)Math.Round((zCoord - image.Origin.z) / image.ZRes);
    }*/

    VVector[]  CreateContour(VVector center, double radius, int nOfPoints)
    {
        VVector[] contour = new VVector[nOfPoints + 1];
        double angleIncrement = Math.PI * 2.0 / Convert.ToDouble(nOfPoints);
        for(int i = 0; i < nOfPoints; ++i)
        {
            double angle = Convert.ToDouble(i) * angleIncrement;
            double xDelta = radius * Math.Cos(angle);
            double yDelta = radius * Math.Sin(angle);
            VVector delta = new VVector(xDelta, yDelta, 0.0);
            contour[i] = center + delta;
        }
        contour[nOfPoints] = contour[0]; // Last pt is same as first

        return contour;
    }

    void addContoursToStructure(VVector[] gridPoints, Image image, ref Structure gridStructure, double radius, int zLayer)
    {
        double zCoord = (double)(zLayer) * image.ZRes + image.Origin.z;

        const int contourSegmentCount = 8;

        foreach(var contourCenter in gridPoints)
        {
            //VVector center = new VVector(contourCenter.x, contourCenter.y, zCoord);
            var contour = CreateContour(contourCenter, radius, contourSegmentCount);
            gridStructure.AddContourOnImagePlane(contour, zLayer);
        }
    }

    /*
    void createGridStructure(VVector[] gridPoints, Image image, Structure ptv, ref Structure gridStructure, double radius)
    {
        for(int z = 0; z < image.ZSize; ++z)
            addContoursToStructure(gridPoints, image,  ref gridStructure, radius, z);
        gridStructure.And(ptv);
        
    }*/

    //VVector [] getBoundingBox(Structure ptv, int nOfLayers)
    //{
    //    VVector[] retval = new VVector[2];
    //    double highVal = 1.0e+10;
    //    retval[0].x = retval[0].y =retval[0].z = highVal;
    //    retval[1].x = retval[1].y =retval[1].z = -highVal;
    //    for(int z = 0; z < nOfLayers; ++z){
    //        var contours = ptv.GetContoursOnImagePlane(z);
    //        foreach (var contour in contours)
    //            foreach(var p in contour)
    //            {
    //                if(retval[0].x > p.x) retval[0].x = p.x;
    //                if(retval[0].y > p.y) retval[0].y = p.y;
    //                if(retval[0].z > p.z) retval[0].z = p.z;
    //                if(retval[1].x < p.x) retval[1].x = p.x;
    //                if(retval[1].y < p.y) retval[1].y = p.y;
    //                if(retval[1].z < p.z) retval[1].z = p.z;
    //            }
    //    }

    //    return retval;

    //}

    //void create2Dgrid(Structure ptv, Image image, double xDelta, double yDelta, double xCenter, double yCenter, double radius)
    //{
    //    var bb = getBoundingBox(ptv, image.ZSize);

    //    int startX = (int)(-Math.Floor((xCenter - bb[0].x + radius) / xDelta)); 
    //    int endX = (int)(1 + Math.Floor((bb[1].x + radius - xCenter) / xDelta)); 
    //    int startY = (int)(-Math.Floor((yCenter - bb[0].y + radius) / yDelta)); 
    //    int endY = (int)(1 + Math.Floor((bb[1].y + radius - yCenter) / yDelta)); 

    //    VVector[] retval = new VVector[(endX - startX)*(endY - startY)];
    //    int ind = 0;
    //    for(int x = startX; xCenter < endX; ++x)
    //        for(int y = startY; yCenter < endY; ++y)
    //        {
    //            retval[ind].x = xCenter + (double)(x) *xDelta;
    //            retval[ind].y = yCenter + (double)(y) * yDelta;
    //        }
    //}







    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
    {

        if (context.Patient == null || context.PlanSetup == null)
        {
            MessageBox.Show("No active plan selected - exiting.");
            return;
        }

        /*
        GridBlockCreator.GridDialog mainWindow = new GridBlockCreator.GridDialog(context);
        mainWindow.ShowDialog();*/



    }
  }
}
