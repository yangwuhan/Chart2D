using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chart2D
{
    internal class ChartCursor1V : Object
    {
        public int Pt_x { get; set; }
        public int Pt_y { get; set; }
        public int YIndex { get; set; }        
        public float YValue { get; set; }
        public int Pt_count { get; set; }
        public float Y_max { get; set; }
        public float Y_min { get; set; }        
    }
}
