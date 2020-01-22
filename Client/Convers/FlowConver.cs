using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Client.Convers
{
    /// <summary>
    /// 流量转换器
    /// </summary>
    public class FlowConver : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int i = System.Convert.ToInt32(value);
            if (i < 1000) return i.ToString() + "B";
            if (i < 1000 * 1024) return (i / 1024D).ToString("#.##") + "KB";
            if (i < 1000 * 1024 * 1024) return (i / 1024D / 1024D).ToString("#.##") + "MB";
            else return (i / 1024D / 1024D / 1024D).ToString("#.##") + "GB";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
