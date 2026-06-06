using System;
using System.Windows;
using System.Windows.Controls;

namespace LetterClashClient.Views
{
    public partial class EditProfile : Page
    {
        public EditProfile()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);

            if (window != null)
            {
                window.Title = "Editar Perfil";
            }

            TextBoxUsername.Text = "jugador1";
            TextBoxFullName.Text = "Irving Alejandro Seguin Luna";
            TextBoxEmail.Text = "jugador1@tecnohorcado.com";
            TextBoxPhone.Text = "2281234567";
            DatePickerBirthDate.SelectedDate = new DateTime(2005, 5, 10);
            ComboBoxPreferredLanguage.SelectedIndex = 2;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            TextBlockFullNameError.Visibility = Visibility.Hidden;
            TextBlockPhoneError.Visibility = Visibility.Hidden;
            TextBlockPasswordError.Visibility = Visibility.Hidden;
            TextBlockBirthDateError.Visibility = Visibility.Hidden;
            TextBlockLanguageError.Visibility = Visibility.Hidden;

            bool hasError = false;

            if (string.IsNullOrWhiteSpace(TextBoxFullName.Text))
            {
                TextBlockFullNameError.Visibility = Visibility.Visible;
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(TextBoxPhone.Text) || !long.TryParse(TextBoxPhone.Text, out _))
            {
                TextBlockPhoneError.Visibility = Visibility.Visible;
                hasError = true;
            }

            if (PasswordBoxPassword.Password != PasswordBoxConfirmPassword.Password)
            {
                TextBlockPasswordError.Visibility = Visibility.Visible;
                hasError = true;
            }

            if (DatePickerBirthDate.SelectedDate == null)
            {
                TextBlockBirthDateError.Visibility = Visibility.Visible;
                hasError = true;
            }

            if (ComboBoxPreferredLanguage.SelectedIndex <= 0)
            {
                TextBlockLanguageError.Visibility = Visibility.Visible;
                hasError = true;
            }

            if (!hasError)
            {
                MessageBox.Show("Perfil actualizado correctamente.",
                                "TecnoHorcado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                NavigationService.Navigate(new Profile());
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Profile());
        }

        private void ButtonAddAvatar_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Aquí se podrá seleccionar una nueva imagen de avatar.",
                            "TecnoHorcado",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void PasswordBoxPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            TextBlockPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(PasswordBoxPassword.Password) ? Visibility.Visible : Visibility.Hidden;
        }

        private void PasswordBoxConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            TextBlockConfirmPasswordPlaceholder.Visibility = string.IsNullOrWhiteSpace(PasswordBoxConfirmPassword.Password) ? Visibility.Visible : Visibility.Hidden;
        }
    }
}