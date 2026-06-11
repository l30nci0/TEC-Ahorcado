using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

using LetterClashClient.Models;
using LetterClashClient.Services;
using LetterClashServer.Domain.Models;

namespace LetterClashClient.Views {
  public partial class Login : Page {
    public Login() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = "Ingreso de Sesión";
      }
    }

    private void ButtonLogin_Click(object sender, RoutedEventArgs e) {
      TextBlockUsernameError.Visibility = Visibility.Hidden;
      TextBlockEmailError.Visibility = Visibility.Hidden;
      TextBlockPasswordError.Visibility = Visibility.Hidden;

      bool usernameEmpty = string.IsNullOrWhiteSpace(TextBoxUsername.Text);
      bool emailEmpty = string.IsNullOrWhiteSpace(TextBoxEmail.Text);
      bool passwordEmpty = string.IsNullOrWhiteSpace(PasswordBoxPassword.Password);

      bool identityValid = !usernameEmpty || !emailEmpty;
      bool hasError = false;

      if (!identityValid) {
        TextBlockUsernameError.Text = "Ingrese su nombre de usuario o correo.";
        TextBlockUsernameError.Visibility = Visibility.Visible;
        TextBlockEmailError.Text = "Ingrese su nombre de usuario o correo.";
        TextBlockEmailError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (passwordEmpty) {
        TextBlockPasswordError.Text = "Coloque su contraseña.";
        TextBlockPasswordError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (hasError) {
        return;
      }

      string identityInput = !usernameEmpty ? TextBoxUsername.Text.Trim() : TextBoxEmail.Text.Trim();
      string passwordInput = PasswordBoxPassword.Password;

      try {
        var authService = ServiceProxyManager.GetAutenticacionService();
        var result = authService.IniciarSesion(identityInput, passwordInput);

        if (result != null && result.IsSuccess) {
          SessionContext.UsuarioLogueado = result.Value;
          NavigationService.Navigate(new MainMenu());
        } else {
          string errorMsg = result?.Error?.Mensaje ?? "Credenciales incorrectas o error en el sistema.";
          if (result?.Error?.CodigoError == CodigoError.CREDENCIALES_INVALIDAS) {
            TextBlockPasswordError.Text = "Usuario o contraseña incorrectos.";
            TextBlockPasswordError.Visibility = Visibility.Visible;
          } else {
            MessageBox.Show(errorMsg, "Error de Inicio de Sesión", MessageBoxButton.OK, MessageBoxImage.Error);
          }
        }
      } catch (CommunicationException) {
        MessageBox.Show("No se pudo establecer conexión con el servidor. Compruebe que el servidor esté activo.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
      } catch (Exception ex) {
        MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void ButtonCreateAccount_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new RegisterAccount());
    }

    private void ButtonBackHome_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new EntryMenu());
    }

    private void TextBoxUsername_TextChanged(object sender, TextChangedEventArgs e) {
      TextBlockUsernamePlaceholder.Visibility = string.IsNullOrWhiteSpace(TextBoxUsername.Text) ? Visibility.Visible : Visibility.Hidden;
    }

    private void TextBoxEmail_TextChanged(object sender, TextChangedEventArgs e) {
      TextBlockEmailPlaceholder.Visibility = string.IsNullOrWhiteSpace(TextBoxEmail.Text) ? Visibility.Visible : Visibility.Hidden;
    }

    private void PasswordBoxPassword_PasswordChanged(object sender, RoutedEventArgs e) {
      TextBlockPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(PasswordBoxPassword.Password) ? Visibility.Visible : Visibility.Hidden;
    }
  }
}
