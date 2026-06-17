using System;
using System.Windows;

namespace LetterClashClient {
  /// <summary>
  /// Lógica de interacción para App.xaml
  /// </summary>
  public partial class App : Application {
    protected override void OnStartup(StartupEventArgs e) {
      base.OnStartup(e);
      Services.LanguageManager.InitializeLanguage();
    }
  }
}
