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
    private byte[] selectedAvatarBytes;

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

        if (usuario.Avatar != null && usuario.Avatar.Length > 0) {
          try {
            using (var stream = new MemoryStream(usuario.Avatar)) {
              var bitmap = new BitmapImage();
              bitmap.BeginInit();
              bitmap.StreamSource = stream;
              bitmap.CacheOption = BitmapCacheOption.OnLoad;
              bitmap.EndInit();
              ImageUserAvatar.Source = bitmap;
            }
          } catch {
            // Mantiene el default en caso de error
          }
        }
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

          selectedAvatarBytes = usuario.Avatar;
          if (selectedAvatarBytes != null && selectedAvatarBytes.Length > 0) {
            try {
              using (var stream = new MemoryStream(selectedAvatarBytes)) {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ImageEditUserAvatar.Source = bitmap;
              }
            } catch { }
          } else {
            ImageEditUserAvatar.Source = new BitmapImage(new Uri("/Assets/Images/UserAvatar.png", UriKind.RelativeOrAbsolute));
          }
        }
      } else {
        GridProfileView.Visibility = Visibility.Visible;
        GridProfileEdit.Visibility = Visibility.Collapsed;
        GridFooterNavigation.Visibility = Visibility.Visible;
        GridFooterEdit.Visibility = Visibility.Collapsed;
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
      TextBlockCurrentPasswordError.Visibility = Visibility.Hidden;
      TextBlockPasswordError.Visibility = Visibility.Hidden;
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
      if (string.IsNullOrWhiteSpace(phone) || !Regex.IsMatch(phone, @"^[0-9]{10}$")) {
        TextBlockPhoneError.Text = (string) Application.Current.FindResource("Profile_PhoneError") ?? "Coloque un número de 10 dígitos.";
        TextBlockPhoneError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (DatePickerBirthDate.SelectedDate == null) {
        TextBlockBirthDateError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (ComboBoxPreferredLanguage.SelectedIndex <= 0) {
        TextBlockLanguageError.Visibility = Visibility.Visible;
        hasError = true;
      }

      string currentPassword = PasswordBoxCurrentPassword.Password;
      string newPassword = PasswordBoxPassword.Password;
      string confirmPassword = PasswordBoxConfirmPassword.Password;

      bool isChangingPassword = !string.IsNullOrEmpty(currentPassword) ||
                                !string.IsNullOrEmpty(newPassword) ||
                                !string.IsNullOrEmpty(confirmPassword);

      if (isChangingPassword) {
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

        // 1. Cambiar contraseña si se solicita
        if (isChangingPassword) {
          var passwordResult = jugadorService.CambiarContrasena(usuario.IDJugador, currentPassword, newPassword);
          if (passwordResult == null || !passwordResult.IsSuccess) {
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
            return;
          }
        }

        // 2. Actualizar perfil
        var jugadorDto = new JugadorDTO {
          IDJugador = usuario.IDJugador,
          Nombre = fullName,
          NombreDeUsuario = usuario.NombreDeUsuario,
          Correo = usuario.Correo,
          Telefono = phone,
          Puntuacion = usuario.Puntuacion,
          Avatar = selectedAvatarBytes,
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

          // Limpiar campos de contraseña
          PasswordBoxCurrentPassword.Password = "";
          PasswordBoxPassword.Password = "";
          PasswordBoxConfirmPassword.Password = "";

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
      SwitchMode(false);
    }

    private void ButtonAddAvatar_Click(object sender, RoutedEventArgs e) {
      string fileFilter = (string) Application.Current.FindResource("Profile_ImageFilter") ?? "Imágenes (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
      string dialogTitle = (string) Application.Current.FindResource("Profile_AvatarSelectTitle") ?? "Seleccionar Avatar";

      var openFileDialog = new Microsoft.Win32.OpenFileDialog {
        Filter = fileFilter,
        Title = dialogTitle
      };

      if (openFileDialog.ShowDialog() == true) {
        try {
          string filePath = openFileDialog.FileName;
          byte[] avatarBytes = File.ReadAllBytes(filePath);

          const int maxSizeBytes = 2 * 1024 * 1024;
          if (avatarBytes.Length > maxSizeBytes) {
            string sizeMsg = (string) Application.Current.FindResource("Profile_ImageSizeExceededMsg") ?? "El tamaño de la imagen no debe superar los 2MB.";
            string sizeTitle = (string) Application.Current.FindResource("Profile_ImageSizeExceededTitle") ?? "Tamaño Excedido";
            MessageBox.Show(sizeMsg, sizeTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
          }

          using (var stream = new MemoryStream(avatarBytes)) {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            ImageEditUserAvatar.Source = bitmap;
          }

          selectedAvatarBytes = avatarBytes;
        } catch (Exception ex) {
          string loadErr = (string) Application.Current.FindResource("Profile_ImageLoadError") ?? "Error al cargar la imagen:";
          string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
          MessageBox.Show($"{loadErr} {ex.Message}", errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
      NavigationService.Navigate(new GUIMainMenuView());
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
