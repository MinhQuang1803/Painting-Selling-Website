using System.Collections.Generic;
namespace SEP490_G28_SP24_WebSellingPaint.FunctionCode
{
    public class Dict : Dictionary<string, string>
    {
        public new string this[string key]
        {
            get
            {
                return ContainsKey(key) ? base[key] : "";
            }
            set
            {
                base[key] = value;
            }
        }
    }
}
