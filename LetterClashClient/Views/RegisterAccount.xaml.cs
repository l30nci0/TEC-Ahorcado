using System.Windows;
using System.Windows.Controls;

namespace LetterClashClient.Views {
  public partial class RegisterAccount : Page {
    public RegisterAccount() {
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

      if (!hasError) {
        MessageBox.Show("Cuenta creada correctamente.",
                        "TecnoHorcado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

        NavigationService.Navigate(new MainMenu());
      }
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new Login());
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
