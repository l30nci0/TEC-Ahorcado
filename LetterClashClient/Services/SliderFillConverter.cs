using System;
using System.Globalization;
using System.Windows.Data;

namespace LetterClashClient.Services {
  /// <summary>
  /// Convierte el valor del slider (0-100) y el ancho actual del control
  /// en un ancho absoluto para la barra de llenado.
  /// </summary>
  public class SliderFillConverter : IMultiValueConverter {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
      if (values.Length < 2) return 0.0;

      if (values[0] is double value && values[1] is double actualWidth) {
        return Math.Max(0.0, (value / 100.0) * actualWidth);
      }

      return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}