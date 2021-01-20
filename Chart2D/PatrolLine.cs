using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Chart2D
{
    public class PatrolLine : Object, ICloneable
    {
        public EPatrolLineType Type { get; set; }
        public float Value { get; set; }

        public Color LineColor { get; set; }

        public PatrolLine()
        {
            Type = EPatrolLineType.Y;
            Value = 0.0f;
            LineColor = Color.Red;
        }

        public object Clone()
        {
            PatrolLine obj = new PatrolLine();
            obj.Type = Type;
            obj.Value = Value;
            obj.LineColor = LineColor;
            return obj;
        }
    }
}
