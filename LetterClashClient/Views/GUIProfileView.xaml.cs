using System;
using System.IO;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

using LetterClashClient.Models;
using LetterClashClient.Services;

using LetterClashServer.Domain.Models;

namespace LetterClashClient.Views {
  public partial class GUIProfileView : Page {
    private byte[] selectedProfileAvatarBytes;
    private bool isCurrentPasswordVisible;
    private bool isPasswordVisible;
    private bool isConfirmPasswordVisible;
    private bool isSyncingCurrentPassword;
    private bool isSyncingPassword;
    private bool isSyncingConfirmPassword;

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

    private bool IsValidFullName(string fullName) {
      return !string.IsNullOrWhiteSpace(fullName) &&
             Regex.IsMatch(fullName.Trim(), @"^(?=.{8,}$)\p{L}{3,}(?:\s+\p{L}+)*\s+\p{L}{4,}$");
    }

    private bool IsValidPhone(string phone) {
      return !string.IsNullOrWhiteSpace(phone) && Regex.IsMatch(phone.Trim(), @"^[0-9]{10}$");
    }

    private bool IsValidBirthDate(DateTime birthDate) {
      DateTime today = DateTime.Today;
      return birthDate.Date <= today.AddYears(-3) &&
             birthDate.Date >= today.AddYears(-100) &&
             birthDate.Date <= today;
    }

    private bool IsValidPassword(string password) {
      return !string.IsNullOrEmpty(password) &&
             Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{6,15}$");
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
        AvatarHelper.AsignarAImageControl(ImageProfileAvatarView, usuario.Avatar);
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
          selectedProfileAvatarBytes = usuario.Avatar;
          AvatarHelper.AsignarAImageControl(ImageProfileAvatarEdit, selectedProfileAvatarBytes);

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
      if (!IsValidFullName(fullName)) {
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
      } else if (!IsValidBirthDate(DatePickerBirthDate.SelectedDate.Value)) {
        TextBlockBirthDateError.Text = (string) Application.Current.FindResource("Profile_BirthDateRangeError") ?? "Edad permitida: 3 a 100 años.";
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
          Avatar = selectedProfileAvatarBytes,
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
      ClearPasswordFields();
      GridPasswordModal.Visibility = Visibility.Collapsed;
      SwitchMode(false);
    }

    private void ButtonOpenChangePassword_Click(object sender, RoutedEventArgs e) {
      ClearPasswordFields();
      TextBlockCurrentPasswordError.Visibility = Visibility.Hidden;
      TextBlockPasswordError.Visibility = Visibility.Hidden;
      GridPasswordModal.Visibility = Visibility.Visible;
    }

    private void ButtonCancelPassword_Click(object sender, RoutedEventArgs e) {
      ClearPasswordFields();
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
      } else if (!IsValidPassword(newPassword)) {
        TextBlockPasswordError.Text = (string) Application.Current.FindResource("Profile_PasswordFormatError") ?? "Contraseña: 6-15, mayúscula, minúscula, número y símbolo.";
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
          
          ClearPasswordFields();
          GridPasswordModal.Visibility = Visibility.Collapsed;
          SessionContext.UsuarioLogueado = null;
          NavigationService.Navigate(new GUILoginView());
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

      if (isCurrentPasswordVisible && !isSyncingCurrentPassword) {
        isSyncingCurrentPassword = true;
        TextBoxCurrentPasswordVisible.Text = PasswordBoxCurrentPassword.Password;
        isSyncingCurrentPassword = false;
      }
    }

    private void PasswordBoxPassword_PasswordChanged(object sender, RoutedEventArgs e) {
      if (TextBlockPasswordPlaceholder != null) {
        TextBlockPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(PasswordBoxPassword.Password) ? Visibility.Visible : Visibility.Hidden;
      }

      if (isPasswordVisible && !isSyncingPassword) {
        isSyncingPassword = true;
        TextBoxPasswordVisible.Text = PasswordBoxPassword.Password;
        isSyncingPassword = false;
      }
    }

    private void PasswordBoxConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e) {
      if (TextBlockConfirmPasswordPlaceholder != null) {
        TextBlockConfirmPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(PasswordBoxConfirmPassword.Password) ? Visibility.Visible : Visibility.Hidden;
      }

      if (isConfirmPasswordVisible && !isSyncingConfirmPassword) {
        isSyncingConfirmPassword = true;
        TextBoxConfirmPasswordVisible.Text = PasswordBoxConfirmPassword.Password;
        isSyncingConfirmPassword = false;
      }
    }

