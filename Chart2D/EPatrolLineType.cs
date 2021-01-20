using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chart2D
{
    public enum EPatrolLineType
    {
        X = 1, //X轴向警戒线
        Y = 2, //Y轴向警戒线
    }

    public class PatrolLineType : Object
    {
        private EPatrolLineType _type;

        public PatrolLineType(EPatrolLineType type)
        {
            _type = type;
        }

        public EPatrolLineType GetPatrolLineType { get { return _type; } }
    }
}
