using System.Windows;
using System.Windows.Controls;

namespace LetterClashClient.Views {
  public partial class Login : Page {
    private const string VALID_USERNAME = "jugador1";
    private const string VALID_EMAIL = "jugador1@tecnohorcado.com";
    private const string VALID_PASSWORD = "123456";

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

      bool usernameValid = TextBoxUsername.Text == VALID_USERNAME;
      bool emailValid = TextBoxEmail.Text == VALID_EMAIL;
      bool passwordValid = PasswordBoxPassword.Password == VALID_PASSWORD;

      bool identityValid = usernameValid || emailValid;
      bool hasError = false;

      if (!identityValid) {
        if (!usernameEmpty || emailEmpty) {
          TextBlockUsernameError.Visibility = Visibility.Visible;
        }

        if (!emailEmpty || usernameEmpty) {
          TextBlockEmailError.Visibility = Visibility.Visible;
        }

        hasError = true;
      }

      if (passwordEmpty || !passwordValid) {
        TextBlockPasswordError.Visibility = Visibility.Visible;
        hasError = true;
      }

      if (!hasError) {
        NavigationService.Navigate(new MainMenu());
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