    private void TextBoxCurrentPasswordVisible_TextChanged(object sender, TextChangedEventArgs e) {
      if (!isCurrentPasswordVisible || isSyncingCurrentPassword) {
        return;
      }

      isSyncingCurrentPassword = true;
      PasswordBoxCurrentPassword.Password = TextBoxCurrentPasswordVisible.Text;
      isSyncingCurrentPassword = false;
      TextBlockCurrentPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(TextBoxCurrentPasswordVisible.Text) ? Visibility.Visible : Visibility.Hidden;
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

    private void TextBoxConfirmPasswordVisible_TextChanged(object sender, TextChangedEventArgs e) {
      if (!isConfirmPasswordVisible || isSyncingConfirmPassword) {
        return;
      }

      isSyncingConfirmPassword = true;
      PasswordBoxConfirmPassword.Password = TextBoxConfirmPasswordVisible.Text;
      isSyncingConfirmPassword = false;
      TextBlockConfirmPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(TextBoxConfirmPasswordVisible.Text) ? Visibility.Visible : Visibility.Hidden;
    }

    private void ButtonToggleCurrentPasswordVisibility_Click(object sender, RoutedEventArgs e) {
      isCurrentPasswordVisible = !isCurrentPasswordVisible;
      TogglePasswordVisibility(PasswordBoxCurrentPassword, TextBoxCurrentPasswordVisible, isCurrentPasswordVisible);
    }

    private void ButtonTogglePasswordVisibility_Click(object sender, RoutedEventArgs e) {
      isPasswordVisible = !isPasswordVisible;
      TogglePasswordVisibility(PasswordBoxPassword, TextBoxPasswordVisible, isPasswordVisible);
    }

    private void ButtonToggleConfirmPasswordVisibility_Click(object sender, RoutedEventArgs e) {
      isConfirmPasswordVisible = !isConfirmPasswordVisible;
      TogglePasswordVisibility(PasswordBoxConfirmPassword, TextBoxConfirmPasswordVisible, isConfirmPasswordVisible);
    }

    private void TogglePasswordVisibility(PasswordBox passwordBox, TextBox visibleTextBox, bool showText) {
      if (showText) {
        visibleTextBox.Text = passwordBox.Password;
        visibleTextBox.Visibility = Visibility.Visible;
        passwordBox.Visibility = Visibility.Collapsed;
        visibleTextBox.Focus();
        visibleTextBox.CaretIndex = visibleTextBox.Text.Length;
      } else {
        passwordBox.Password = visibleTextBox.Text;
        passwordBox.Visibility = Visibility.Visible;
        visibleTextBox.Visibility = Visibility.Collapsed;
        passwordBox.Focus();
      }
    }

    private void ClearPasswordFields() {
      PasswordBoxCurrentPassword.Password = "";
      PasswordBoxPassword.Password = "";
      PasswordBoxConfirmPassword.Password = "";
      TextBoxCurrentPasswordVisible.Text = "";
      TextBoxPasswordVisible.Text = "";
      TextBoxConfirmPasswordVisible.Text = "";
      isCurrentPasswordVisible = false;
      isPasswordVisible = false;
      isConfirmPasswordVisible = false;
      PasswordBoxCurrentPassword.Visibility = Visibility.Visible;
      PasswordBoxPassword.Visibility = Visibility.Visible;
      PasswordBoxConfirmPassword.Visibility = Visibility.Visible;
      TextBoxCurrentPasswordVisible.Visibility = Visibility.Collapsed;
      TextBoxPasswordVisible.Visibility = Visibility.Collapsed;
      TextBoxConfirmPasswordVisible.Visibility = Visibility.Collapsed;
    }

    private void TextBoxPhone_PreviewTextInput(object sender, TextCompositionEventArgs e) {
      e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
    }

    private void TextBoxPhone_Pasting(object sender, DataObjectPastingEventArgs e) {
      if (!e.DataObject.GetDataPresent(typeof(string)) ||
          !Regex.IsMatch((string) e.DataObject.GetData(typeof(string)), @"^[0-9]+$")) {
        e.CancelCommand();
      }
    }

    private void DatePickerBirthDate_PreviewKeyDown(object sender, KeyEventArgs e) {
      if (e.Key != Key.Tab && e.Key != Key.LeftShift && e.Key != Key.RightShift) {
        e.Handled = true;
      }
    }

    private void DatePickerBirthDate_Pasting(object sender, DataObjectPastingEventArgs e) {
      e.CancelCommand();
    }

    private byte[] LoadAvatarFromDialog() {
      OpenFileDialog dialog = new OpenFileDialog {
        Title = (string) Application.Current.FindResource("Profile_AvatarSelectTitle") ?? "Seleccionar Avatar",
        Filter = (string) Application.Current.FindResource("Profile_ImageFilter") ?? "Imágenes (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
      };

      if (dialog.ShowDialog() != true) {
        return null;
      }

      string extension = Path.GetExtension(dialog.FileName)?.ToLowerInvariant();
      if (extension != ".png" && extension != ".jpg" && extension != ".jpeg") {
        string message = (string) Application.Current.FindResource("Profile_ImageFormatError") ?? "Seleccione una imagen PNG, JPG o JPEG.";
        string title = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        return null;
      }

      FileInfo fileInfo = new FileInfo(dialog.FileName);
      if (fileInfo.Length > 2 * 1024 * 1024) {
        string message = (string) Application.Current.FindResource("Profile_ImageSizeExceededMsg") ?? "El tamaño de la imagen no debe superar los 2MB.";
        string title = (string) Application.Current.FindResource("Profile_ImageSizeExceededTitle") ?? "Tamaño Excedido";
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        return null;
      }

      return File.ReadAllBytes(dialog.FileName);
    }

    private void ButtonSelectAvatar_Click(object sender, RoutedEventArgs e) {
      try {
        byte[] avatar = LoadAvatarFromDialog();
        if (avatar == null) {
          return;
        }

        selectedProfileAvatarBytes = avatar;
        ImageProfileAvatarEdit.Source = AvatarHelper.ObtenerImagen(selectedProfileAvatarBytes);
      } catch (Exception ex) {
        string message = (string) Application.Current.FindResource("Profile_ImageLoadError") ?? "Error al cargar la imagen:";
        string title = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        MessageBox.Show($"{message} {ex.Message}", title, MessageBoxButton.OK, MessageBoxImage.Error);
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
