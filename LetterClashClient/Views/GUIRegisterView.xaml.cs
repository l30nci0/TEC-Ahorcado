using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

using LetterClashClient.Models;
using LetterClashClient.Services;
using LetterClashServer.Domain.Models;

namespace LetterClashClient.Views {
  public partial class GUIRegisterView : Page {
    public GUIRegisterView() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = "Crear Cuenta";
      }
    }

    private void ButtonCreate_Click(object sender, RoutedEventArgs e) {
      TextBlockFullNameError.Visibility = Visibility.Hidden;
      ButtonConfirmName.Visibility = Visibility.Hidden;
      TextBlockEmailError.Visibility = Visibility.Hidden;
      TextBlockPhoneError.Visibility = Visibility.Hidden;
      TextBlockPasswordError.Visibility = Visibility.Hidden;
      TextBlockBirthDateError.Visibility = Visibility.Hidden;
      TextBlockUsernameError.Visibility = Visibility.Hidden;
      TextBlockLanguageError.Visibility = Visibility.Hidden;

      bool hasError = false;

      if (string.IsNullOrWhiteSpace(TextBoxFullName.Text)) {
        TextBlockFullNameError.Visibility = Visibility.Visible;
        ButtonConfirmName.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (string.IsNullOrWhiteSpace(TextBoxEmail.Text) || !TextBoxEmail.Text.Contains("@")) {
        TextBlockEmailError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (string.IsNullOrWhiteSpace(TextBoxPhone.Text) || !long.TryParse(TextBoxPhone.Text, out _)) {
        TextBlockPhoneError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (PasswordBoxPassword.Password != PasswordBoxConfirmPassword.Password || string.IsNullOrWhiteSpace(PasswordBoxPassword.Password)) {
        TextBlockPasswordError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (DatePickerBirthDate.SelectedDate == null) {
        TextBlockBirthDateError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (string.IsNullOrWhiteSpace(TextBoxUsername.Text)) {
        TextBlockUsernameError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (ComboBoxPreferredLanguage.SelectedIndex <= 0) {
        TextBlockLanguageError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (hasError) {
        return;
      }

      var nuevoJugador = new JugadorDTO {
        Nombre = TextBoxFullName.Text.Trim(),
        NombreDeUsuario = TextBoxUsername.Text.Trim(),
        Correo = TextBoxEmail.Text.Trim(),
        Telefono = TextBoxPhone.Text.Trim(),
        FechaDeNacimiento = DatePickerBirthDate.SelectedDate.Value,
        IdiomaPreferido = ComboBoxPreferredLanguage.SelectedIndex == 1 ? Idiomas.INGLES : Idiomas.ESPANOL,
        Puntuacion = 0,
        Avatar = null
      };

      string password = PasswordBoxPassword.Password;

      try {
        var authService = ServiceProxyManager.GetAutenticacionService();
        var registerResult = authService.RegistrarJugador(nuevoJugador, password);

        if (registerResult == null || !registerResult.IsSuccess || registerResult.Error != null) {
          ManejarErrorRegistro(registerResult?.Error);
          return;
        }

        MessageBox.Show("Cuenta creada con éxito. Inicie sesión para comenzar.", "Registro Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
        NavigationService.Navigate(new GUILoginView());
      } catch (CommunicationException) {
        MessageBox.Show("No se pudo establecer conexión con el servidor. Compruebe que el servidor esté activo.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
      } catch (Exception ex) {
        MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void ManejarErrorRegistro(ServiceFault error) {
      if (error == null) {
        MessageBox.Show("Error al registrar la cuenta.", "Error de Registro", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      if (error.CodigoError == CodigoError.RECURSO_DUPLICADO) {
        if (error.Detalle != null && error.Detalle.Contains("Correo")) {
          TextBlockEmailError.Text = error.Mensaje;
          TextBlockEmailError.Visibility = Visibility.Visible;
        } else if (error.Detalle != null && error.Detalle.Contains("Nombre de usuario")) {
          TextBlockUsernameError.Text = error.Mensaje;
          TextBlockUsernameError.Visibility = Visibility.Visible;
        } else {
          MessageBox.Show(error.Mensaje, "Error de Registro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      } else if (error.CodigoError == CodigoError.PARAMETRO_INVALIDO) {
        if (error.Mensaje != null && (error.Mensaje.Contains("usuario") || error.Mensaje.Contains("Nombre de usuario"))) {
          TextBlockUsernameError.Text = error.Mensaje;
          TextBlockUsernameError.Visibility = Visibility.Visible;
        } else if (error.Mensaje != null && (error.Mensaje.Contains("correo") || error.Mensaje.Contains("Correo"))) {
          TextBlockEmailError.Text = error.Mensaje;
          TextBlockEmailError.Visibility = Visibility.Visible;
        } else {
          MessageBox.Show(error.Mensaje, "Error de Registro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      } else {
        MessageBox.Show(error.Mensaje, "Error de Registro", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUILoginView());
    }

    private void ButtonAddAvatar_Click(object sender, RoutedEventArgs e) {
      MessageBox.Show("Aquí se podrá seleccionar una imagen de avatar.",
                      "TecnoHorcado",
                      MessageBoxButton.OK,
                      MessageBoxImage.Information);
    }

    private void TextBoxUsername_TextChanged(object sender, TextChangedEventArgs e) {
      TextBlockUsernamePlaceholder.Visibility = string.IsNullOrWhiteSpace(TextBoxUsername.Text) ? Visibility.Visible : Visibility.Hidden;
    }

    private void TextBoxFullName_TextChanged(object sender, TextChangedEventArgs e) {
      TextBlockFullNamePlaceholder.Visibility = string.IsNullOrWhiteSpace(TextBoxFullName.Text) ? Visibility.Visible : Visibility.Hidden;
    }

    private void TextBoxEmail_TextChanged(object sender, TextChangedEventArgs e) {
      TextBlockEmailPlaceholder.Visibility = string.IsNullOrWhiteSpace(TextBoxEmail.Text) ? Visibility.Visible : Visibility.Hidden;
    }

    private void TextBoxPhone_TextChanged(object sender, TextChangedEventArgs e) {
      TextBlockPhonePlaceholder.Visibility = string.IsNullOrWhiteSpace(TextBoxPhone.Text) ? Visibility.Visible : Visibility.Hidden;
    }

    private void PasswordBoxPassword_PasswordChanged(object sender, RoutedEventArgs e) {
      TextBlockPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(PasswordBoxPassword.Password) ? Visibility.Visible : Visibility.Hidden;
    }

    private void PasswordBoxConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e) {
      TextBlockConfirmPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(PasswordBoxConfirmPassword.Password) ? Visibility.Visible : Visibility.Hidden;
    }
  }
}
