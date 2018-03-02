using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Chart2D;

namespace Demo
{
    public partial class Form1 : Form
    {
        CurvedLineChart1V curvedLineChart1V1;
        CurvedLineChart2V curvedLineChart2V1;

        const int SAMPLE_RATE1 = 100;
        const int SAMPLE_RATE2 = 200;
        const int SAMPLE_RATE3 = 50;

        const int DATA_COUNT = 1000;

        float[] pre_data1 = new float[DATA_COUNT];
        float[] pre_data2 = new float[DATA_COUNT];
        float[] pre_data3 = new float[DATA_COUNT];

        float[] pre_data_y_2v = new float[DATA_COUNT];
        float[] pre_data_x_2v = new float[DATA_COUNT];

        int tick_count = 0;

        //1v
        List<float> data_y_1v_1 = new List<float>();
        List<float> data_y_1v_2 = new List<float>();
        List<float> data_y_1v_3 = new List<float>();
        List<ChartCurve1V> curves1v = new List<ChartCurve1V>();
        
        //2v
        List<float> data_y_2v = new List<float>();
        List<float> data_x_2v = new List<float>();
        List<ChartCurve1V> curves2v = new List<ChartCurve1V>();

        public Form1()
        {
            InitializeComponent();

            for(int i = 0; i < DATA_COUNT; ++i)
            {
                pre_data1[i] = 250.0f * (float)Math.Sin(2.0D * Math.PI * 2.0D * (double)i / (double)SAMPLE_RATE1);
                pre_data2[i] = 100.0f + 100.0f * (float)Math.Sin(2.0D * Math.PI * 4.0D * (double)i / (double)SAMPLE_RATE2);
                pre_data3[i] = 100.0f + 100.0f * (float)Math.Sin(2.0D * Math.PI * 1.0D * (double)i / (double)SAMPLE_RATE3);

                pre_data_y_2v[i] = (float)Math.Sin(2.0D * Math.PI * 0.1D * (double)i / (double)SAMPLE_RATE1);
                pre_data_x_2v[i] = (float)Math.Cos(2.0D * Math.PI * 0.1D * (double)i / (double)SAMPLE_RATE1);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //可以通过代码创建控件，也可以通过工具箱创建控件

            curvedLineChart1V1 = new CurvedLineChart1V(); 
            curvedLineChart1V1.Parent = splitContainer1.Panel1;
            curvedLineChart1V1.Dock = DockStyle.Fill;
            curvedLineChart1V1.TitleYSpace = 40;
            curvedLineChart1V1.Title = "电压电流实时数据曲线";
            curvedLineChart1V1.YAxisName = "电压";
            curvedLineChart1V1.YAxisUnitName = "伏特";
            curvedLineChart1V1.YAxisCombined = false;
            curvedLineChart1V1.YAxisIsAuto = true;
            curvedLineChart1V1.XAxisName = "时间";
            curvedLineChart1V1.XAxisUnitName = "时:分:秒.毫秒";
            DataFormat df = new DataFormat(EDataFormat.FORMAT_FUNC, new Func<float, string>((v) =>
            {
                int total_secs = (int)Math.Floor(v);
                int msec = (int)Math.Round((v - total_secs) * 1000);
                int hours = total_secs / 3600;
                total_secs = total_secs % 3600;
                int minutes = total_secs / 60;
                int secs = total_secs % 60;
                if(hours > 0)                    
                    return string.Format("{0}:{1}:{2}.{3:000}", hours, minutes, secs, msec);
                else if (minutes > 0)
                    return string.Format("{1}:{2}.{3:000}", hours, minutes, secs, msec);
                else
                    return string.Format("{2}.{3:000}", hours, minutes, secs, msec);
            }));
            curvedLineChart1V1.XAxisKdFormat = df;
            curvedLineChart1V1.XAxisValueFormat = df;

            curvedLineChart2V1 = new CurvedLineChart2V();
            curvedLineChart2V1.Parent = splitContainer1.Panel2;
            curvedLineChart2V1.Dock = DockStyle.Fill;
            curvedLineChart2V1.TitleYSpace = 40;
            curvedLineChart2V1.Title = "电压电流轨迹曲线";
            curvedLineChart2V1.YAxisName = "电压";
            curvedLineChart2V1.YAxisUnitName = "伏特";
            curvedLineChart2V1.YAxisCombined = false;
            curvedLineChart2V1.YAxisIsAuto = false;
            curvedLineChart2V1.XAxisName = "电流";
            curvedLineChart2V1.XAxisUnitName = "安培";
            curvedLineChart2V1.XAxisCombined = false;
            curvedLineChart2V1.XAxisIsAuto = false;

            //给curves1v添加3条数据线

            ChartCurve1V cc1v = new ChartCurve1V();
            cc1v.Color = Color.Red;
            cc1v.Delta = 1.0f / SAMPLE_RATE1;
            cc1v.YAxisName = "实时电压数据轴";
            cc1v.YAxisUnitName = "V";
            cc1v.YAxisIsAuto = true;//false;
            cc1v.YAxisMaxValue = 250;
            cc1v.YAxisMinValue = -250;
            cc1v.YAxisData = data_y_1v_1;
            df = new DataFormat(EDataFormat.FORMAT_FUNC, new Func<float, string>((v) => {
                int iv = (int)Math.Round(v);
                return string.Format("{0:0.0}", iv);
            }));
            cc1v.YAxisKdFormat = df;
            cc1v.YAxisValueFormat = df;
            curves1v.Add(cc1v);

            cc1v = new ChartCurve1V();
            cc1v.Color = Color.Blue;
            cc1v.Delta = 1.0f / SAMPLE_RATE2;
            cc1v.YAxisName = "功率数据轴";
            cc1v.YAxisUnitName = "dB";
            cc1v.YAxisIsAuto = true;//false;
            cc1v.YAxisMaxValue = 100;
            cc1v.YAxisMinValue = 0;
            cc1v.YAxisData = data_y_1v_2;
            df = new DataFormat(EDataFormat.FORMAT_FUNC, new Func<float, string>((v) =>
            {
                int iv = (int)Math.Round(v);
                return string.Format("{0:#0.0#}", iv);
            }));
            cc1v.YAxisKdFormat = df;
            cc1v.YAxisValueFormat = df;
            curves1v.Add(cc1v);

            cc1v = new ChartCurve1V();
            cc1v.Color = Color.Green;
            cc1v.Delta = 1.0f / SAMPLE_RATE3;
            cc1v.YAxisName = "电流";
            cc1v.YAxisUnitName = "A";
            cc1v.YAxisIsAuto = true;//false;
            cc1v.YAxisMaxValue = 100;
            cc1v.YAxisMinValue = 0;
            cc1v.YAxisData = data_y_1v_3;
            cc1v.YAxisKdFormat = df;
            cc1v.YAxisValueFormat = df;
            curves1v.Add(cc1v);

            curvedLineChart1V1.Curves = curves1v;

            //给curves2v添加1条轨迹
            ChartCurve2V cc2v = new ChartCurve2V();
            cc2v.Color = Color.Red;
            cc2v.Delta = 1.0f / SAMPLE_RATE1;
            cc2v.YAxisName = "电压";
            cc2v.YAxisUnitName = "V";
            cc2v.YAxisIsAuto = true;//false;
            cc2v.YAxisMaxValue = 1;
            cc2v.YAxisMinValue = -1;
            cc2v.YAxisData = data_y_2v;
            cc2v.XAxisName = "电流";
            cc2v.XAxisUnitName = "A";
            cc2v.XAxisIsAuto = true;
            cc2v.XAxisData = data_x_2v;
            cc2v.XAxisMaxValue = 1;
            cc2v.XAxisMinValue = -1;
            df = new DataFormat(EDataFormat.FORMAT_FUNC, new Func<float, string>((v) =>
            {
                return string.Format("{0:0.0#####}", v);
            }));
            cc2v.YAxisKdFormat = df;
            cc2v.YAxisValueFormat = df;
            df = new DataFormat(EDataFormat.FORMAT_FUNC, new Func<float, string>((v) =>
            {
                return string.Format("{0:0.0#####}", v);
            }));
            cc2v.XAxisKdFormat = df;
            cc2v.XAxisValueFormat = df;
            curves2v.Add(cc2v);
            curvedLineChart2V1.Curves = curves2v;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //timer1.Interval == 100;
            //STEP=1/10秒

            //SAMPLE_RATE1=100
            //STEP1=10

            //SAMPLE_RATE1=200
            //STEP2=20

            //SAMPLE_RATE3=50
            //STEP3=5

            if(data_y_1v_1.Count < DATA_COUNT)
            {
                int index_start = tick_count * 10;
                int index_stop = index_start + 10;
                for (int i = index_start; i < index_stop; ++i)
                    data_y_1v_1.Add(pre_data1[i]);
                curvedLineChart1V1.DataChanged = true;
            }
            if (data_y_1v_2.Count < DATA_COUNT)
            {
                int index_start = tick_count * 20;
                int index_stop = index_start + 20;
                for (int i = index_start; i < index_stop; ++i)
                    data_y_1v_2.Add(pre_data2[i]);
                curvedLineChart1V1.DataChanged = true;
            }
            if (data_y_1v_3.Count < DATA_COUNT)
            {
                int index_start = tick_count * 5;
                int index_stop = index_start + 5;
                for (int i = index_start; i < index_stop; ++i)
                    data_y_1v_3.Add(pre_data3[i]);
                curvedLineChart1V1.DataChanged = true;
            }

            if (data_y_2v.Count < DATA_COUNT)
            {
                int index_start = tick_count * 10;
                int index_stop = index_start + 10;
                for (int i = index_start; i < index_stop; ++i)
                {
                    data_y_2v.Add(pre_data_y_2v[i]);
                    data_x_2v.Add(pre_data_x_2v[i]);
                }
                curvedLineChart2V1.DataChanged = true;
            }

            tick_count++;
        }        
    }
}
