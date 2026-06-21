using System;
using System.Windows;

using LetterClashClient.Models;

namespace LetterClashClient {
  /// <summary>
  /// Lógica de interacción para App.xaml
  /// </summary>
  public partial class App : Application {
    protected override void OnStartup(StartupEventArgs e) {
      base.OnStartup(e);
      Services.LanguageManager.InitializeLanguage();
    }

    protected override void OnExit(ExitEventArgs e) {
      SessionContext.LimpiarSesion();
      base.OnExit(e);
    }
  }
}
