using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chart2D
{
    public enum EDataFormat
    {
        FORMAT_STRING = 1, //通过格式化字符串
        FORMAT_FUNC = 2, //通过格式化函数
    }

    public class DataFormat : Object
    {
        private EDataFormat _type;
        private string _format_string;
        private Func<float, string> _format_func_float;
        private Func<int, string> _format_func_int;

        public DataFormat(EDataFormat type, object par)
        {
            SetType(type, par);
        }

        public EDataFormat GetDataFormatType { get { return _type; } }
        public bool SetType(EDataFormat type, object par)
        {
            bool ret = true;
            switch (type)
            {
                case EDataFormat.FORMAT_STRING:
                    try 
                    {
                        string s = par as string; 
                        _format_string = s;
                        _type = type;
                    }
                    catch { ret = false; }

                    break;
                case EDataFormat.FORMAT_FUNC:
                    try 
                    { 
                        Func<float, string> ff = par as Func<float, string>;
                        if(ff != null)
                            _format_func_float = ff;
                        else
                        {
                            Func<int, string> fi = par as Func<int, string>;
                            _format_func_int = fi;
                        }                        
                        _type = type;
                    }
                    catch { ret = false; }
                    break;
            }
            return ret;
        }        

        public string Format<T>(T v)
        {
            string ret = "";
            switch (_type)
            {
                case EDataFormat.FORMAT_STRING:
                    try 
                    {
                        if (string.IsNullOrEmpty(_format_string))
                            throw new Exception("");
                        ret = string.Format(_format_string, v);
                    }
                    catch { }
                    break;
                case EDataFormat.FORMAT_FUNC:
                    try 
                    {
                        object o = v;
                        if (o.GetType() == typeof(float))
                        {
                            if (_format_func_float == null)
                                throw new Exception("无合适格式化函数！");
                            ret = _format_func_float((float)o);
                        }
                        if (o.GetType() == typeof(int))
                        {
                            if (_format_func_int == null)
                                throw new Exception("无合适格式化函数！");
                            ret = _format_func_int((int)o);
                        }
                        else
                            throw new Exception("无合适格式化函数！");
                    }
                    catch { }
                    break;
            }
            return ret;
        }
    }
}
