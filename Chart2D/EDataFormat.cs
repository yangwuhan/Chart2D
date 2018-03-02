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
        private Func<float, string> _format_func;

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
                        Func<float, string> f = par as Func<float, string>;
                        _format_func = f;
                        _type = type;
                    }
                    catch { ret = false; }
                    break;
            }
            return ret;
        }        

        public string Format(float v)
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
                        if (_format_func == null)
                            throw new Exception("");
                        ret = _format_func(v);
                    }
                    catch { }
                    break;
            }
            return ret;
        }
    }
}
