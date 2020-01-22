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
    /// 显示转换器
    /// </summary>
    public class EnableConver : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (parameter == null ? 0 : System.Convert.ToInt32(parameter)) == System.Convert.ToInt32(value) ;



        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
