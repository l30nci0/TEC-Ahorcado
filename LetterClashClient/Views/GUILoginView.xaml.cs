using System;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

using LetterClashClient.Models;
using LetterClashClient.Properties;
using LetterClashClient.Services;

using LetterClashServer.Domain.Models;

namespace LetterClashClient.Views {
  public partial class GUILoginView : Page {
    private bool isPasswordVisible;
    private bool isSyncingPassword;

    public GUILoginView() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = (string) Application.Current.FindResource("Login_WindowTitle") ?? "Ingreso de Sesión";
      }
    }

    private bool IsValidPassword(string password) {
      return !string.IsNullOrEmpty(password) &&
             Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{6,15}$");
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
      } else if (!IsValidPassword(PasswordBoxPassword.Password)) {
        TextBlockPasswordError.Text = (string) Application.Current.FindResource("Login_PasswordFormatError") ?? "Contraseña: 6-15, mayúscula, minúscula, número y símbolo.";
        TextBlockPasswordError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (hasError) {
        return;
      }

      string identityInput = TextBoxUsername.Text.Trim();
      string passwordInput = PasswordBoxPassword.Password;
      var usuarioActual = SessionContext.UsuarioLogueado;
      if (usuarioActual != null &&
          (string.Equals(usuarioActual.NombreDeUsuario, identityInput, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(usuarioActual.Correo, identityInput, StringComparison.OrdinalIgnoreCase))) {
        TextBlockPasswordError.Text = "Este usuario ya tiene una sesión iniciada.";
        TextBlockPasswordError.Visibility = Visibility.Visible;
        return;
      }

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

      if (isPasswordVisible && !isSyncingPassword) {
        isSyncingPassword = true;
        TextBoxPasswordVisible.Text = PasswordBoxPassword.Password;
        isSyncingPassword = false;
      }
    }

    private void TextBoxPasswordVisible_TextChanged(object sender, TextChangedEventArgs e) {
      if (!isPasswordVisible || isSyncingPassword) {
        return;
      }

      isSyncingPassword = true;
      PasswordBoxPassword.Password = TextBoxPasswordVisible.Text;
      isSyncingPassword = false;
      TextBlockPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(TextBoxPasswordVisible.Text) ? Visibility.Visible : Visibility.Hidden;
    }

    private void ButtonTogglePasswordVisibility_Click(object sender, RoutedEventArgs e) {
      isPasswordVisible = !isPasswordVisible;

      if (isPasswordVisible) {
        TextBoxPasswordVisible.Text = PasswordBoxPassword.Password;
        TextBoxPasswordVisible.Visibility = Visibility.Visible;
        PasswordBoxPassword.Visibility = Visibility.Collapsed;
        TextBoxPasswordVisible.Focus();
        TextBoxPasswordVisible.CaretIndex = TextBoxPasswordVisible.Text.Length;
      } else {
        PasswordBoxPassword.Password = TextBoxPasswordVisible.Text;
        PasswordBoxPassword.Visibility = Visibility.Visible;
        TextBoxPasswordVisible.Visibility = Visibility.Collapsed;
        PasswordBoxPassword.Focus();
      }
    }
  }
}
