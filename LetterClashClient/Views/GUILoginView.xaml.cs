using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

using LetterClashClient.Models;
using LetterClashClient.Properties;
using LetterClashClient.Services;

using LetterClashServer.Domain.Models;

namespace LetterClashClient.Views {
  public partial class GUILoginView : Page {
    public GUILoginView() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = (string) Application.Current.FindResource("Login_WindowTitle") ?? "Ingreso de Sesión";
      }
    }

    private void ButtonLogin_Click(object sender, RoutedEventArgs e) {
      TextBlockUsernameError.Visibility = Visibility.Hidden;
      TextBlockPasswordError.Visibility = Visibility.Hidden;

      bool usernameEmpty = string.IsNullOrWhiteSpace(TextBoxUsername.Text);
      bool passwordEmpty = string.IsNullOrWhiteSpace(PasswordBoxPassword.Password);
      bool hasError = false;

      if (usernameEmpty) {
        TextBlockUsernameError.Text = (string) Application.Current.FindResource("Login_UsernameErrorEmpty") ?? "Ingrese su nombre de usuario o correo.";
        TextBlockUsernameError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (passwordEmpty) {
        TextBlockPasswordError.Text = (string) Application.Current.FindResource("Login_PasswordErrorEmpty") ?? "Coloque su contraseña.";
        TextBlockPasswordError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (hasError) {
        return;
      }

      string identityInput = TextBoxUsername.Text.Trim();
      string passwordInput = PasswordBoxPassword.Password;

      try {
        var authService = ServiceProxyManager.GetAutenticacionService();
        var result = authService.IniciarSesion(identityInput, passwordInput);

        if (result != null && result.IsSuccess) {
          SessionContext.UsuarioLogueado = result.Value;
          MostrarAvisoPenalizacionPendiente();
          NavigationService.Navigate(new GUIGameHubView());
        } else {
          string errorMsg = result?.Error?.Mensaje ?? "Credenciales incorrectas o error en el sistema.";
          if (result?.Error?.CodigoError == CodigoError.CREDENCIALES_INVALIDAS) {
            TextBlockPasswordError.Text = (string) Application.Current.FindResource("Login_PasswordErrorIncorrect") ?? "Usuario o contraseña incorrectos.";
            TextBlockPasswordError.Visibility = Visibility.Visible;
          } else {
            string errTitle = (string) Application.Current.FindResource("Login_MsgBoxErrorTitle") ?? "Error de Inicio de Sesión";
            MessageBox.Show(errorMsg, errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
          }
        }
      } catch (CommunicationException) {
        string connMsg = (string) Application.Current.FindResource("Msg_ConnectionError") ?? "No se pudo establecer conexión con el servidor. Compruebe que el servidor esté activo.";
        string connTitle = (string) Application.Current.FindResource("Msg_ConnectionErrorTitle") ?? "Error de Conexión";
        MessageBox.Show(connMsg, connTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      } catch (Exception ex) {
        string unexpMsg = (string) Application.Current.FindResource("Msg_UnexpectedError") ?? "Ocurrió un error inesperado:";
        string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        MessageBox.Show($"{unexpMsg} {ex.Message}", errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void ButtonCreateAccount_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIRegisterView());
    }

    private void MostrarAvisoPenalizacionPendiente() {
      try {
        if (!Settings.Default.PenalizacionAbandonoPendiente) {
          return;
        }

        Settings.Default.PenalizacionAbandonoPendiente = false;
        Settings.Default.Save();

        string message = (string) Application.Current.FindResource("Game_PenaltyNotice") ?? "Se te penalizo con -3 puntos por abandonar una partida.";
        string title = (string) Application.Current.FindResource("Game_ResultTitle") ?? "Partida Finalizada";
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
      } catch { }
    }

    private void ButtonBackHome_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIWelcomeView());
    }

    private void TextBoxUsername_TextChanged(object sender, TextChangedEventArgs e) {
      TextBlockUsernamePlaceholder.Visibility = string.IsNullOrWhiteSpace(TextBoxUsername.Text) ? Visibility.Visible : Visibility.Hidden;
    }

    private void PasswordBoxPassword_PasswordChanged(object sender, RoutedEventArgs e) {
      TextBlockPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(PasswordBoxPassword.Password) ? Visibility.Visible : Visibility.Hidden;
    }
  }
}
