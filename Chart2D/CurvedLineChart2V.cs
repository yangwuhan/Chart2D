using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Chart2D
{
    public partial class CurvedLineChart2V : CurvedLineChart1V
    {
        #region 数据和属性成员

        /* 所有曲线公用一条X坐标轴
         */
        public bool XAxisCombined { get; set; }

        /* X轴是否自动调整（XAxisCombined==true时有效）
        */
        public bool XAxisIsAuto { get; set; }

        /* X轴手动调整时，最大值（XAxisCombined==true时有效）
         */
        public float XAxisMaxValue { get; set; }

        /* X轴手动调整时，最小值（XAxisCombined==true时有效）
         */
        public float XAxisMinValue { get; set; }

        #endregion

        #region 函数

        /* 构造函数
         */
        public CurvedLineChart2V()
        {
            InitializeComponent();

            XAxisCombined = false;
            XAxisIsAuto = true;
            XAxisMaxValue = 100.0f;
            XAxisMinValue = 0.0f;
        }

        /* 区域空间是否足够绘图
         */
        public override bool IsSpaceEnoughToDraw(int width, int height)
        {
            int y_space_need = (_MarginSpace << 1) + _TitleYSpace + (_GridYSpliteCount * _MIN_GRID_XY_SPACE);
            if (XAxisCombined) y_space_need += (_AxesKdSpace + _AxesSpace);
            else y_space_need += (_Curves.Count == 0 ? (_AxesKdSpace + _AxesSpace) : _Curves.Count * (_AxesKdSpace + _AxesSpace));
            if (height < y_space_need)
                return false;
            int x_space_need = (_MarginSpace << 1) + (_GridXSpliteCount * _MIN_GRID_XY_SPACE);
            if (YAxisCombined) x_space_need += (_AxesKdSpace + _AxesSpace);
            else x_space_need += (_Curves.Count == 0 ? (_AxesKdSpace + _AxesSpace) : _Curves.Count * (_AxesKdSpace + _AxesSpace));
            if (width < x_space_need)
                return false;
            return true;
        }

        /* 获取X坐标绘图区域在Y轴向占用像素
        */
        protected override int _GetXAxisSpace()
        {
            if (XAxisCombined)
                return (_AxesKdSpace + _AxesSpace);
            else
                return (_Curves.Count == 0 ? (_AxesKdSpace + _AxesSpace) : _Curves.Count * (_AxesKdSpace + _AxesSpace));
        }

        /* 绘制单个X坐标轴名称/单位和刻度线值
         */
        protected override void _DrawSingleXAxisVal(Graphics g, Rectangle rect_axis, Rectangle rect_grid, string name_unit,
                                        Font font, Brush brush,
                                        float f_min, float f_max, DataFormat df)
        {
            Point pt_center = new Point(rect_axis.X + rect_axis.Width / 2, rect_axis.Y + _AxesKdSpace + (int)Math.Floor((float)_AxesSpace * 3.0f / 4.0f));
            _DrawXAxisText(g, name_unit, pt_center, 0, font, brush); //名称/单位
            //刻度值          
            float v_span = (f_max - f_min) / _GridXSpliteCount;
            if (v_span < 0)
                v_span = 0;
            int kd_center_y = rect_axis.Y + _AxesKdSpace + (int)Math.Floor((float)_AxesSpace / 4.0f);
            float col_width = (float)rect_grid.Width / (float)_GridXSpliteCount;
            for (int i = 0; i < _GridXSpliteCount + 1; ++i)
            {
                int h_lean = 0;
                float v_kd = 0;
                if (i == 0)
                {
                    h_lean = -1;
                    v_kd = f_min;
                    pt_center = new Point(rect_axis.X, kd_center_y);
                }
                else if (i == _GridYSpliteCount)
                {
                    h_lean = 1;
                    v_kd = f_max;
                    pt_center = new Point(rect_axis.X + rect_axis.Width, kd_center_y);
                }
                else
                {
                    h_lean = 0;
                    if (f_min == -f_max && i == _GridYSpliteCount / 2)
                        v_kd = 0.0f;
                    else
                        v_kd = f_min + i * v_span;
                    pt_center = new Point((int)Math.Round(rect_axis.X + col_width * i), kd_center_y);
                }
                string s_kd = df.Format(v_kd);
                _DrawXAxisText(g, s_kd, pt_center, h_lean, font, brush); //刻度值
            }
        }

        /* 获取数据间隔总和的最大值
         */
        protected override float _GetMaxDeltaAmount()
        {
            float ret = 0.0f;
            _Curves.ForEach((ChartCurve1V c) =>
            {
                ChartCurve2V cc = c as ChartCurve2V;
                if (cc == null)
                    return;
                int pt_cnt = (cc.YAxisData.Count > cc.XAxisData.Count ? cc.YAxisData.Count : cc.XAxisData.Count);
                float l = pt_cnt * cc.Delta;
                if (l > ret)
                    ret = l;
            });
            return ret;
        }

        /* 绘制X坐标轴
         */
        protected override void _DrawXAxis(Graphics g, int width, int height)
        {
            Rectangle rect_grid = _GetGridRect(width, height);
            Rectangle rect_axis = _GetXAxisRect(width, height);
            if (XAxisCombined || _Curves.Count == 0)
            {
                _DrawSingleXAxis(g, rect_axis, rect_grid, _AxesPen);
                string name_unit = XAxisName + (string.IsNullOrEmpty(XAxisUnitName) ? "" : ("（" + XAxisUnitName + "）"));
                if (XAxisIsAuto)
                {
                    float x_axis_max = 0.0f, x_axis_min = 0.0f;
                    bool first = true;
                    for (int i = 0; i < _Curves.Count; ++i)
                    {
                        ChartCurve2V cc = _Curves[i] as ChartCurve2V;
                        if (cc == null)
                            continue;
                        List<float> data = cc.XAxisData;
                        float f_max, f_min;
                        cc.GetXAxisDataMaxMinValueInSpan(0, data.Count, out f_max, out f_min);
                        if (first)
                        {
                            first = false;
                            x_axis_max = f_max;
                            x_axis_min = f_min;
                        }
                        else
                        {
                            if (x_axis_max < f_max)
                                x_axis_max = f_max;
                            if (x_axis_min > f_min)
                                x_axis_min = f_min;
                        }
                    }
                    float abs_max = Math.Abs(x_axis_max) > Math.Abs(x_axis_min) ? Math.Abs(x_axis_max) : Math.Abs(x_axis_min);
                    Int64 y_axis_range = ChartCurve1V.GetAxisRange(abs_max);
                    _XAxisMaxValueUsed = (float)y_axis_range / (float)1E6;
                    _XAxisMinValueUsed = -_XAxisMaxValueUsed;
                }
                else
                {
                    _XAxisMaxValueUsed = XAxisMaxValue;
                    _XAxisMinValueUsed = XAxisMinValue;
                }
                _DrawSingleXAxisVal(g, rect_axis, rect_grid, name_unit, _AxesFont, _AxesBrush, _XAxisMinValueUsed, _XAxisMaxValueUsed, XAxisKdFormat);
            }
            else
            {
                int single_axis_height = (int)Math.Round((double)rect_axis.Height / (double)_Curves.Count);
                for (int i = 0; i < _Curves.Count; ++i)
                {
                    ChartCurve2V cc = _Curves[i] as ChartCurve2V;
                    if (cc == null)
                        continue;
                    Rectangle rect_axis_single;
                    if (i == 0)
                        rect_axis_single = new Rectangle(rect_axis.Left, rect_axis.Top, rect_axis.Width, single_axis_height);
                    else if (i == _Curves.Count - 1)
                        rect_axis_single = new Rectangle(rect_axis.Left, rect_axis.Bottom - single_axis_height, rect_axis.Width, single_axis_height);
                    else
                        rect_axis_single = new Rectangle(rect_axis.Left, rect_axis.Top + i * single_axis_height, rect_axis.Width, single_axis_height);
                    _DrawSingleXAxis(g, rect_axis_single, rect_grid, cc.Pen);
                    string name_unit = cc.XAxisName + (string.IsNullOrEmpty(cc.XAxisUnitName) ? "" : ("（" + cc.XAxisUnitName + "）"));
                    if (cc.XAxisIsAuto)
                    {
                        float f_max, f_min;
                        cc.GetXAxisDataMaxMinValueInSpan(0, cc.XAxisData.Count, out f_max, out f_min);
                        float abs_max = Math.Abs(f_max) > Math.Abs(f_min) ? Math.Abs(f_max) : Math.Abs(f_min);
                        Int64 x_axis_range = ChartCurve1V.GetAxisRange(abs_max);
                        cc.XAxisMaxValueUsed = (float)x_axis_range / (float)1E6;
                        cc.XAxisMinValueUsed = -cc.XAxisMaxValueUsed;
                    }
                    else
                    {
                        cc.XAxisMaxValueUsed = cc.XAxisMaxValue;
                        cc.XAxisMinValueUsed = cc.XAxisMinValue;
                    }
                    _DrawSingleXAxisVal(g, rect_axis_single, rect_grid, name_unit, _AxesFont, cc.Brush, cc.XAxisMinValueUsed, cc.XAxisMaxValueUsed, cc.XAxisKdFormat);
                }
            }
        }

        /* 绘制Y坐标轴
         */
        protected override void _DrawYAxis(Graphics g, int width, int height)
        {
            Rectangle rect_grid = _GetGridRect(width, height);
            Rectangle rect_axis = _GetYAxisRect(width, height);
            if (YAxisCombined || _Curves.Count == 0)
            {
                _DrawSingleYAxis(g, rect_axis, rect_grid, _AxesPen);
                string name_unit = YAxisName + (string.IsNullOrEmpty(YAxisUnitName) ? "" : ("（" + YAxisUnitName + "）"));
                if (YAxisIsAuto)
                {
                    float y_axis_max = 0.0f, y_axis_min = 0.0f;
                    bool first = true;
                    for (int i = 0; i < _Curves.Count; ++i)
                    {
                        ChartCurve2V cc = _Curves[i] as ChartCurve2V;
                        if(cc == null)
                            continue;
                        List<float> data = cc.YAxisData;
                        float f_max, f_min;
                        cc.GetYAxisDataMaxMinValueInSpan(0, data.Count, out f_max, out f_min);
                        if (first)
                        {
                            first = false;
                            y_axis_max = f_max;
                            y_axis_min = f_min;
                        }
                        else
                        {
                            if (y_axis_max < f_max)
                                y_axis_max = f_max;
                            if (y_axis_min > f_min)
                                y_axis_min = f_min;
                        }
                    }
                    float abs_max = Math.Abs(y_axis_max) > Math.Abs(y_axis_min) ? Math.Abs(y_axis_max) : Math.Abs(y_axis_min);
                    Int64 y_axis_range = ChartCurve1V.GetAxisRange(abs_max);
                    _YAxisMaxValueUsed = (float)y_axis_range / (float)1E6;
                    _YAxisMinValueUsed = -_YAxisMaxValueUsed;
                }
                else
                {
                    _YAxisMaxValueUsed = YAxisMaxValue;
                    _YAxisMinValueUsed = YAxisMinValue;
                }
                _DrawSingleYAxisVal(g, rect_axis, rect_grid, name_unit, _AxesFont, _AxesBrush, _YAxisMinValueUsed, _YAxisMaxValueUsed, YAxisKdFormat);
            }
            else
            {
                int single_axis_width = (int)Math.Round((double)rect_axis.Width / (double)_Curves.Count);
                for (int i = 0; i < _Curves.Count; ++i)
                {
                    ChartCurve2V cc = _Curves[i] as ChartCurve2V;
                    if(cc == null)
                        continue;
                    Rectangle rect_axis_single;
                    if (i == 0)
                        rect_axis_single = new Rectangle(rect_axis.Right - single_axis_width, rect_axis.Y, single_axis_width, rect_axis.Height);
                    else if (i == _Curves.Count - 1)
                        rect_axis_single = new Rectangle(rect_axis.X, rect_axis.Y, single_axis_width, rect_axis.Height);
                    else
                        rect_axis_single = new Rectangle(rect_axis.X + (_Curves.Count - 1 - i) * single_axis_width, rect_axis.Y, single_axis_width, rect_axis.Height);
                    _DrawSingleYAxis(g, rect_axis_single, rect_grid, cc.Pen);
                    string name_unit = cc.YAxisName + (string.IsNullOrEmpty(cc.YAxisUnitName) ? "" : ("（" + cc.YAxisUnitName + "）"));
                    if (cc.YAxisIsAuto)
                    {
                        float f_max, f_min;
                        cc.GetYAxisDataMaxMinValueInSpan(0, cc.YAxisData.Count, out f_max, out f_min);
                        float abs_max = Math.Abs(f_max) > Math.Abs(f_min) ? Math.Abs(f_max) : Math.Abs(f_min);
                        Int64 y_axis_range = ChartCurve1V.GetAxisRange(abs_max);
                        cc.YAxisMaxValueUsed = (float)y_axis_range / (float)1E6;
                        cc.YAxisMinValueUsed = -cc.YAxisMaxValueUsed;
                    }
                    else
                    {
                        cc.YAxisMaxValueUsed = cc.YAxisMaxValue;
                        cc.YAxisMinValueUsed = cc.YAxisMinValue;
                    }
                    _DrawSingleYAxisVal(g, rect_axis_single, rect_grid, name_unit, _AxesFont, cc.Brush, cc.YAxisMinValueUsed, cc.YAxisMaxValueUsed, cc.YAxisKdFormat);
                }
            }
        }

        /* 绘制单条曲线
         */
        protected override void _DrawSingleCurve(Graphics g, int width, int height, ChartCurve1V c)
        {
            ChartCurve2V cc = c as ChartCurve2V;
            Rectangle rect_grid = _GetGridRect(width, height);
            int chart_width = rect_grid.Width;
            int chart_height = rect_grid.Height;
            List<Point> pts = new List<Point>();
            List<float> data_y = cc.YAxisData;
            List<float> data_x = cc.XAxisData;
            if (data_y.Count < 2) 
                return;
            cc._ChartCursors2V.Clear();        
            float y_axis_max, y_axis_min;
            if (YAxisCombined)
            {
                y_axis_max = _YAxisMaxValueUsed;
                y_axis_min = _YAxisMinValueUsed;
            }
            else
            {
                y_axis_max = cc.YAxisMaxValueUsed;
                y_axis_min = cc.YAxisMinValueUsed;
            }
            float x_axis_max, x_axis_min;
            if (XAxisCombined)
            {
                x_axis_max = _XAxisMaxValueUsed;
                x_axis_min = _XAxisMinValueUsed;
            }
            else
            {
                x_axis_max = cc.XAxisMaxValueUsed;
                x_axis_min = cc.XAxisMinValueUsed;
            }
            for (int i = 0; i < data_x.Count && i < data_y.Count; ++i)
            {
                float x_v = data_x[i];
                int pt_x = rect_grid.X + _GetXAxisPos(x_v, x_axis_max, x_axis_min, rect_grid.Width);

                float y_v = data_y[i];
                int pt_y = rect_grid.Y + _GetYAxisPos(y_v, y_axis_max, y_axis_min, rect_grid.Height);

                pts.Add(new Point(pt_x, pt_y));
                ChartCursor2V cursor = new ChartCursor2V
                {
                    Pt_x = pt_x,
                    Pt_y = pt_y,
                    XValue = x_v,
                    YValue = y_v,
                    Pt_count = 1,
                    Y_max = y_v,
                    Y_min = y_v,
                    X_max = x_v,
                    X_min = x_v,
                };
                if (!cc._ChartCursors2V.Keys.Contains(pt_x))
                {
                    cc._ChartCursors2V.Add(pt_x, new Dictionary<int, ChartCursor2V>());
                    cc._ChartCursors2V[pt_x].Add(pt_y, cursor);
                }
                else
                {
                    if (!cc._ChartCursors2V[pt_x].Keys.Contains(pt_y))
                        cc._ChartCursors2V[pt_x].Add(pt_y, cursor);
                    else
                    {
                        ChartCursor2V cursor_exist = cc._ChartCursors2V[pt_x][pt_y];
                        cursor_exist.Pt_count++;
                        cursor_exist.Y_max = (cursor_exist.Y_max >= y_v ? cursor_exist.Y_max : y_v);
                        cursor_exist.Y_min = (cursor_exist.Y_min <= y_v ? cursor_exist.Y_min : y_v);
                        cursor_exist.X_max = (cursor_exist.X_max >= x_v ? cursor_exist.X_max : x_v);
                        cursor_exist.X_min = (cursor_exist.X_min >= x_v ? cursor_exist.X_min : x_v);
                    }
                }
            }
            if (pts.Count > 1)
            {
                g.DrawLines(cc.Pen, pts.ToArray());
                Rectangle r = new Rectangle();
                r.Width = 4;
                r.Height = 4;
                foreach (var kv in cc._ChartCursors2V)
                {
                    int pt_x = kv.Key;
                    if (cc._ChartCursors2V.Keys.Contains(pt_x + 1) || cc._ChartCursors2V.Keys.Contains(pt_x - 1))
                        continue;
                    foreach(var kv2 in kv.Value)
                    {
                        int pt_y = kv2.Key;
                        if (kv.Value.Keys.Contains(pt_y + 1) || kv.Value.Keys.Contains(pt_y - 1))
                            continue;
                        r.X = pt_x - 2;
                        r.Y = pt_y - 2;
                        g.DrawRectangle(cc.Pen, r);
                    }
                }
            }                
            else if (pts.Count == 1)
                g.DrawRectangle(cc.Pen, new Rectangle(pts[0].X - 2, pts[0].Y - 2, 4, 4));
        }        

        /* 获取最接近的点的游标
         */
        private ChartCursor2V _GetNearestPtCursor(int x, int y, Dictionary<int, Dictionary<int, ChartCursor2V>> cursors)
        {
            if (cursors.ContainsKey(x) && cursors[x].ContainsKey(y))
                return cursors[x][y];
            else
            {
                if (_NEAREST_PIXEL_RANG > 0) //只找当前鼠标所在位置
                {
                    for (int i = 1; i <= _NEAREST_PIXEL_RANG; ++i)
                    {
                        //y-i
                        for(int j = x - i; j <= x + i; ++j)
                        {
                            if (cursors.ContainsKey(j) && cursors[j].ContainsKey(y-i))
                                return cursors[j][y-i];
                        }
                        //x-i
                        for (int j = y - i + 1; j <= y + i - 1; ++j)
                        {
                            if (cursors.ContainsKey(x - i) && cursors[x - i].ContainsKey(j))
                                return cursors[x - i][j];
                        }
                        //x+i
                        for (int j = y - i + 1; j <= y + i - 1; ++j)
                        {
                            if (cursors.ContainsKey(x + i) && cursors[x + i].ContainsKey(j))
                                return cursors[x + i][j];
                        }
                        //y+i
                        for (int j = x - i; j <= x + i; ++j)
                        {
                            if (cursors.ContainsKey(j) && cursors[j].ContainsKey(y + i))
                                return cursors[j][y + i];
                        }
                    }
                    return null;
                }
                else
                    return null;
            }
        }

        /* 绘制游标
         */
        protected override void _DrawCursors(Graphics g, Point pt)
        {
            for (int ijk = 0; ijk < _Curves.Count; ++ijk)
            {
                ChartCurve2V cc = _Curves[ijk] as ChartCurve2V;
                if (cc == null)
                    continue;
                Dictionary<int, Dictionary<int, ChartCursor2V>> cc_cursors = cc._ChartCursors2V;
                ChartCursor2V cursor = _GetNearestPtCursor(pt.X, pt.Y, cc_cursors);
                if (cursor != null)
                {
                    g.FillEllipse(_OutlinePtBrush, cursor.Pt_x - _CURSOR_HALF_SIDE, cursor.Pt_y - _CURSOR_HALF_SIDE, _CURSOR_HALF_SIDE << 1, _CURSOR_HALF_SIDE << 1);
                    string info = "";
                    if (CursorFormat == null)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("X:").Append(cc.XAxisValueFormat.Format(cursor.X_max)).Append(cc.XAxisUnitName).Append("-Y:").Append(cc.YAxisValueFormat.Format(cursor.Y_max)).Append(cc.YAxisUnitName).Append("\r\n");
                        info = sb.ToString();
                        sb.Clear();
                    }
                    else
                        info = CursorFormat.Format<int>(cursor.YIndex);
                    if (!string.IsNullOrEmpty(info))
                        _DrawCursor(g, info, new Point(cursor.Pt_x, cursor.Pt_y), cc.Pen, _AxesFont, cc.Brush, _DragRectBrush, _DestImage.Width, _DestImage.Height);
                }
            }
        }

        #endregion
    }
}
