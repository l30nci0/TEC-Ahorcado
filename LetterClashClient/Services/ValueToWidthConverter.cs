using System;
using System.Globalization;
using System.Windows.Data;

namespace LetterClashClient.Services {
  /// <summary>
  /// Converter que convierte un valor de slider (0-100) a un ancho proporcional (0.0-1.0).
  /// Se usa para mostrar el progreso del volumen en la barra deslizante.
  /// </summary>
  public class ValueToWidthConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value is double val && targetType == typeof(double)) {
        return val / 100.0;
      }
      return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value is double val && targetType == typeof(double)) {
        return val * 100.0;
      }
      return 0.0;
    }
  }
}