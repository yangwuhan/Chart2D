using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Chart2D
{
    [Serializable] 
    public class ChartCurve2V : ChartCurve1V
    {
        #region 数据和属性成员

        /* X轴数据
         */
        public List<float> XAxisData { get; set; }       
 
        /* 游标
         */
        [NonSerialized]
        internal Dictionary<int, Dictionary<int, ChartCursor2V>> _ChartCursors2V = new Dictionary<int, Dictionary<int, ChartCursor2V>>();

        /* X轴的名称
        */
        public string XAxisName { get; set; }

        /* X轴的数值单位
         */
        public string XAxisUnitName { get; set; }

        /* X轴，刻度值格式化对象，和数值格式化对象
        */
        public DataFormat XAxisKdFormat { get; set; }
        public DataFormat XAxisValueFormat { get; set; }

        /* X轴最大值和最小值
        */
        internal float XAxisMaxValueUsed;
        internal float XAxisMinValueUsed;

        /* X轴是否自动调整
        */
        public bool XAxisIsAuto { get; set; }

        /* X轴手动调整时，最大值
         */
        public float XAxisMaxValue { get; set; }

        /* X轴手动调整时，最小值
         */
        public float XAxisMinValue { get; set; }

        #endregion

        #region 函数

        /* 构造函数
         */
        public ChartCurve2V()
        {
            XAxisData = new List<float>();
            XAxisName = "";
            XAxisUnitName = "";
            XAxisIsAuto = true;
            XAxisMaxValueUsed = XAxisMaxValue = 20.0f;
            XAxisMinValueUsed = XAxisMinValue = -20.0f;
            XAxisKdFormat = new DataFormat(EDataFormat.FORMAT_STRING, "{0}");
            XAxisValueFormat = new DataFormat(EDataFormat.FORMAT_STRING, "{0}");     
        }

        /* 获取X轴数据最大绝对值
         */
        public float GetXAxisDataMaxAbsValue(float bilichi_start, float bilichi_stop)
        {
            float f_min, f_max, f_max_abs;
            if (GetDataMaxAbsValue(XAxisData, bilichi_start, bilichi_stop, out f_max_abs, out f_max, out f_min))
                return f_max_abs;
            else
                return 0;
        }

        /* 获取X轴数据最大值和最小值
         */
        public bool GetXAxisDataMaxMinValue(float bilichi_start, float bilichi_stop, out float f_max, out float f_min)
        {
            float f_max_abs;
            return GetDataMaxAbsValue(XAxisData, bilichi_start, bilichi_stop, out f_max_abs, out f_max, out f_min);
        }

        /* 获取X轴数据在指定范围内的最大值和最小值
         */
        public bool GetXAxisDataMaxMinValueInSpan(int start_index, int stop_index, out float f_max, out float f_min)
        {
            f_max = f_min = 0.0f;
            List<float> data = XAxisData;
            bool first = true;
            for (int i = start_index; i <= stop_index && i < data.Count; ++i)
            {
                if (first)
                {
                    first = false;
                    f_max = data[i];
                    f_min = data[i];
                }
                else
                {
                    if (data[i] > f_max)
                        f_max = data[i];
                    else if (data[i] < f_min)
                        f_min = data[i];
                }
            }
            return !first;
        }

        ///* 根据缩放比例和数据总长度值，获取X轴和Y轴数据的起始间隔累加值和截止间隔累加值
        // */
        //public static bool GetXYAxisStartStopDeltaAmountByBilichi(float start_bili, float stop_bili, float max_delta_amount, out float start_delta_amount, out float stop_delta_amount)
        //{
        //    start_delta_amount = start_bili * max_delta_amount;
        //    stop_delta_amount = stop_bili * max_delta_amount;
        //    if (start_delta_amount < 0) start_delta_amount = 0.0f;
        //    else if (start_delta_amount >= max_delta_amount) start_delta_amount = max_delta_amount;
        //    if (stop_delta_amount < 0) stop_delta_amount = 0.0f;
        //    else if (stop_delta_amount >= max_delta_amount) stop_delta_amount = max_delta_amount;
        //    if (stop_delta_amount < start_delta_amount) stop_delta_amount = start_delta_amount;
        //    return true;
        //}

        #endregion
    }
}
