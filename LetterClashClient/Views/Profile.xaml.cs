using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

using LetterClashClient.Models;

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

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario != null) {
        TextBlockUsernameHeader.Text = $"\"{usuario.NombreDeUsuario}\"";
        TextBlockFullName.Text = usuario.Nombre;
        TextBlockAge.Text = CalculateAge(usuario.FechaDeNacimiento).ToString() + " Años";
        TextBlockBirthDate.Text = usuario.FechaDeNacimiento.ToString("dd/MM/yyyy");
        TextBlockEmail.Text = usuario.Correo;
        TextBlockPhone.Text = !string.IsNullOrWhiteSpace(usuario.Telefono) ? usuario.Telefono : "No Registrado";

        if (usuario.Avatar != null && usuario.Avatar.Length > 0) {
          try {
            using (var stream = new System.IO.MemoryStream(usuario.Avatar)) {
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
