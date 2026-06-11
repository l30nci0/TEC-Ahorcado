using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace LetterClashClient.Views {
  public partial class Profile : Page {
    public Profile() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = "Perfil de Usuario";
      }

      string fullName = "Irving Alejandro Seguin Luna";
      string username = "jugador1";
      DateTime birthDate = new DateTime(2005, 5, 10);
      int age = CalculateAge(birthDate);

      TextBlockUsernameHeader.Text = $"\"{username}\"";
      TextBlockFullName.Text = fullName;
      TextBlockAge.Text = age.ToString();
      TextBlockBirthDate.Text = birthDate.ToString("dd/MM/yyyy");
      TextBlockEmail.Text = "jugador1@tecnohorcado.com";
      TextBlockPhone.Text = "2281234567";
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
      NavigationService.Navigate(new EditProfile());
    }

    private void ButtonMainMenu_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new MainMenu());
    }

    private void ButtonProfile_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new Profile());
    }

    private void ButtonHistory_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new History());
    }

    private void ButtonScoreboard_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new Scoreboard());
    }

    private void ButtonSettings_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new Settings());
    }
  }
}
