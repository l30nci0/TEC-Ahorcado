using System;
using System.Windows;
using System.Windows.Controls;

namespace LetterClashClient.Views {
  public partial class DifferentUser : Page {
    private string username;

    public DifferentUser() {
      InitializeComponent();
      username = "Usuario 1";
    }

    public DifferentUser(string username) {
      InitializeComponent();
      this.username = string.IsNullOrWhiteSpace(username) ? "Usuario 1" : username;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = "Diferente Usuario";
      }

      LoadUserData();
    }

    private void LoadUserData() {
      DateTime birthDate = GetBirthDate(username);
      int age = CalculateAge(birthDate);

      TextBlockUsernameHeader.Text = $"\"{username}\"";
      TextBlockFullName.Text = username;
      TextBlockAge.Text = age.ToString();
      TextBlockBirthDate.Text = birthDate.ToString("dd/MM/yyyy");
      TextBlockEmail.Text = $"{username.ToLower().Replace(" ", "")}@gmail.com";
      TextBlockPhone.Text = GetPhone(username);

      TextBlockTotalWins.Text = "Total Ganadas = 60";
      TextBlockPercentWins.Text = "%60";
      TextBlockTotalLosses.Text = "Total Perdidas = 35";
      TextBlockPercentLosses.Text = "%35";
      TextBlockTotalDisconnected.Text = "Total Desconectadas = 5";
      TextBlockPercentDisconnected.Text = "%5";
    }

    private DateTime GetBirthDate(string username) {
      if (username == "Usuario 1") {
        return new DateTime(2005, 4, 1);
      }

      if (username == "Usuario 2") {
        return new DateTime(2004, 8, 15);
      }

      if (username == "Usuario 3") {
        return new DateTime(2006, 2, 20);
      }

      return new DateTime(2005, 1, 10);
    }

    private string GetPhone(string username) {
      if (username == "Usuario 1") {
        return "2288945645";
      }

      if (username == "Usuario 2") {
        return "2281239981";
      }

      if (username == "Usuario 3") {
        return "2284567789";
      }

      return "2280000000";
    }

    private int CalculateAge(DateTime birthDate) {
      DateTime today = DateTime.Today;
      int age = today.Year - birthDate.Year;

      if (birthDate.Date > today.AddYears(-age)) {
        age--;
      }

      return age;
    }

    private void ButtonBack_Click(object sender, RoutedEventArgs e) {
      if (NavigationService != null && NavigationService.CanGoBack) {
        NavigationService.GoBack();
      } else {
        NavigationService.Navigate(new Scoreboard());
      }
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
