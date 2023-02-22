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
using Prism.Mvvm;
using ModulationComplexity.Models;
using Prism.Commands;
using System.Numerics;
using JR.Utils.GUI.Forms;

// TODO
// Button that shows formula

namespace ViewModels
{

    public class View1Model : BindableBase
    {
        private ScriptContext context;

        public DelegateCommand AboutCmd { get; set; }

        public DelegateCommand SaveCmd { get; set; }  
       // public DelegateCommand ExeCmd { get; set; }

        private string ResultStr;

        public ObservableCollection<ListItem> ListItems { get; set; }

        internal ComplexityModel ComplexityModel { get; }



        public View1Model(ScriptContext currentContext)
        {

            this.context = currentContext;
            this.ComplexityModel = new ComplexityModel(context);
            var retval = this.ComplexityModel.Execute();
            this.ListItems = retval.Item1;
            this.ResultStr = retval.Item2;

            //ExeCmd = new DelegateCommand(OnExe);
            SaveCmd = new DelegateCommand(OnSave);
            AboutCmd = new DelegateCommand(OnAbout);
        }

        private void OnAbout()
        {
            // Read XML Text

            string txt = @"
Plan Complexity Analyser
Authors: 
Esa Kuusela, Varian Medical Systems.
Filippo Cozzi. Liceo Scientifico A. Sereni, Luino, Italy

The script executes an analysis of the plan complexity for IMRT and VMAT on a field per field basis and extracts some quantitative metrics from the MLC sequences.
NOTE:  the tool currently works only for c-arm linacs (e.g. TrueBeam) and it is not intended for dual layer MLC based machines like Halcyon.  It will be upgraded soon.

Average aperture area (in mm2).  It is the MLC defined open field averaged over all the control points in the field.  It is provided with its standard deviation.

Average aperture per leaf couple (in mm), ALPO:  it is the per-leaf-pair aperture averaged per each control point and over all the control points in the field. It is provided with its standard deviation.

Detailed descriptions and discussion of BA, BI and BM can be found in Du et al [1] and a more general overview in the chapter 8 of [2].

References:
[1] Du W, Cho S H, Zhang X, Hoffman K E and Kudchadker R J 2014 Quantification of beam complexity in intensity-modulated radiation therapy treatment plans Med. Phys. 41 021716

[2]. Das. I. , Sanfilippo N, Fogliata A, Cozzi L.    Intensity modulated radiation therapy. A clinical overview.    IOP Publishing, Bristol, UK, 2020";

            
            FlexibleMessageBox.Show(txt, "About Modulation-Complexity Plugin");
        }

        private void OnSave()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                var result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var outputpath = dialog.SelectedPath;
                    Directory.CreateDirectory(outputpath);

                    var savepath = Path.Combine(outputpath, string.Format("{0}-complexity.csv", context.Patient.Name));
                    File.WriteAllText(savepath, ResultStr);

                    // MessageBox.Show(msg);
                    MessageBox.Show(string.Format("CSV saved in {0}", savepath));

                }

            }

        }

    }
}
