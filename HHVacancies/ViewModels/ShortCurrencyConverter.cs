using System;
using System.Globalization;
using System.Windows.Data;

namespace HHVacancies.ViewModels
{
    /// <summary>
    /// Преобразовывает числовое значение в денежное без учета дробных частей
    /// </summary>
    public class ShortCurrencyConverter : IValueConverter
    {
        /// <summary>
        /// Конвертировать значение в строку
        /// </summary>
        public object Convert(object value, Type type, object param, CultureInfo ci)
        {
            if (type != typeof(string))
                throw new InvalidCastException();

            string currencyValue = ((int)value).ToString("C");
            var roubleSymbol = new CultureInfo("RU-ru").NumberFormat.CurrencySymbol;
            string shortValue = currencyValue.Substring(0, currencyValue.IndexOf(','));

            return String.Concat(shortValue, ' ', roubleSymbol);
        }

        public object ConvertBack(object value, Type type, object param, CultureInfo ci)
        {
            throw new NotImplementedException();
        }
    }
}
