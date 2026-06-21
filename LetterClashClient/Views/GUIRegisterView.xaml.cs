using System;
using System.IO;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

using LetterClashClient.Models;
using LetterClashClient.Services;

using LetterClashServer.Domain.Models;

namespace LetterClashClient.Views {
  public partial class GUIRegisterView : Page {
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);
    private byte[] selectedAvatarBytes;
    private bool isPasswordVisible;
    private bool isConfirmPasswordVisible;
    private bool isSyncingPassword;
    private bool isSyncingConfirmPassword;

    public GUIRegisterView() {
      InitializeComponent();
    }

    private bool IsValidFullName(string fullName) {
      return !string.IsNullOrWhiteSpace(fullName) &&
             Regex.IsMatch(fullName.Trim(), @"^(?=.{8,}$)\p{L}{3,}(?:\s+\p{L}+)*\s+\p{L}{4,}$", RegexOptions.None, RegexTimeout);
    }

    private bool IsValidUsername(string username) {
      return !string.IsNullOrWhiteSpace(username) &&
             Regex.IsMatch(username.Trim(), @"^[A-Za-z0-9_-]{3,12}$", RegexOptions.None, RegexTimeout);
    }

    private bool IsValidEmail(string email) {
      return !string.IsNullOrWhiteSpace(email) &&
             Regex.IsMatch(email.Trim(), @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,24}$", RegexOptions.IgnoreCase, RegexTimeout);
    }

    private bool IsValidPhone(string phone) {
      return !string.IsNullOrWhiteSpace(phone) &&
             Regex.IsMatch(phone.Trim(), @"^[0-9]{10}$", RegexOptions.None, RegexTimeout);
    }

    private bool IsValidBirthDate(DateTime birthDate) {
      DateTime today = DateTime.Today;
      return birthDate.Date <= today.AddYears(-3) &&
             birthDate.Date >= today.AddYears(-100) &&
             birthDate.Date <= today;
    }

    private bool IsValidPassword(string password) {
      return !string.IsNullOrEmpty(password) &&
             Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{6,15}$", RegexOptions.None, RegexTimeout);
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
      TextBlockEmailError.Visibility = Visibility.Hidden;
      TextBlockPhoneError.Visibility = Visibility.Hidden;
      TextBlockPasswordError.Visibility = Visibility.Hidden;
      TextBlockBirthDateError.Visibility = Visibility.Hidden;
      TextBlockUsernameError.Visibility = Visibility.Hidden;
      TextBlockLanguageError.Visibility = Visibility.Hidden;

      bool hasError = false;

      string fullName = TextBoxFullName.Text.Trim();
      if (!IsValidFullName(fullName)) {
        TextBlockFullNameError.Text = (string) Application.Current.FindResource("Register_FullNameError") ?? "Ingrese nombre y apellido válidos.";
        TextBlockFullNameError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (!IsValidEmail(TextBoxEmail.Text)) {
        TextBlockEmailError.Text = (string) Application.Current.FindResource("Register_EmailError") ?? "Ingrese un correo válido.";
        TextBlockEmailError.Visibility = Visibility.Visible;
        hasError = true;
      }

      string phone = TextBoxPhone.Text.Trim();

      if (!IsValidPhone(phone)) {
        TextBlockPhoneError.Text = (string) Application.Current.FindResource("Register_PhoneError") ?? "Ingrese mínimo 10 dígitos numéricos.";
        TextBlockPhoneError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (!IsValidPassword(PasswordBoxPassword.Password)) {
        TextBlockPasswordError.Text = (string) Application.Current.FindResource("Register_PasswordFormatError") ?? "Contraseña: 6-15, mayúscula, minúscula, número y símbolo.";
        TextBlockPasswordError.Visibility = Visibility.Visible;
        hasError = true;
      } else if (PasswordBoxPassword.Password != PasswordBoxConfirmPassword.Password) {
        TextBlockPasswordError.Text = (string) Application.Current.FindResource("Register_PasswordError") ?? "Las contraseñas no coinciden.";
        TextBlockPasswordError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (DatePickerBirthDate.SelectedDate == null) {
        TextBlockBirthDateError.Text = (string) Application.Current.FindResource("Register_BirthDateError") ?? "Seleccione una fecha válida.";
        TextBlockBirthDateError.Visibility = Visibility.Visible;
        hasError = true;
      } else if (!IsValidBirthDate(DatePickerBirthDate.SelectedDate.Value)) {
        TextBlockBirthDateError.Text = (string) Application.Current.FindResource("Register_BirthDateRangeError") ?? "Edad permitida: 3 a 100 años.";
        TextBlockBirthDateError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (!IsValidUsername(TextBoxUsername.Text)) {
        TextBlockUsernameError.Text = (string) Application.Current.FindResource("Register_UsernameErrorInvalid") ?? "Use 3 a 12 caracteres: letras, números, guion o guion bajo.";
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
        Nombre = fullName,
        NombreDeUsuario = TextBoxUsername.Text.Trim(),
        Correo = TextBoxEmail.Text.Trim(),
        Telefono = phone,
        FechaDeNacimiento = DatePickerBirthDate.SelectedDate.Value,
        IdiomaPreferido = ComboBoxPreferredLanguage.SelectedIndex == 1 ? Idiomas.INGLES : Idiomas.ESPANOL,
        Puntuacion = 0,
        Avatar = selectedAvatarBytes
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

    private void TextBoxPhone_PreviewTextInput(object sender, TextCompositionEventArgs e) {
      e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$", RegexOptions.None, RegexTimeout);
    }

    private void TextBoxPhone_Pasting(object sender, DataObjectPastingEventArgs e) {
      if (!e.DataObject.GetDataPresent(typeof(string)) ||
          !Regex.IsMatch((string) e.DataObject.GetData(typeof(string)), @"^[0-9]+$", RegexOptions.None, RegexTimeout)) {
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

    private void PasswordBoxPassword_PasswordChanged(object sender, RoutedEventArgs e) {
      TextBlockPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(PasswordBoxPassword.Password) ? Visibility.Visible : Visibility.Hidden;

      if (isPasswordVisible && !isSyncingPassword) {
        isSyncingPassword = true;
        TextBoxPasswordVisible.Text = PasswordBoxPassword.Password;
        isSyncingPassword = false;
      }
    }

    private void PasswordBoxConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e) {
      TextBlockConfirmPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(PasswordBoxConfirmPassword.Password) ? Visibility.Visible : Visibility.Hidden;

      if (isConfirmPasswordVisible && !isSyncingConfirmPassword) {
        isSyncingConfirmPassword = true;
        TextBoxConfirmPasswordVisible.Text = PasswordBoxConfirmPassword.Password;
        isSyncingConfirmPassword = false;
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

    private void TextBoxConfirmPasswordVisible_TextChanged(object sender, TextChangedEventArgs e) {
      if (!isConfirmPasswordVisible || isSyncingConfirmPassword) {
        return;
      }

      isSyncingConfirmPassword = true;
      PasswordBoxConfirmPassword.Password = TextBoxConfirmPasswordVisible.Text;
      isSyncingConfirmPassword = false;
      TextBlockConfirmPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(TextBoxConfirmPasswordVisible.Text) ? Visibility.Visible : Visibility.Hidden;
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

        selectedAvatarBytes = avatar;
        ImageRegisterAvatar.Source = AvatarHelper.ObtenerImagen(selectedAvatarBytes);
      } catch (Exception ex) {
        string message = (string) Application.Current.FindResource("Profile_ImageLoadError") ?? "Error al cargar la imagen:";
        string title = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        MessageBox.Show($"{message} {ex.Message}", title, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }
  }
}
