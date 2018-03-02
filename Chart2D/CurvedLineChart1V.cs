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
    public partial class CurvedLineChart1V : UserControl
    {
        #region 数据和属性成员

        /* 允许绘图
         */ 
        public bool AllowWork{ get; set;}
        
        /* 背景颜色
         */
        public Color ChartBackColor { get; set; }

        /* 定时绘图定时间隔，单位：毫秒
         */ 
        public const int PAINT_INTERVAL = 50;

        /* 网格的Y轴向分割数
         */
        protected int _GridYSpliteCount = 10;
        public int GridYSpliteCount 
        {
            get { return _GridYSpliteCount; }
            set 
            {
                if (value < 2)
                    _GridYSpliteCount = 2;
                else if (value > 20)
                    _GridYSpliteCount = 20;
                else 
                    _GridYSpliteCount = value;
                if (_GridYSpliteCount % 2 == 1) ++_GridYSpliteCount;
            } 
        }

        /* 网格的X轴向分割数
         */
        protected int _GridXSpliteCount = 10;
        public int GridXSpliteCount
        {
            get { return _GridXSpliteCount; }
            set
            {
                if (value < 2)
                    _GridXSpliteCount = 2;
                else if (value > 20)
                    _GridXSpliteCount = 20;
                else
                    _GridXSpliteCount = value;
                if (_GridXSpliteCount % 2 == 1) ++_GridXSpliteCount;
            }
        }

        /* 所有曲线公用一条Y坐标轴
         */
        public bool YAxisCombined { get; set; }

        /* Chart表头Y轴向占用像素
         */
        protected int _TitleYSpace = 0;
        public int TitleYSpace
        {
            get { return _TitleYSpace; }
            set
            {
                if (value < 0)
                    _TitleYSpace = 0;
                else if (value > 100)
                    _TitleYSpace = 100;
                else
                    _TitleYSpace = value;
                if(_TitleYSpace > 0)
                {
                    float fs = _GetSuitedFontSize(_TitleYSpace);
                    _TitleFont = new Font("微软雅黑", fs, FontStyle.Regular, GraphicsUnit.Pixel); 
                }
            }
        }

        /* 网格Y轴向/X轴向最小占用像素
         */
        protected const int _MIN_GRID_XY_SPACE = 10;

        /* 曲线
         */
        protected List<ChartCurve1V> _Curves = new List<ChartCurve1V>();
        [Bindable(false), Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]  
        public List<ChartCurve1V> Curves
        {
            get { return _Curves; }
            set
            {
                if (value == null)
                    return;
                _Curves = value;
            }
        }

        /* Chart绘图区域离可绘图边界的距离，单位：像素
        */
        protected int _MarginSpace = 2;
        public int MarginSpace
        {
            set
            {
                if (value < 0)
                    _MarginSpace = 0;
                else if (value > 100)
                    _MarginSpace = 100;
                else _MarginSpace = value;
            }
        }

        /* X坐标轴的Y轴向/Y坐标轴的X轴向占用的像素
         */
        protected int _AxesSpace;
        public int AxesSpace
        {
            get { return _AxesSpace; }
            set
            {
                int new_v = value;
                if (new_v <= 20)
                    new_v = 20;
                if (_AxesSpace != new_v)
                {
                    _AxesSpace = new_v;
                    float fs = _GetSuitedFontSize((float)_AxesSpace / (float)2.0f);
                    _AxesFont = new Font("微软雅黑", fs, FontStyle.Regular, GraphicsUnit.Pixel); 
                }
            }
        }

        /* 刻度线占用像素
         */
        protected int _AxesKdSpace = 3;
        public int AxesKdSpace
        { 
            get { return _AxesKdSpace; } 
            set
            {
                int new_v = value;
                if (new_v < 1)
                    new_v = 1;
                _AxesKdSpace = new_v;
            }
        }

        /* 网格的画笔/坐标轴和坐标轴刻度线的默认画笔
         */
        protected Pen _AxesPen;

        /* 坐标轴的刻度/名称/单位 的 字体/刷子
         */
        protected Font _AxesFont;
        protected Brush _AxesBrush;
        protected Brush _DragRectBrush;
        
        /* 坐标轴默认颜色
         */
        private Color _AxesColor = Color.Gray;
        public Color AxesColor
        {
            get { return _AxesColor; }
            set
            {
                _AxesPen = new Pen(value);
                _AxesPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                _AxesBrush = new SolidBrush(value);
                _DragRectBrush = new SolidBrush(Color.FromArgb(125, value));
            }
        }

        /* 游标位置的数据点框框的刷子
         */
        protected Brush _OutlinePtBrush = new SolidBrush(Color.FromArgb(170, 40, 255, 40));

        /* 缓冲区
         */
        protected Bitmap _DataImage;

        /* 显示区
         */
        protected Bitmap _DestImage;

        /* 缩放比例尺起始/截止比例，取值范围0~1，_DataBilichiStart < _DataBilichiEnd
         */
        protected float _DataBilichiStart = 0;
        protected float _DataBilichiStop = 1;

        /* 数据发生改变，会导致重绘缓冲区
         */
        protected bool _DataChanged = false;
        public bool DataChanged { set { _DataChanged = value; } }

        /* 比例尺发生改变，会导致重绘缓冲区
         */
        protected bool _BilichiChanged = false;

        /* 鼠标进入绘图绘图区域的标志
         */
        protected bool _MouseIn = false;

        /* 按住鼠标左键，并且拖动的标志
         */
        protected bool _MouseDraging = false;
        /* 按住鼠标左键后记录的鼠标位置
         */
        protected Point _MousePositionStartDrag;
        /* 按住鼠标左键后释放鼠标时的鼠标位置
         */
        protected Point _MousePositionStopDrag;

        /* 找最接近的点的游标时的查找像素范围
         */
        protected const int _NEAREST_PIXEL_RANG = 4; 

        /* 游标方块半边长
         */
        protected const int _CURSOR_HALF_SIDE = 10;

        /* 表的标题
         */
        public string Title { get; set; }

        /* 标题的颜色
         */
        protected Color _TitleColor;
        public Color TitleColor
        {
            get { return _TitleColor; }
            set
            {
                _TitleColor = value;
                _TitleBrush = new SolidBrush(value);
            }
        }

        /* 标题的字体/刷子
         */ 
        protected Font _TitleFont;
        protected Brush _TitleBrush;

        /* Y轴的名称（YAxisCombined==true时有效）
        */
        public string YAxisName { get; set; }

        /* Y轴的数值单位（YAxisCombined==true时有效）
         */
        public string YAxisUnitName { get; set; }

        /* Y轴是否自动调整（YAxisCombined==true时有效）
         */
        public bool YAxisIsAuto { get; set; }

        /* Y轴手动调整时，最大值（YAxisCombined==true时有效）
         */
        public float YAxisMaxValue { get; set; }

        /* Y轴手动调整时，最小值（YAxisCombined==true时有效）
         */
        public float YAxisMinValue { get; set; }

        /* Y轴最大值和最小值（YAxisCombined==true时有效）
        */
        protected float _YAxisMaxValueUsed;
        protected float _YAxisMinValueUsed;

        /* Y轴，刻度值格式化对象，和数值格式化对象（YAxisCombined==true时有效）
         */
        public DataFormat YAxisKdFormat { get; set; }
        public DataFormat YAxisValueFormat { get; set; }

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
        protected float _XAxisMaxValueUsed;
        protected float _XAxisMinValueUsed;

        /* 游标格式化对象
        */
        public DataFormat CursorFormat { get; set; }

        #endregion

        #region 函数

        /* 构造函数
         */
        public CurvedLineChart1V()
        {
            InitializeComponent();
            AllowWork = true;
            ChartBackColor = Color.White;
            YAxisCombined = false;
            TitleColor = Color.Black;
            AxesColor = Color.Gray;
            AxesKdSpace = 3;
            AxesSpace = 20 + AxesKdSpace;
            Title = "";
            YAxisName = "";
            YAxisUnitName = "";
            YAxisIsAuto = true;
            _YAxisMaxValueUsed = YAxisMaxValue = 20.0f;
            _YAxisMinValueUsed = YAxisMinValue = -20.0f;
            YAxisKdFormat = new DataFormat(EDataFormat.FORMAT_STRING, "{0}");
            YAxisValueFormat = new DataFormat(EDataFormat.FORMAT_STRING, "{0}");
            XAxisKdFormat = new DataFormat(EDataFormat.FORMAT_STRING, "{0}");
            XAxisValueFormat = new DataFormat(EDataFormat.FORMAT_STRING, "{0}");
            XAxisName = "";
            XAxisUnitName = "";
            _XAxisMaxValueUsed = 0.0f;
            _XAxisMinValueUsed = 0.0f;
            CursorFormat = null;
            this.Paint += new PaintEventHandler((object sender, PaintEventArgs e) =>
            {
                if (_DestImage != null)
                {
                    Graphics g = e.Graphics;
                    g.DrawImage(_DestImage, e.ClipRectangle);
                }
            });
            this.MouseEnter += new EventHandler((object sender, EventArgs e) =>
            {
                _MouseIn = true;
            });
            this.MouseLeave += new EventHandler((object sender, EventArgs e) =>
            {
                _MouseIn = false;
            });
            this.MouseDown += new MouseEventHandler((object sender, MouseEventArgs e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    _MouseDraging = true;
                    _MousePositionStartDrag = e.Location;
                }
            });
            this.MouseUp += new MouseEventHandler((object sender, MouseEventArgs e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    _MouseDraging = false;
                    _MousePositionStopDrag = e.Location;

                    if (_MousePositionStopDrag.X == _MousePositionStartDrag.X ||
                        _MousePositionStopDrag.Y == _MousePositionStartDrag.Y) return;

                    int x_min = _MousePositionStopDrag.X < _MousePositionStartDrag.X ? _MousePositionStopDrag.X : _MousePositionStartDrag.X;
                    int x_max = _MousePositionStopDrag.X > _MousePositionStartDrag.X ? _MousePositionStopDrag.X : _MousePositionStartDrag.X;
                    if (x_max <= _MarginSpace + _GetYAxisSpace()) return;
                    if (x_min >= this.ClientRectangle.Width - _MarginSpace) return;
                    if (x_min < _MarginSpace + _GetYAxisSpace()) x_min = _MarginSpace + _GetYAxisSpace();
                    if (x_max > this.ClientRectangle.Width - _MarginSpace) x_max = this.ClientRectangle.Width - _MarginSpace;

                    bool bili_changed = false;
                    float cur_bili_span = _DataBilichiStop - _DataBilichiStart;
                    float s_add_rate = cur_bili_span * (float)(x_min - _MarginSpace - _GetYAxisSpace()) / (float)this.ClientRectangle.Width;
                    float e_minus_rate = cur_bili_span * (float)(this.ClientRectangle.Width - _MarginSpace - x_max) / (float)this.ClientRectangle.Width;
                    if (_DataBilichiStart + s_add_rate < _DataBilichiStop)
                    {
                        _DataBilichiStart += s_add_rate;
                        bili_changed = true;
                    }
                    if (_DataBilichiStop - e_minus_rate > _DataBilichiStart)
                    {
                        _DataBilichiStop -= e_minus_rate;
                        bili_changed = true;
                    }
                    if (bili_changed) _BilichiChanged = true;
                }
            });
            this.MouseWheel += new MouseEventHandler((object sender, MouseEventArgs e) => {
                if (_MouseDraging)
                    return;
                bool bili_changed = false;
                float cur_bili_span = _DataBilichiStop - _DataBilichiStart;
                float s_add_rate = cur_bili_span * 0.05f;
                float e_minus_rate = cur_bili_span * 0.05f;
                if (e.Delta > 0)
                {
                    if (cur_bili_span == 0.0f) return;
                    if (_DataBilichiStart + s_add_rate < _DataBilichiStop)
                    {
                        _DataBilichiStart += s_add_rate;
                        bili_changed = true;
                    }
                    if (_DataBilichiStop - e_minus_rate > _DataBilichiStart)
                    {
                        _DataBilichiStop -= e_minus_rate;
                        bili_changed = true;
                    }
                    if (bili_changed) _BilichiChanged = true;
                }                
                else if (e.Delta < 0)
                {
                    if (_DataBilichiStart == 0.0f && _DataBilichiStop == 1.0f) return;
                    if (_DataBilichiStart - s_add_rate >= 0.0f)
                    {
                        _DataBilichiStart -= s_add_rate;
                        bili_changed = true;
                    }
                    else
                    {
                        _DataBilichiStart = 0;
                        bili_changed = true;
                    }
                    if (_DataBilichiStop + e_minus_rate <= 1.0f)
                    {
                        _DataBilichiStop += e_minus_rate;
                        bili_changed = true;
                    }
                    else
                    {
                        _DataBilichiStop = 1.0f;
                        bili_changed = true;
                    }
                    if (bili_changed) _BilichiChanged = true;
                }
            });
            timer_paint.Interval = PAINT_INTERVAL;
            timer_paint.Tick += new EventHandler((object sender, EventArgs e) => {
                timer_paint.Enabled = false;
                if(AllowWork)
                {
                    bool b_re_draw_data = false;
                    if (_DataImage == null ||
                        _DataImage.Width != this.ClientRectangle.Width ||
                        _DataImage.Height != this.ClientRectangle.Height ||
                        _DataChanged ||
                        _BilichiChanged)
                    {
                        b_re_draw_data = true;
                    }
                    if (b_re_draw_data)
                    {
                        _DrawData();
                        _DrawDest();
                        Invalidate();
                        _DataChanged = false;
                        _BilichiChanged = false;
                    }
                    else
                    {
                        _DrawDest();
                        Invalidate();
                    }
                }                
                timer_paint.Enabled = true;
            });
            timer_paint.Enabled = true;
        }

        /* Load
         */ 
        private void CurvedLineChart1V_Load(object sender, EventArgs e)
        {

        }

        /* 区域空间是否足够绘图
         */
        public virtual bool IsSpaceEnoughToDraw(int width, int height)
        {
            int y_space_need = (_MarginSpace << 1) + _TitleYSpace + (_GridYSpliteCount * _MIN_GRID_XY_SPACE);
            y_space_need += (_AxesKdSpace + _AxesSpace);
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
        protected virtual int _GetXAxisSpace()
        {
            return (_AxesKdSpace + _AxesSpace);
        }

        /* 获取Y坐标绘图区域在X轴向占用像素
         */
        protected int _GetYAxisSpace()
        {
            if (YAxisCombined)
                return (_AxesKdSpace + _AxesSpace);
            else
                return (_Curves.Count == 0 ? (_AxesKdSpace + _AxesSpace) : _Curves.Count * (_AxesKdSpace + _AxesSpace));
        }

        /* 获取网格绘制区域，单位：像素
         */
        protected Rectangle _GetGridRect(int width, int height)
        {
            Rectangle rect = new Rectangle();
            rect.X = _MarginSpace + _GetYAxisSpace();
            rect.Width = width - rect.X - _MarginSpace;
            rect.Y = _MarginSpace + _TitleYSpace;
            rect.Height = height - rect.Y - _MarginSpace - _GetXAxisSpace();
            return rect;
        }

        /* 获取X坐标轴绘制区域，单位：像素
         */
        protected Rectangle _GetXAxisRect(int width, int height)
        {
            Rectangle rect = new Rectangle();
            rect.X = _MarginSpace + _GetYAxisSpace();
            rect.Width = width - rect.X - _MarginSpace;
            int x_axis_space = _GetXAxisSpace();
            rect.Y = height - _MarginSpace - x_axis_space;
            rect.Height = x_axis_space;
            return rect;
        }

        /* 获取Y坐标轴绘制区域，单位：像素
         */
        protected Rectangle _GetYAxisRect(int width, int height)
        {
            Rectangle rect = new Rectangle();
            rect.X = _MarginSpace;
            rect.Width = _GetYAxisSpace();
            rect.Y = _MarginSpace + _TitleYSpace;
            rect.Height = height - rect.Y - _MarginSpace - _GetXAxisSpace();
            return rect;
        }

        /* 绘制网格和标题
         */
        protected void _DrawGrid(Graphics g, int width, int height)
        {
            if (!IsSpaceEnoughToDraw(width, height))
                return;
            Rectangle rect_grid = _GetGridRect(width, height);
            //X轴向网格线
            int rows = _GridYSpliteCount;
            float row_height = (float)rect_grid.Height / (float)rows;
            int x1 = rect_grid.X;
            int x2 = x1 + rect_grid.Width;
            int y1 = 0;
            int y2 = 0;
            for (int i = 0; i < rows; ++i)
            {
                if (i == 0)
                    y2 = y1 = rect_grid.Y;
                else
                    y2 = y1 = (int)Math.Round((float)rect_grid.Y + (float)i * row_height);
                g.DrawLine(_AxesPen, x1, y1, x2, y2);
            }
            //Y轴向网格线
            int cols = _GridXSpliteCount;
            float col_width = (float)rect_grid.Width / (float)cols;
            x1 = x2 = 0;
            y1 = rect_grid.Y;
            y2 = y1 + rect_grid.Height;
            for (int i = 0; i < cols; ++i)
            {
                if (i != cols - 1)
                    x2 = x1 = (int)Math.Round((float)rect_grid.X + (float)(i + 1) * col_width);
                else
                    x2 = x1 = rect_grid.X + rect_grid.Width - 1;
                g.DrawLine(_AxesPen, x1, y1, x2, y2);
            }
            //标题
            if(_TitleYSpace > 0 && !string.IsNullOrEmpty(Title))
            {
                Point pt_center = new Point(rect_grid.X + rect_grid.Width / 2, rect_grid.Y - _TitleYSpace / 2);
                _DrawXAxisText(g, Title, pt_center, 0, _TitleFont, _TitleBrush);
            }
        }

        /* 获取最适合文字区域的文字大小
         */
        protected float _GetSuitedFontSize(float height)
        {
            return (float)Math.Floor(0.8D * (double)height);
        }

        /* 绘制Y轴向文字
         */
        protected void _DrawYAxisText(Graphics g, string str, Point point_center, int point_center_h_lean, Font font, Brush brush)
        {
            SizeF szf = g.MeasureString(str, font);
            PointF pf_draw;
            if (point_center_h_lean == -1)
                pf_draw = new PointF(point_center.X, point_center.Y - szf.Height / 2.0f);
            else if (point_center_h_lean == 1)
                pf_draw = new PointF(point_center.X - szf.Width, point_center.Y - szf.Height / 2.0f);
            else
                pf_draw = new PointF(point_center.X - szf.Width / 2.0f, point_center.Y - szf.Height / 2.0f);
            pf_draw.Y += 1;
            Matrix matrix_old = g.Transform;
            Matrix matrix_new = g.Transform;
            matrix_new.RotateAt(270, point_center);
            g.Transform = matrix_new;
            g.DrawString(str, font, brush, pf_draw);
            g.Transform = matrix_old;
        }        

        /* 绘制单个Y坐标轴和刻度线
         */
        protected void _DrawSingleYAxis(Graphics g, Rectangle rect_axis, Rectangle rect_grid, Pen pen)
        {
            g.DrawLine(pen, rect_axis.Right, rect_axis.Bottom, rect_axis.Right, rect_axis.Top); //坐标轴
            int x1, x2, y1, y2;
            y2 = rect_axis.Top;
            y1 = y2 + 8;
            x2 = rect_axis.Right;
            for (int i = -2; i < 3; ++i)
            {
                if (i == 0) continue;
                x1 = x2 + i;
                g.DrawLine(pen, x1, y1, x2, y2); //坐标轴箭头
            }
            //刻度线
            float row_height = (float)rect_grid.Height / (float)_GridYSpliteCount;
            x1 = rect_axis.X + _AxesSpace;
            x2 = x1 + _AxesKdSpace;
            for (int i = 0; i < _GridYSpliteCount + 1; ++i)
            {
                int pty = 0;
                if (i == 0)
                    pty = rect_axis.Y + rect_axis.Height;
                else if (i == _GridYSpliteCount)
                    pty = rect_axis.Y;
                else
                    pty = (int)Math.Round(rect_axis.Y + row_height * (_GridYSpliteCount - i));
                y1 = y2 = pty;
                g.DrawLine(pen, x1, y1, x2, y2); //刻度线
            }
        }

        /* 绘制单个Y坐标轴名称/单位和刻度线值
         */
        protected void _DrawSingleYAxisVal(Graphics g, Rectangle rect_axis, Rectangle rect_grid, string name_unit,
                                        Font font, Brush brush,
                                        float f_min, float f_max, DataFormat df)
        {
            Point pt_center = new Point(rect_axis.X + (int)Math.Floor((float)_AxesSpace / 4.0f), rect_axis.Y + rect_axis.Height / 2);
            _DrawYAxisText(g, name_unit, pt_center, 0, font, brush); //名称/单位
            //刻度值
            float v_span = (f_max - f_min) / _GridYSpliteCount;
            if (v_span < 0)
                v_span = 0;
            int kd_center_x = rect_axis.X + (int)Math.Floor((float)_AxesSpace * 3.0f / 4.0f);
            float row_height = (float)rect_grid.Height / (float)_GridYSpliteCount;
            for (int i = 0; i < _GridYSpliteCount + 1; ++i)
            {
                float v_kd = 0;
                int h_lean = 0;
                if (i == 0)
                {
                    h_lean = -1;
                    v_kd = f_min;
                    pt_center = new Point(kd_center_x, rect_axis.Y + rect_axis.Height);
                }
                else if (i == _GridYSpliteCount)
                {
                    h_lean = 1;
                    v_kd = f_max;
                    pt_center = new Point(kd_center_x, rect_axis.Y);
                }
                else
                {
                    h_lean = 0;
                    if (f_min == -f_max && i == _GridYSpliteCount / 2)
                        v_kd = 0.0f;
                    else
                        v_kd = f_min + i * v_span;
                    pt_center = new Point(kd_center_x, (int)Math.Round(rect_axis.Y + row_height * (_GridYSpliteCount - i)));
                }
                string s_kd = df.Format(v_kd);
                _DrawYAxisText(g, s_kd, pt_center, h_lean, font, brush); //刻度值
            }
        }

        /* 绘制Y坐标轴
         */
        protected virtual void _DrawYAxis(Graphics g, int width, int height)
        {
            Rectangle rect_grid = _GetGridRect(width, height);
            Rectangle rect_axis = _GetYAxisRect(width, height);
            float delta_amount = _GetMaxDeltaAmount();
            float start_delta_amount, stop_delta_amount;
            ChartCurve1V.GetYAxisStartStopDeltaAmountByBilichi(_DataBilichiStart, _DataBilichiStop, delta_amount, out start_delta_amount, out stop_delta_amount);
            if (YAxisCombined || _Curves.Count == 0)
            {
                _DrawSingleYAxis(g, rect_axis, rect_grid, _AxesPen);
                string name_unit = YAxisName + (string.IsNullOrEmpty(YAxisUnitName) ? "" : ("（" + YAxisUnitName + "）"));
                if(YAxisIsAuto)
                {
                    float y_axis_max = 0.0f, y_axis_min = 0.0f;
                    bool first = true;
                    for (int i = 0; i < _Curves.Count; ++i)
                    {
                        ChartCurve1V cc = _Curves[i];
                        List<float> data = cc.YAxisData;
                        int start_index, stop_index;
                        cc.GetYAxisStartStopIndex(start_delta_amount, stop_delta_amount, out start_index, out stop_index);
                        float f_max, f_min;
                        cc.GetYAxisDataMaxMinValueInSpan(start_index, stop_index, out f_max, out f_min);
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
                for (int i = 0; i < _Curves.Count; ++i )
                {
                    ChartCurve1V cc = _Curves[i];
                    Rectangle rect_axis_single;
                    if (i == 0)
                        rect_axis_single = new Rectangle(rect_axis.Right - single_axis_width, rect_axis.Y, single_axis_width, rect_axis.Height);
                    else if (i == _Curves.Count - 1)
                        rect_axis_single = new Rectangle(rect_axis.X, rect_axis.Y, single_axis_width, rect_axis.Height);
                    else
                        rect_axis_single = new Rectangle(rect_axis.X + (_Curves.Count - 1 - i) * single_axis_width, rect_axis.Y, single_axis_width, rect_axis.Height);
                    _DrawSingleYAxis(g, rect_axis_single, rect_grid, cc.Pen);
                    string name_unit = cc.YAxisName + (string.IsNullOrEmpty(cc.YAxisUnitName) ? "" : ("（" + cc.YAxisUnitName + "）"));
                    if(cc.YAxisIsAuto)
                    {
                        int start_index, stop_index;
                        cc.GetYAxisStartStopIndex(start_delta_amount, stop_delta_amount, out start_index, out stop_index);
                        float f_max, f_min;
                        cc.GetYAxisDataMaxMinValueInSpan(start_index, stop_index, out f_max, out f_min);
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

        /* 绘制X轴向文字
         */
        protected void _DrawXAxisText(Graphics g, string str, Point point_center, int point_center_h_lean, Font font, Brush brush)
        {
            SizeF szf = g.MeasureString(str, font);
            PointF pf_draw;
            if (point_center_h_lean == -1)
                pf_draw = new PointF(point_center.X, point_center.Y - szf.Height / 2.0f);
            else if (point_center_h_lean == 1)
                pf_draw = new PointF(point_center.X - szf.Width, point_center.Y - szf.Height / 2.0f);
            else
                pf_draw = new PointF(point_center.X - szf.Width / 2.0f, point_center.Y - szf.Height / 2.0f);
            g.DrawString(str, font, brush, pf_draw);
        }        

        /* 绘制单个X坐标轴和刻度线
         */
        protected void _DrawSingleXAxis(Graphics g, Rectangle rect_axis, Rectangle rect_grid, Pen pen)
        {
            g.DrawLine(pen, rect_axis.Left, rect_axis.Top, rect_axis.Right, rect_axis.Top); //坐标轴
            int x1, x2, y1, y2;
            x2 = rect_axis.Right;
            x1 = x2 - 8;
            y2 = rect_axis.Top;
            for (int i = -2; i < 3; ++i)
            {
                if (i == 0) continue;
                y1 = y2 + i;
                g.DrawLine(pen, x1, y1, x2, y2); //坐标轴箭头
            }
            //刻度线
            float col_width = (float)rect_grid.Width / (float)_GridXSpliteCount;
            y1 = rect_axis.Top + _AxesKdSpace;
            y2 = rect_axis.Top;
            for (int i = 0; i < _GridXSpliteCount + 1; ++i)
            {
                int ptx = 0;
                if (i == 0)
                    ptx = rect_axis.X;
                else if (i == _GridYSpliteCount)
                    ptx = rect_axis.Right;
                else
                    ptx = (int)Math.Round(rect_axis.X + col_width * i);
                x1 = x2 = ptx;
                g.DrawLine(pen, x1, y1, x2, y2); //刻度线
            }
        }

        /* 绘制单个X坐标轴名称/单位和刻度线值
         */
        protected virtual void _DrawSingleXAxisVal(Graphics g, Rectangle rect_axis, Rectangle rect_grid, string name_unit,
                                        Font font, Brush brush,
                                        float delta_amount_start, float delta_amount_stop, DataFormat df)
        {
            Point pt_center = new Point(rect_axis.X + rect_axis.Width / 2, rect_axis.Y + _AxesKdSpace + (int)Math.Floor((float)_AxesSpace * 3.0f / 4.0f));
            _DrawXAxisText(g, name_unit, pt_center, 0, font, brush); //名称/单位
            //刻度值
            float v_span = (delta_amount_stop - delta_amount_start) / _GridXSpliteCount;
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
                    v_kd = delta_amount_start;
                    pt_center = new Point(rect_axis.X, kd_center_y);
                }
                else if (i == _GridYSpliteCount)
                {
                    h_lean = 1;
                    v_kd = delta_amount_stop;
                    pt_center = new Point(rect_axis.X + rect_axis.Width, kd_center_y);
                }
                else
                {
                    h_lean = 0;
                    v_kd = delta_amount_start + i * v_span;
                    pt_center = new Point((int)Math.Round(rect_axis.X + col_width * i), kd_center_y);
                }
                string s_kd = df.Format(v_kd);
                _DrawXAxisText(g, s_kd, pt_center, h_lean, font, brush); //刻度值
            }
        }

        /* 获取数据间隔总和的最大值
         */ 
        protected virtual float _GetMaxDeltaAmount()
        {
            float ret = 0.0f;
            _Curves.ForEach((ChartCurve1V cc) =>
            {
                float l = cc.YAxisData.Count * cc.Delta;
                if (l > ret)
                    ret = l;
            });
            return ret;
        }

        /* 绘制X坐标轴
         */
        protected virtual void _DrawXAxis(Graphics g, int width, int height)
        {
            Rectangle rect_grid = _GetGridRect(width, height);
            Rectangle rect_axis = _GetXAxisRect(width, height);
            _DrawSingleXAxis(g, rect_axis, rect_grid, _AxesPen);
            float delta_amount = _GetMaxDeltaAmount();
            float start_delta_amount, stop_delta_amount;
            ChartCurve1V.GetYAxisStartStopDeltaAmountByBilichi(_DataBilichiStart, _DataBilichiStop, delta_amount, out start_delta_amount, out stop_delta_amount);
            _XAxisMinValueUsed = start_delta_amount;
            _XAxisMaxValueUsed = stop_delta_amount;
            string name_unit = XAxisName + (string.IsNullOrEmpty(XAxisUnitName) ? "" : ("（" + XAxisUnitName + "）"));
            DataFormat df = XAxisKdFormat;
            _DrawSingleXAxisVal(g, rect_axis, rect_grid, name_unit, _AxesFont, _AxesBrush, start_delta_amount, stop_delta_amount, df);
        }

        /* 根据Y轴数据的数值和Y坐标轴的最大值/最小值获取像素点的位置
         */
        protected int _GetYAxisPos(float v, float f_max, float f_min, int chart_space)
        {

            if (v >= f_max)
                return 0;
            else if (v <= f_min)
                return chart_space;
            if ((f_max == -f_min) && (v == 0.0f))
                return chart_space / 2;
            if (f_max == f_min)
                return chart_space / 2;
            return (int)Math.Round((float)chart_space * (f_max - v) / (f_max - f_min));
        }

        /* 根据X轴数据的数值和X坐标轴的最大值/最小值获取像素点的位置
         */
        protected int _GetXAxisPos(float v, float f_max, float f_min, int chart_space)
        {

            if (v >= f_max)
                return chart_space;
            else if (v <= f_min)
                return 0;
            if ((f_max == -f_min) && (v == 0.0f))
                return chart_space / 2;
            if (f_max == f_min)
                return chart_space / 2;
            return (int)Math.Round((float)chart_space * (v - f_min) / (f_max - f_min));
        }

        /* 绘制单条曲线
         */
        protected virtual void _DrawSingleCurve(Graphics g, int width, int height, ChartCurve1V cc)
        {
            Rectangle rect_grid = _GetGridRect(width, height);
            int chart_width = rect_grid.Width;
            int chart_height = rect_grid.Height;
            List<Point> pts = new List<Point>();
            List<float> data_y = cc.YAxisData;
            if (data_y.Count < 2) 
                return;
            cc._ChartCursors1V.Clear();
            int start_index, stop_index;
            cc.GetYAxisStartStopIndex(_XAxisMinValueUsed, _XAxisMaxValueUsed, out start_index, out stop_index);
            float y_axis_max, y_axis_min;
            if(YAxisCombined)
            {
                y_axis_max = _YAxisMaxValueUsed;
                y_axis_min = _YAxisMinValueUsed;
            }
            else
            {
                y_axis_max = cc.YAxisMaxValueUsed;
                y_axis_min = cc.YAxisMinValueUsed;
            }
            for (int i = start_index; i <= stop_index && i < data_y.Count; ++i)
            {
                float x_v = i * cc.Delta;
                int pt_x = rect_grid.X + _GetXAxisPos(x_v, _XAxisMaxValueUsed, _XAxisMinValueUsed, rect_grid.Width);

                float y_v = data_y[i];
                int pt_y = rect_grid.Y + _GetYAxisPos(y_v, y_axis_max, y_axis_min, rect_grid.Height);

                pts.Add(new Point(pt_x, pt_y));
                ChartCursor1V cursor = new ChartCursor1V
                {
                    Pt_x = pt_x,
                    Pt_y = pt_y,
                    YValue = y_v,
                    Pt_count = 1,
                    Y_max = y_v,
                    Y_min = y_v,
                    YIndex = i,
                };
                if (!cc._ChartCursors1V.Keys.Contains(pt_x))
                {
                    cc._ChartCursors1V.Add(pt_x, cursor);
                }
                else
                {
                    ChartCursor1V cursor_exist = cc._ChartCursors1V[pt_x];
                    cursor_exist.Pt_count++;
                    cursor_exist.Y_max = (cursor_exist.Y_max >= y_v ? cursor_exist.Y_max : y_v);
                    cursor_exist.Y_min = (cursor_exist.Y_min <= y_v ? cursor_exist.Y_min : y_v);
                }
            }
            if (pts.Count > 1)
            {
                g.DrawLines(cc.Pen, pts.ToArray());
                if(pts.Count < rect_grid.Width / 2)
                {
                    Rectangle r = new Rectangle();
                    r.Width = 4;
                    r.Height = 4;
                    pts.ForEach((Point pt) =>
                    {
                        r.X = pt.X - 2;
                        r.Y = pt.Y - 2;
                        g.DrawRectangle(cc.Pen, r);
                    });
                }
            }
            else if (pts.Count == 1)
                g.DrawRectangle(cc.Pen, new Rectangle(pts[0].X - 2, pts[0].Y - 2, 4, 4));
        }        

        /* 绘制曲线
         */
        protected void _DrawCurves(Graphics g, int width, int height)
        {
            for (int i = 0; i < _Curves.Count; ++i)
            {
                ChartCurve1V cc = _Curves[i];
                _DrawSingleCurve(g, width, height, cc);
            }
        }
           
        /* 重绘整个缓冲区
         */
        protected void _DrawData()
        {
            if (_DataImage != null)
            {
                if(_DataImage.Width != this.ClientRectangle.Width ||
                    _DataImage.Height != this.ClientRectangle.Height)
                {
                    _DataImage.Dispose();
                    _DataImage = null;
                }
            }
            if(_DataImage == null)
            {
                _DataImage = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
            }
            Graphics g = Graphics.FromImage(_DataImage);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(ChartBackColor);
            _DrawGrid(g, _DataImage.Width, _DataImage.Height);
            _DrawYAxis(g, _DataImage.Width, _DataImage.Height);
            _DrawXAxis(g, _DataImage.Width, _DataImage.Height);
            _DrawCurves(g, _DataImage.Width, _DataImage.Height);
            g.Dispose();
            g = null;
        }

        /* 获取最接近的点的游标
         */
        private ChartCursor1V _GetNearestPtCursor(int x, Dictionary<int, ChartCursor1V> cursors)
        {
            if (cursors.ContainsKey(x))
                return cursors[x];
            else
            {
                if (_NEAREST_PIXEL_RANG > 0) //只找当前鼠标所在位置
                {
                    for (int i = 1; i <= _NEAREST_PIXEL_RANG; ++i)
                    {
                        int j = x - i;
                        if (cursors.ContainsKey(j))
                            return cursors[j];
                        j = x + i;
                        if (cursors.ContainsKey(j))
                            return cursors[j];
                    }
                    return null;
                }
                else
                    return null;
            }
        }

        /* 绘制单条曲线的游标
         */
        protected void _DrawCursor(Graphics g, string str, Point point_pt, Font font, Brush brush, Brush brush_back, int width, int height)
        {
            SizeF szf = g.MeasureString(str, font);
            PointF pf_draw = new PointF();
            pf_draw.Y = point_pt.Y - szf.Height / 2.0f;
            if (pf_draw.Y < 0) pf_draw.Y = 0;
            if (pf_draw.Y + szf.Height > height)
                pf_draw.Y = height - szf.Height;
            pf_draw.X = point_pt.X + _CURSOR_HALF_SIDE;
            if (pf_draw.X >= width)
                pf_draw.X = point_pt.X - _CURSOR_HALF_SIDE - szf.Width - 1;
            else
            {
                if(pf_draw.X + szf.Width > width)
                    pf_draw.X = point_pt.X - _CURSOR_HALF_SIDE - szf.Width - 1;
            }
            g.FillRectangle(brush_back, new RectangleF(pf_draw.X, pf_draw.Y, szf.Width, szf.Height));
            g.FillRectangle(brush, new Rectangle(point_pt.X - 2, point_pt.Y - 2, 4, 4));
            g.DrawString(str, font, brush, pf_draw);
        }        

        /* 绘制游标
         */
        protected virtual void _DrawCursors(Graphics g, Point pt)
        {
            for (int ijk = 0; ijk < _Curves.Count; ++ijk)
            {
                ChartCurve1V cc = _Curves[ijk];
                Dictionary<int, ChartCursor1V> cc_cursors = cc._ChartCursors1V;
                ChartCursor1V cursor = _GetNearestPtCursor(pt.X, cc_cursors);
                if (cursor != null)
                {
                    g.FillEllipse(_OutlinePtBrush, cursor.Pt_x - _CURSOR_HALF_SIDE, cursor.Pt_y - _CURSOR_HALF_SIDE, _CURSOR_HALF_SIDE << 1, _CURSOR_HALF_SIDE << 1);
                    string info = "";
                    if(CursorFormat == null)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("X:").Append(XAxisValueFormat.Format(cursor.YIndex * cc.Delta));
                        if (!string.IsNullOrEmpty(XAxisUnitName))
                            sb.Append("(").Append(XAxisUnitName).Append(")");
                        sb.Append("Y:").Append(cc.YAxisValueFormat.Format(cursor.Y_max));
                        if (!string.IsNullOrEmpty(cc.YAxisUnitName))
                            sb.Append("(").Append(cc.YAxisUnitName).Append(")");
                        sb.Append("\r\n");
                        info = sb.ToString();
                        sb.Clear();
                    }
                    else
                        info = CursorFormat.Format<int>(cursor.YIndex);
                    if (!string.IsNullOrEmpty(info))
                        _DrawCursor(g, info, new Point(cursor.Pt_x, cursor.Pt_y), _AxesFont, cc.Brush, _DragRectBrush, _DestImage.Width, _DestImage.Height);
                }
            }               
        }

        /* 将缓冲区绘制到显示区，并绘制游标等
         */
        private void _DrawDest()
        {
            if (_DataImage == null) return;
            if (_DestImage != null)
            {
                if (_DestImage.Width != this.ClientRectangle.Width ||
                    _DestImage.Height != this.ClientRectangle.Height)
                {
                    _DestImage.Dispose();
                    _DestImage = null;
                }
            }
            if (_DestImage == null)
            {
                _DestImage = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
            }
            Graphics g = Graphics.FromImage(_DestImage);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.DrawImage(_DataImage, 0, 0);
            //绘制按住鼠标左键并拖动形成的方框
            Point mp = this.PointToClient(MousePosition);
            if (_MouseDraging && mp.X != _MousePositionStartDrag.X && mp.Y != _MousePositionStartDrag.Y)
            {
                int w = Math.Abs(mp.X - _MousePositionStartDrag.X);
                int h = Math.Abs(mp.Y - _MousePositionStartDrag.Y);
                int x = mp.X > _MousePositionStartDrag.X ? _MousePositionStartDrag.X : mp.X;
                int y = mp.Y > _MousePositionStartDrag.Y ? _MousePositionStartDrag.Y : mp.Y;
                g.FillRectangle(_DragRectBrush, x, y, w, h);
                g.DrawRectangle(_AxesPen, x, y, w, h);
            }
            //游标
            if(_MouseIn)
            {
                int x = mp.X;
                int y = mp.Y;
                if(x >= _MarginSpace + _GetYAxisSpace() && x <= this.ClientRectangle.Width - _MarginSpace &&
                    y>= _MarginSpace + _TitleYSpace && y <= this.ClientRectangle.Height - _MarginSpace - _GetXAxisSpace())
                {
                    _DrawCursors(g, mp);
                }
            }
        }
        
        #endregion        
    }
}
