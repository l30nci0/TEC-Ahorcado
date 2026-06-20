using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

using LetterClashClient.Models;
using LetterClashClient.Services;

namespace LetterClashClient.Views {
  public partial class GUIGameHubView : Page {
    private static readonly int[] HangmanStates = { 6, 5, 4, 3, 2, 7, 1 };
    private int currentHangmanStateIndex;

    public GUIGameHubView() {
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
      if (currentHangmanStateIndex < HangmanStates.Length - 1) {
        currentHangmanStateIndex++;
        UpdateHangmanImage();
      }
    }

    private void ButtonAddPart_Click(object sender, RoutedEventArgs e) {
      if (currentHangmanStateIndex > 0) {
        currentHangmanStateIndex--;
        UpdateHangmanImage();
      }
    }

    private void UpdateHangmanImage() {
      ImageHangman.Source = new BitmapImage(new Uri($"/Assets/Images/Hangedman{HangmanStates[currentHangmanStateIndex]}.png", UriKind.Relative));
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
  }
}
