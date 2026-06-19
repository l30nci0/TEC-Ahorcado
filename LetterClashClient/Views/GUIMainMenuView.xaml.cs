using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

using LetterClashClient.Models;
using LetterClashClient.Services;

namespace LetterClashClient.Views {
  public partial class GUIMainMenuView : Page {
    private int currentHangmanState = 5;

    public GUIMainMenuView() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = (string) Application.Current.FindResource("MainMenu_WindowTitle") ?? "Menu Principal";
      }

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario != null) {
        TextBlockUsername.Text = $"\"{usuario.NombreDeUsuario}\"";
        string yearsText = (string) Application.Current.FindResource("MainMenu_Years") ?? "Años";
        TextBlockAge.Text = $"\"{CalculateAge(usuario.FechaDeNacimiento)} {yearsText}\"";

        AvatarHelper.AsignarAImageControl(ImageUserAvatar, usuario.Avatar);
      }

      UpdateHangmanImage();
    }

    private int CalculateAge(DateTime birthDate) {
      DateTime today = DateTime.Today;
      int age = today.Year - birthDate.Year;

      if (birthDate.Date > today.AddYears(-age)) {
        age--;
      }

      return age;
    }

    private void ButtonCreateRoom_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUICreateRoomView());
    }

    private void ButtonPublicLobby_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUILobbyView(false));
    }

    private void ButtonPrivateLobby_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUILobbyView(true));
    }

    private void ButtonRemovePart_Click(object sender, RoutedEventArgs e) {
      if (currentHangmanState < 5) {
        currentHangmanState++;
        UpdateHangmanImage();
      }
    }

    private void ButtonAddPart_Click(object sender, RoutedEventArgs e) {
      if (currentHangmanState > 1) {
        currentHangmanState--;
        UpdateHangmanImage();
      }
    }

    private void UpdateHangmanImage() {
      ImageHangman.Source = new BitmapImage(new Uri($"/Assets/Images/Hangedman{currentHangmanState}.jpg", UriKind.Relative));
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
  }
}
