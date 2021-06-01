using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chart2D
{
    public enum EDataPointShape
    {
        RECTANGLE4E = 1,  //边长为4pix的空心正方形
        CIRCLE2 = 2,     //直径为2pix的实心圆形
    }

    public class DataPointShape : Object
    {
        private EDataPointShape _shape;

        public DataPointShape(EDataPointShape shape)
        {
            _shape = shape;
        }

        public EDataPointShape GetDataPointShape { get { return _shape; } }
    }
}
