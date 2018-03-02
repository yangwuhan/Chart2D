using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Chart2D
{
    [Serializable] 
    public class ChartCurve1V : Object
    {
        #region 数据和属性成员

        /* 曲线颜色
         */
        private Color _color;
        public Color Color 
        {
            get { return _color; }
            set
            {
                _color = value;
                _brush = new SolidBrush(_color);
                _pen = new Pen(_color);
            }
        }

        /* 曲线的画笔
         */
        [NonSerialized]
        private Pen _pen;
        public Pen Pen { get { return _pen; } }

        /* 曲线的刷子
         */
        [NonSerialized]
        private Brush _brush;
        public Brush Brush { get { return _brush; } }

        /* 数据点之间的间隔
        */
        private float _Delta;
        public float Delta
        {
            get { return _Delta; }
            set
            {
                if (value <= 0)
                    _Delta = 1;
                else
                    _Delta = value;
            }
        }

        /* Y轴的名称
         */
        public string YAxisName { get; set; }

        /* Y轴的数值单位
        */
        public string YAxisUnitName { get; set; }

        /* Y轴是否自动调整
         */
        public bool YAxisIsAuto { get; set; }

        /* Y轴手动调整时，最大值
         */
        public float YAxisMaxValue { get; set; }

        /* Y轴手动调整时，最小值
         */
        public float YAxisMinValue { get; set; }

        /* Y轴数据
         */
        [NonSerialized]
        protected List<float> _YAxisData = new List<float>();
        public List<float> YAxisData 
        {
            get { return _YAxisData; }
            set
            {
                if (value == null)
                    return;
                _YAxisData = value;
            }
        }

        /* Y轴，刻度值格式化对象，和数值格式化对象
         */
        public DataFormat YAxisKdFormat { get; set; }
        public DataFormat YAxisValueFormat { get; set; }
        
        /* Y轴最大值和最小值
        */
        internal float YAxisMaxValueUsed { get; set; }
        internal float YAxisMinValueUsed { get; set; }

        /* 游标
         */
        [NonSerialized]
        internal Dictionary<int, ChartCursor1V> _ChartCursors1V = new Dictionary<int, ChartCursor1V>();

        #endregion

        #region 函数

        /* 构造函数
         */
        public ChartCurve1V()
        {
            Color = Color.Gray;
            Delta = 1.0f;
            YAxisName = "";
            YAxisUnitName = "";
            YAxisIsAuto = true;
            YAxisMaxValueUsed = YAxisMaxValue = 20.0f;
            YAxisMinValueUsed = YAxisMinValue = -20.0f;
            YAxisData = new List<float>();
            YAxisKdFormat = new DataFormat(EDataFormat.FORMAT_STRING, "{0}");
            YAxisValueFormat = new DataFormat(EDataFormat.FORMAT_STRING, "{0}");                    
        }

        /* 获取数据最大绝对值、最大值、最小值
         */
        public static bool GetDataMaxAbsValue(List<float> data, float bilichi_start, float bilichi_stop, 
                                                out float f_max_abs, out float f_max, out float f_min)
        {
            f_max_abs = f_max = f_min = 0.0f;
            if (data.Count < 2)
                return false;
            int data_index_s = (int)Math.Floor(bilichi_start * (float)(data.Count - 1));
            if (data_index_s < 0) data_index_s = 0;
            else if (data_index_s >= data.Count - 1) data_index_s = data.Count - 1;
            int data_index_e = (int)Math.Ceiling(bilichi_stop * (float)(data.Count - 1));
            if (data_index_e < 0) data_index_e = 0;
            else if (data_index_e >= data.Count - 1) data_index_e = data.Count - 1;
            if (data_index_e < data_index_s) data_index_e = data_index_s;
            int data_count = data_index_e - data_index_s + 1;
            if (data_count <= 0)
                return false;
            float v_max = data[data_index_s];
            float v_min = data[data_index_s];
            for (int i = data_index_s + 1; i <= data_index_e; ++i)
            {
                if (data[i] > v_max)
                    v_max = data[i];
                else if (data[i] < v_min)
                    v_min = data[i];
            }
            f_max = v_max;
            f_min = v_min;
            v_max = Math.Abs(v_max);
            v_min = Math.Abs(v_min);
            if (v_max < v_min)
                v_max = v_min;
            f_max_abs = v_max;
            return true;
        }
        /* 获取Y轴数据最大绝对值
         */
        public float GetYAxisDataMaxAbsValue(float bilichi_start, float bilichi_stop)
        {
            float f_min, f_max, f_max_abs;
            if (GetDataMaxAbsValue(YAxisData, bilichi_start, bilichi_stop, out f_max_abs, out f_max, out f_min))
                return f_max_abs;
            else
                return 0;
        }
        
        /* 获取Y轴数据最大值和最小值
         */
        public bool GetYAxisDataMaxMinValue(float bilichi_start, float bilichi_stop, out float f_max, out float f_min)
        {
            float f_max_abs;
            return GetDataMaxAbsValue(YAxisData, bilichi_start, bilichi_stop, out f_max_abs, out f_max, out f_min);
        }

        /* 按缩放比例，获取Y轴数据序列的起止索引
         */ 
        public bool GetYAxisStartStopIndexByBilichi(float start, float stop, out int start_index, out int stop_index)
        {
            start_index = stop_index = 0;
            List<float> data = YAxisData;
            int data_index_s = (int)Math.Floor(start * (float)(data.Count - 1));
            if (data_index_s < 0) data_index_s = 0;
            else if (data_index_s >= data.Count - 1) data_index_s = data.Count - 1;
            int data_index_e = (int)Math.Ceiling(stop * (float)(data.Count - 1));
            if (data_index_e < 0) data_index_e = 0;
            else if (data_index_e >= data.Count - 1) data_index_e = data.Count - 1;
            if (data_index_e < data_index_s) data_index_e = data_index_s;
            start_index = data_index_s;
            stop_index = data_index_e;
            return true;
        }

        /* 根据缩放比例和数据总长度值，获取Y轴数据的起始间隔累加值和截止间隔累加值
         */
        public static bool GetYAxisStartStopDeltaAmountByBilichi(float start_bili, float stop_bili, float max_delta_amount, out float start_delta_amount, out float stop_delta_amount)
        {
            start_delta_amount = start_bili * max_delta_amount;
            stop_delta_amount = stop_bili * max_delta_amount;
            if (start_delta_amount < 0) start_delta_amount = 0.0f;
            else if (start_delta_amount >= max_delta_amount) start_delta_amount = max_delta_amount;
            if (stop_delta_amount < 0) stop_delta_amount = 0.0f;
            else if (stop_delta_amount >= max_delta_amount) stop_delta_amount = max_delta_amount;
            if (stop_delta_amount < start_delta_amount) stop_delta_amount = start_delta_amount;
            return true;
        }

        /* 根据显示范围的Y轴数据索引对用的DeltaAmount浮点数计算出曲线的实际起始索引和截止索引
         */ 
        public bool GetYAxisStartStopIndex(float f_start, float f_stop, out int start_index, out int stop_index)
        {
            start_index = (int)Math.Ceiling(f_start / Delta);
            stop_index = (int)Math.Floor(f_stop / Delta);
            return true;
        }

        /* 获取Y轴数据在指定范围内的最大值和最小值
         */
        public bool GetYAxisDataMaxMinValueInSpan(int start_index, int stop_index, out float f_max, out float f_min)
        {
            f_max = f_min = 0.0f;
            List<float> data = YAxisData;
            bool first = true;
            for(int i = start_index; i <= stop_index && i < data.Count; ++i)
            {
                if(first)
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

        /* 坐标轴自动调整时，坐标轴范围
         */
        public static Int64 GetAxisRange(float vv_max)
        {
            Int64 range = 2;
            float v_max = vv_max;
            Int64 i_max = (Int64)Math.Ceiling(v_max * 1E6);
            if (i_max <= 2)
            {
                range = 2;
            }
            else if (i_max <= 5)
            {
                range = 5;
            }
            else if (i_max <= 10)
            {
                range = 10;
            }
            else if (i_max <= 20)
            {
                range = 20;
            }
            else if (i_max <= 50)
            {
                range = 50;
            }
            else if (i_max <= 100)
            {
                range = 100;
            }
            else if (i_max <= 200)
            {
                range = 200;
            }
            else if (i_max <= 500)
            {
                range = 500;
            }
            else if (i_max <= 1000)
            {
                range = 1000;
            }
            else if (i_max <= 2000)
            {
                range = 2000;
            }
            else if (i_max <= 5000)
            {
                range = 5000;
            }
            else if (i_max <= 10000)
            {
                range = 10000;
            }
            else if (i_max <= 20000)
            {
                range = 20000;
            }
            else if (i_max <= 50000)
            {
                range = 50000;
            }
            else if (i_max <= 100000)
            {
                range = 100000;
            }
            else if (i_max <= 200000)
            {
                range = 200000;
            }
            else if (i_max <= 500000)
            {
                range = 500000;
            }
            else if (i_max <= 1000000)
            {
                range = 1000000;
            }
            else if (i_max <= 2000000)
            {
                range = 2000000;
            }
            else if (i_max <= 5000000)
            {
                range = 5000000;
            }
            else if (i_max <= 10000000)
            {
                range = 10000000;
            }
            else if (i_max <= 20000000)
            {
                range = 20000000;
            }
            else if (i_max <= 50000000)
            {
                range = 50000000;
            }
            else if (i_max <= 100000000)
            {
                range = 100000000;
            }
            else if (i_max <= 200000000)
            {
                range = 200000000;
            }
            else if (i_max <= 500000000)
            {
                range = 500000000;
            }
            else if (i_max <= 1000000000)
            {
                range = 1000000000;
            }
            else if (i_max <= 2000000000)
            {
                range = 2000000000;
            }
            else if (i_max <= 5000000000)
            {
                range = 5000000000;
            }
            else if (i_max <= 10000000000)
            {
                range = 10000000000;
            }
            else if (i_max <= 20000000000)
            {
                range = 20000000000;
            }
            else if (i_max <= 50000000000)
            {
                range = 50000000000;
            }
            else if (i_max <= 100000000000)
            {
                range = 100000000000;
            }
            else if (i_max <= 200000000000)
            {
                range = 200000000000;
            }
            else if (i_max <= 500000000000)
            {
                range = 500000000000;
            }
            else if (i_max <= 1000000000000)
            {
                range = 1000000000000;
            }
            else if (i_max <= 2000000000000)
            {
                range = 2000000000000;
            }
            else if (i_max <= 5000000000000)
            {
                range = 5000000000000;
            }
            else if (i_max <= 10000000000000)
            {
                range = 10000000000000;
            }
            else if (i_max <= 20000000000000)
            {
                range = 20000000000000;
            }
            else if (i_max <= 50000000000000)
            {
                range = 50000000000000;
            }
            else if (i_max <= 100000000000000)
            {
                range = 100000000000000;
            }
            else if (i_max <= 200000000000000)
            {
                range = 200000000000000;
            }
            else if (i_max <= 500000000000000)
            {
                range = 500000000000000;
            }
            else if (i_max <= 1000000000000000)
            {
                range = 1000000000000000;
            }
            else if (i_max <= 2000000000000000)
            {
                range = 2000000000000000;
            }
            else if (i_max <= 5000000000000000)
            {
                range = 5000000000000000;
            }
            else if (i_max <= 10000000000000000)
            {
                range = 10000000000000000;
            }
            else if (i_max <= 20000000000000000)
            {
                range = 20000000000000000;
            }
            else if (i_max <= 50000000000000000)
            {
                range = 50000000000000000;
            }
            else if (i_max <= 100000000000000000)
            {
                range = 100000000000000000;
            }
            else if (i_max <= 200000000000000000)
            {
                range = 200000000000000000;
            }
            else if (i_max <= 500000000000000000)
            {
                range = 500000000000000000;
            }
            else if (i_max <= 1000000000000000000)
            {
                range = 1000000000000000000;
            }
            return range;
        }

        #endregion
    }
}
