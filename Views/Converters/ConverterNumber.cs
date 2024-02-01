using System;
using System.Globalization;
using System.Windows.Data;

namespace Apontamento.Views.Converters
{
    public class ConverterNumber : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                //var valorFormatado = string.Format(CultureInfo.GetCultureInfo("pt-BR"), "{0:D}", value);
                var valor = Convert.ToDouble(value) % 1 == 0 ? value : string.Format(CultureInfo.CurrentCulture, "{0:N}", value);
                return valor;
            }
            catch (Exception ex)
            {
            }
            return value;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
