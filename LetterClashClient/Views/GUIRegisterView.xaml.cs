using System;
using System.ServiceModel;
using System.Text.RegularExpressions;
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
    private bool IsValidPhone(string phone) {
      return !string.IsNullOrWhiteSpace(phone) && Regex.IsMatch(phone.Trim(), @"^[0-9]{10,}$");
    }

    private bool IsValidMinimumAge(DateTime birthDate) {
      return birthDate.Date <= DateTime.Today.AddYears(-3);
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = (string) Application.Current.FindResource("Register_WindowTitle") ?? "Crear Cuenta";
      }

      // Sincronizar UI de botones según el idioma cargado actual
      if (Services.LanguageManager.CurrentLanguage == "EN") {
        ComboBoxPreferredLanguage.SelectedIndex = 1; // Ingles
        ButtonLangES.Style = (Style) FindResource("ModernSecondaryButton");
        ButtonLangEN.Style = (Style) FindResource("ModernPrimaryButton");
      } else {
        ComboBoxPreferredLanguage.SelectedIndex = 2; // Español
        ButtonLangES.Style = (Style) FindResource("ModernPrimaryButton");
        ButtonLangEN.Style = (Style) FindResource("ModernSecondaryButton");
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

      string phone = TextBoxPhone.Text.Trim();

      if (!IsValidPhone(phone)) {
        TextBlockPhoneError.Text = (string) Application.Current.FindResource("Register_PhoneError") ?? "Ingrese mínimo 10 dígitos numéricos.";
        TextBlockPhoneError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (PasswordBoxPassword.Password != PasswordBoxConfirmPassword.Password || string.IsNullOrWhiteSpace(PasswordBoxPassword.Password)) {
        TextBlockPasswordError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (DatePickerBirthDate.SelectedDate == null) {
        TextBlockBirthDateError.Text = (string) Application.Current.FindResource("Register_BirthDateError") ?? "Seleccione una fecha válida.";
        TextBlockBirthDateError.Visibility = Visibility.Visible;
        hasError = true;
      } else if (!IsValidMinimumAge(DatePickerBirthDate.SelectedDate.Value)) {
        TextBlockBirthDateError.Text = (string) Application.Current.FindResource("Register_MinimumAgeError") ?? "El jugador debe tener al menos 3 años.";
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
        Telefono = phone,
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

        string successMsg = (string) Application.Current.FindResource("Register_SuccessMsg") ?? "Cuenta creada con éxito. Inicie sesión para comenzar.";
        string successTitle = (string) Application.Current.FindResource("Register_SuccessTitle") ?? "Registro Exitoso";
        MessageBox.Show(successMsg, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
        NavigationService.Navigate(new GUILoginView());
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

    private void ManejarErrorRegistro(ServiceFault error) {
      string registerErrTitle = (string) Application.Current.FindResource("Register_ErrorTitle") ?? "Error de Registro";
      if (error == null) {
        string errGeneric = (string) Application.Current.FindResource("Register_ErrorGeneric") ?? "Error al registrar la cuenta.";
        MessageBox.Show(errGeneric, registerErrTitle, MessageBoxButton.OK, MessageBoxImage.Error);
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
          MessageBox.Show(error.Mensaje, registerErrTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
      } else if (error.CodigoError == CodigoError.PARAMETRO_INVALIDO) {
        if (error.Mensaje != null && (error.Mensaje.Contains("usuario") || error.Mensaje.Contains("Nombre de usuario"))) {
          TextBlockUsernameError.Text = error.Mensaje;
          TextBlockUsernameError.Visibility = Visibility.Visible;
        } else if (error.Mensaje != null && (error.Mensaje.Contains("correo") || error.Mensaje.Contains("Correo"))) {
          TextBlockEmailError.Text = error.Mensaje;
          TextBlockEmailError.Visibility = Visibility.Visible;
        } else {
          MessageBox.Show(error.Mensaje, registerErrTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
      } else {
        MessageBox.Show(error.Mensaje, registerErrTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUILoginView());
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

    private void ButtonLangES_Click(object sender, RoutedEventArgs e) {
      ComboBoxPreferredLanguage.SelectedIndex = 2; // Español
      ButtonLangES.Style = (Style) FindResource("ModernPrimaryButton");
      ButtonLangEN.Style = (Style) FindResource("ModernSecondaryButton");
      Services.LanguageManager.SetLanguage("ES");
    }

    private void ButtonLangEN_Click(object sender, RoutedEventArgs e) {
      ComboBoxPreferredLanguage.SelectedIndex = 1; // Ingles
      ButtonLangES.Style = (Style) FindResource("ModernSecondaryButton");
      ButtonLangEN.Style = (Style) FindResource("ModernPrimaryButton");
      Services.LanguageManager.SetLanguage("EN");
    }
  }
}
