using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulationComplexity.Models
{
    public class ListItem
    {
        // Contains 
        // Field,avgArea,stdArea,ALPO,stdALPO,BI,BA
        public string Field{ get; set; }
        public double avgArea { get; set; }
        public double stdArea { get; set; }
        public double ALPO { get; set; }    
        public double stALPO { get; set; }
        public double BI { get; set; }
        public double BA { get; set; }  

        public ListItem(string field, double[] input)
        {
            Field = field;
            avgArea = input[0];
            stdArea = input[1];
            ALPO = input[2];
            stALPO = input[3];
            BI = input[4];
            BA = input[5];
        }
    }
}
