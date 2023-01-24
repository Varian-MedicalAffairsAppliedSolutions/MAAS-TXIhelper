using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using Views;

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


    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Execute(ScriptContext context)
    {

        if (context.Patient == null || context.PlanSetup == null)
        {
            MessageBox.Show("No active plan selected - exiting.");
            return;
        }

        // Check exp date
        DateTime exp = Template2.Properties.Settings.Default.ExpDate;
        if (exp < DateTime.Now)
        {
            MessageBox.Show("Application has expired");
            return;
        }

        // Display opening msg
        string msg = "The current MAAS-SFRThelper application is provided AS IS as a non-clinical, research only tool in evaluation only. The current " +
        "application will only be available until {exp.Date} after which the application will be unavailable. " +
        "By Clicking 'Yes' you agree that this application will be evaluated and not utilized in providing planning decision support\n\n" +
        "Newer builds with future expiration dates can be found here: https://github.com/Varian-Innovation-Center/MAAS-SFRThelper\n\n" +
        "See the FAQ for more information on how to remove this pop-up and expiration";
        var res = MessageBox.Show(msg, "Agreement  ", MessageBoxButton.YesNo);
       
        if (res == MessageBoxResult.No) {
            return;
        }

        var mainWindow = new MainWindow(context);
        
        mainWindow.ShowDialog();



    }
  }
}
