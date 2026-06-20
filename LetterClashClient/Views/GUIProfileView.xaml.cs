using System;
using System.IO;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using LetterClashClient.Models;
using LetterClashClient.Services;

using LetterClashServer.Domain.Models;

namespace LetterClashClient.Views {
  public partial class GUIProfileView : Page {

    public GUIProfileView() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = (string) Application.Current.FindResource("Profile_WindowTitle") ?? "Perfil de Usuario";
      }

      LoadViewData();
      SwitchMode(false);
    }

    private bool IsValidPhone(string phone) {
      return !string.IsNullOrWhiteSpace(phone) && Regex.IsMatch(phone.Trim(), @"^[0-9]{10,}$");
    }

    private bool IsValidMinimumAge(DateTime birthDate) {
      return birthDate.Date <= DateTime.Today.AddYears(-3);
    }

    private void LoadViewData() {
      var usuario = SessionContext.UsuarioLogueado;
      if (usuario != null) {
        TextBlockUsernameHeader.Text = $"\"{usuario.NombreDeUsuario}\"";
        TextBlockFullName.Text = usuario.Nombre;
        string yearsText = (string) Application.Current.FindResource("MainMenu_Years") ?? "Años";
        TextBlockAge.Text = $"{CalculateAge(usuario.FechaDeNacimiento)} {yearsText}";
        TextBlockBirthDate.Text = usuario.FechaDeNacimiento.ToString("dd/MM/yyyy");
        TextBlockEmail.Text = usuario.Correo;
        string notRegText = (string) Application.Current.FindResource("Profile_NotRegistered") ?? "No Registrado";
        TextBlockPhone.Text = !string.IsNullOrWhiteSpace(usuario.Telefono) ? usuario.Telefono : notRegText;
      }
    }

    private void SwitchMode(bool isEdit) {
      if (isEdit) {
        GridProfileView.Visibility = Visibility.Collapsed;
        GridProfileEdit.Visibility = Visibility.Visible;
        GridFooterNavigation.Visibility = Visibility.Collapsed;
        GridFooterEdit.Visibility = Visibility.Visible;

        var usuario = SessionContext.UsuarioLogueado;
        if (usuario != null) {
          TextBoxUsername.Text = usuario.NombreDeUsuario;
          TextBoxFullName.Text = usuario.Nombre;
          TextBoxEmail.Text = usuario.Correo;
          TextBoxPhone.Text = usuario.Telefono;
          DatePickerBirthDate.SelectedDate = usuario.FechaDeNacimiento;

          if (usuario.IdiomaPreferido == Idiomas.INGLES) {
            ComboBoxPreferredLanguage.SelectedIndex = 1;
          } else if (usuario.IdiomaPreferido == Idiomas.ESPANOL) {
            ComboBoxPreferredLanguage.SelectedIndex = 2;
          } else {
            ComboBoxPreferredLanguage.SelectedIndex = 0;
          }
          UpdateLanguageButtons();
        }
      } else {
        GridProfileView.Visibility = Visibility.Visible;
        GridProfileEdit.Visibility = Visibility.Collapsed;
        GridFooterNavigation.Visibility = Visibility.Visible;
        GridFooterEdit.Visibility = Visibility.Collapsed;
        GridPasswordModal.Visibility = Visibility.Collapsed;
      }
    }

    private int CalculateAge(DateTime birthDate) {
      DateTime today = DateTime.Today;
      int age = today.Year - birthDate.Year;

      if (birthDate.Date > today.AddYears(-age)) {
        age--;
      }

      return age;
    }

    private void ButtonEdit_Click(object sender, RoutedEventArgs e) {
      SwitchMode(true);
    }

    private void ButtonSave_Click(object sender, RoutedEventArgs e) {
      TextBlockFullNameError.Visibility = Visibility.Hidden;
      TextBlockPhoneError.Visibility = Visibility.Hidden;
      TextBlockBirthDateError.Visibility = Visibility.Hidden;
      TextBlockLanguageError.Visibility = Visibility.Hidden;

      bool hasError = false;

      string fullName = TextBoxFullName.Text.Trim();
      if (string.IsNullOrWhiteSpace(fullName)) {
        TextBlockFullNameError.Text = (string) Application.Current.FindResource("Profile_FullNameError") ?? "Ingrese un nombre válido.";
        TextBlockFullNameError.Visibility = Visibility.Visible;
        hasError = true;
      }

      string phone = TextBoxPhone.Text.Trim();
      if (!IsValidPhone(phone)) {
        TextBlockPhoneError.Text = (string) Application.Current.FindResource("Profile_PhoneError") ?? "Ingrese mínimo 10 dígitos numéricos.";
        TextBlockPhoneError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if(DatePickerBirthDate.SelectedDate == null) {
        TextBlockBirthDateError.Text = (string) Application.Current.FindResource("Profile_BirthDateError") ?? "Seleccione una fecha válida.";
        TextBlockBirthDateError.Visibility = Visibility.Visible;
        hasError = true;
      } else if (!IsValidMinimumAge(DatePickerBirthDate.SelectedDate.Value)) {
        TextBlockBirthDateError.Text = (string) Application.Current.FindResource("Profile_MinimumAgeError") ?? "El jugador debe tener al menos 3 años.";
        TextBlockBirthDateError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (ComboBoxPreferredLanguage.SelectedIndex <= 0) {
        TextBlockLanguageError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (hasError) {
        return;
      }

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario == null) {
        return;
      }

      try {
        var jugadorService = ServiceProxyManager.GetJugadorService();

        // Actualizar perfil
        var jugadorDto = new JugadorDTO {
          IDJugador = usuario.IDJugador,
          Nombre = fullName,
          NombreDeUsuario = usuario.NombreDeUsuario,
          Correo = usuario.Correo,
          Telefono = phone,
          Puntuacion = usuario.Puntuacion,
          Avatar = usuario.Avatar,
          IdiomaPreferido = ComboBoxPreferredLanguage.SelectedIndex == 1 ? Idiomas.INGLES : Idiomas.ESPANOL,
          FechaDeNacimiento = DatePickerBirthDate.SelectedDate.Value
        };

        var profileResult = jugadorService.ActualizarPerfil(jugadorDto);
        if (profileResult != null && profileResult.IsSuccess) {
          // Actualizar sesión local
          usuario.Nombre = jugadorDto.Nombre;
          usuario.Telefono = jugadorDto.Telefono;
          usuario.Avatar = jugadorDto.Avatar;
          usuario.IdiomaPreferido = jugadorDto.IdiomaPreferido;
          usuario.FechaDeNacimiento = jugadorDto.FechaDeNacimiento;

          string successMsg = (string) Application.Current.FindResource("Profile_UpdateSuccessMsg") ?? "Perfil actualizado correctamente.";
          string successTitle = (string) Application.Current.FindResource("Profile_UpdateSuccessTitle") ?? "TecnoHorcado";
          MessageBox.Show(successMsg,
                          successTitle,
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);

          LoadViewData();
          SwitchMode(false);
        } else {
          if (profileResult?.Error != null) {
            string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
            MessageBox.Show(profileResult.Error.Mensaje, errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
          } else {
            string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
            string updateErr = (string) Application.Current.FindResource("Profile_UpdateErrorMsg") ?? "Error al actualizar el perfil.";
            MessageBox.Show(updateErr, errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
          }
        }
      } catch (CommunicationException) {
        string connMsg = (string) Application.Current.FindResource("Msg_ConnectionError") ?? "No se pudo conectar con el servidor. Verifique su conexión.";
        string connTitle = (string) Application.Current.FindResource("Msg_ConnectionErrorTitle") ?? "Error de Conexión";
        MessageBox.Show(connMsg, connTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      } catch (Exception ex) {
        string unexpMsg = (string) Application.Current.FindResource("Msg_UnexpectedError") ?? "Ocurrió un error inesperado:";
        string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        MessageBox.Show($"{unexpMsg} {ex.Message}", errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
      PasswordBoxCurrentPassword.Password = "";
      PasswordBoxPassword.Password = "";
      PasswordBoxConfirmPassword.Password = "";
      GridPasswordModal.Visibility = Visibility.Collapsed;
      SwitchMode(false);
    }

    private void ButtonOpenChangePassword_Click(object sender, RoutedEventArgs e) {
      PasswordBoxCurrentPassword.Password = "";
      PasswordBoxPassword.Password = "";
      PasswordBoxConfirmPassword.Password = "";
      TextBlockCurrentPasswordError.Visibility = Visibility.Hidden;
      TextBlockPasswordError.Visibility = Visibility.Hidden;
      GridPasswordModal.Visibility = Visibility.Visible;
    }

    private void ButtonCancelPassword_Click(object sender, RoutedEventArgs e) {
      PasswordBoxCurrentPassword.Password = "";
      PasswordBoxPassword.Password = "";
      PasswordBoxConfirmPassword.Password = "";
      GridPasswordModal.Visibility = Visibility.Collapsed;
    }

    private void ButtonSavePassword_Click(object sender, RoutedEventArgs e) {
      TextBlockCurrentPasswordError.Visibility = Visibility.Hidden;
      TextBlockPasswordError.Visibility = Visibility.Hidden;

      string currentPassword = PasswordBoxCurrentPassword.Password;
      string newPassword = PasswordBoxPassword.Password;
      string confirmPassword = PasswordBoxConfirmPassword.Password;

      bool hasError = false;

      if (string.IsNullOrEmpty(currentPassword)) {
        TextBlockCurrentPasswordError.Text = (string) Application.Current.FindResource("Profile_CurrentPasswordPlaceholder") ?? "Ingrese su contraseña actual.";
        TextBlockCurrentPasswordError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (string.IsNullOrEmpty(newPassword)) {
        TextBlockPasswordError.Text = (string) Application.Current.FindResource("Profile_NewPasswordPlaceholder") ?? "Ingrese la nueva contraseña.";
        TextBlockPasswordError.Visibility = Visibility.Visible;
        hasError = true;
      } else if (newPassword != confirmPassword) {
        TextBlockPasswordError.Text = (string) Application.Current.FindResource("Profile_PasswordMatchError") ?? "Las contraseñas no coinciden.";
        TextBlockPasswordError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (hasError) {
        return;
      }

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario == null) {
        return;
      }

      try {
        var jugadorService = ServiceProxyManager.GetJugadorService();
        var passwordResult = jugadorService.CambiarContrasena(usuario.IDJugador, currentPassword, newPassword);
        
        if (passwordResult != null && passwordResult.IsSuccess) {
          string successMsg = (string) Application.Current.FindResource("Profile_UpdateSuccessMsg") ?? "Contraseña actualizada correctamente.";
          string successTitle = (string) Application.Current.FindResource("Profile_UpdateSuccessTitle") ?? "TecnoHorcado";
          MessageBox.Show(successMsg, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
          
          PasswordBoxCurrentPassword.Password = "";
          PasswordBoxPassword.Password = "";
          PasswordBoxConfirmPassword.Password = "";
          GridPasswordModal.Visibility = Visibility.Collapsed;
        } else {
          if (passwordResult?.Error != null) {
            if (passwordResult.Error.CodigoError == CodigoError.CREDENCIALES_INVALIDAS) {
              TextBlockCurrentPasswordError.Text = passwordResult.Error.Mensaje;
              TextBlockCurrentPasswordError.Visibility = Visibility.Visible;
            } else {
              string passChangeErrTitle = (string) Application.Current.FindResource("Profile_PasswordChangeErrorTitle") ?? "Error al cambiar contraseña";
              MessageBox.Show(passwordResult.Error.Mensaje, passChangeErrTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
          } else {
            string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
            string noChangeErr = (string) Application.Current.FindResource("Profile_ErrorPasswordChange") ?? "No se pudo cambiar la contraseña.";
            MessageBox.Show(noChangeErr, errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
          }
        }
      } catch (CommunicationException) {
        string connMsg = (string) Application.Current.FindResource("Msg_ConnectionError") ?? "No se pudo conectar con el servidor. Verifique su conexión.";
        string connTitle = (string) Application.Current.FindResource("Msg_ConnectionErrorTitle") ?? "Error de Conexión";
        MessageBox.Show(connMsg, connTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      } catch (Exception ex) {
        string unexpMsg = (string) Application.Current.FindResource("Msg_UnexpectedError") ?? "Ocurrió un error inesperado:";
        string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        MessageBox.Show($"{unexpMsg} {ex.Message}", errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void PasswordBoxCurrentPassword_PasswordChanged(object sender, RoutedEventArgs e) {
      if (TextBlockCurrentPasswordPlaceholder != null) {
        TextBlockCurrentPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(PasswordBoxCurrentPassword.Password) ? Visibility.Visible : Visibility.Hidden;
      }
    }

    private void PasswordBoxPassword_PasswordChanged(object sender, RoutedEventArgs e) {
      if (TextBlockPasswordPlaceholder != null) {
        TextBlockPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(PasswordBoxPassword.Password) ? Visibility.Visible : Visibility.Hidden;
      }
    }

    private void PasswordBoxConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e) {
      if (TextBlockConfirmPasswordPlaceholder != null) {
        TextBlockConfirmPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(PasswordBoxConfirmPassword.Password) ? Visibility.Visible : Visibility.Hidden;
      }
    }

    private void ButtonMainMenu_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIGameHubView());
    }

    private void ButtonProfile_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIProfileView());
    }

    private void ButtonHistory_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIHistoryView());
    }

    private void ButtonScoreboard_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUILeaderboardView());
    }

    private void ButtonSettings_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUISettingsView());
    }

    private void UpdateLanguageButtons() {
      if (ComboBoxPreferredLanguage.SelectedIndex == 1) { // Ingles
        ButtonLangEN.Style = (Style) FindResource("ModernPrimaryButton");
        ButtonLangES.Style = (Style) FindResource("ModernSecondaryButton");
      } else if (ComboBoxPreferredLanguage.SelectedIndex == 2) { // Español
        ButtonLangES.Style = (Style) FindResource("ModernPrimaryButton");
        ButtonLangEN.Style = (Style) FindResource("ModernSecondaryButton");
      } else {
        ButtonLangES.Style = (Style) FindResource("ModernSecondaryButton");
        ButtonLangEN.Style = (Style) FindResource("ModernSecondaryButton");
      }
    }

    private void ButtonLangES_Click(object sender, RoutedEventArgs e) {
      ComboBoxPreferredLanguage.SelectedIndex = 2;
      UpdateLanguageButtons();
      Services.LanguageManager.SetLanguage("ES");
    }

    private void ButtonLangEN_Click(object sender, RoutedEventArgs e) {
      ComboBoxPreferredLanguage.SelectedIndex = 1;
      UpdateLanguageButtons();
      Services.LanguageManager.SetLanguage("EN");
    }
  }
}
