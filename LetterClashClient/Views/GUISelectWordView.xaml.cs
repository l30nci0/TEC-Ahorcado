using System;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

using LetterClashClient.Services;
using LetterClashServer.Domain.Models;

namespace LetterClashClient.Views {
  public partial class GUISelectWordView : Page {
    private string selectedLanguage;
    private int privacyIndex;

    public GUISelectWordView() : this(Idiomas.ESPANOL, 1) { }

    public GUISelectWordView(string selectedLanguage, int privacyIndex) {
      InitializeComponent();
      this.selectedLanguage = selectedLanguage;
      this.privacyIndex = privacyIndex;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = "Crea Lobby (Escoger palabra)";
      }

      TextBlockHeader.Text = $"Palabras en {selectedLanguage.ToLower()}";

      try {
        var palabraService = ServiceProxyManager.GetPalabraService();
        var result = palabraService.ObtenerPalabrasPorIdioma(selectedLanguage);

        if (result != null && result.IsSuccess) {
          DataGridWords.ItemsSource = result.Value;
        } else {
          MessageBox.Show(result?.Error?.Mensaje ?? "Error al recuperar palabras del catálogo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      } catch (CommunicationException) {
        MessageBox.Show("No se pudo conectar con el servidor para obtener las palabras.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
      } catch (Exception ex) {
        MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void ButtonUseWord_Click(object sender, RoutedEventArgs e) {
      Button button = sender as Button;

      if (button != null) {
        var selectedWord = button.DataContext as PalabraDTO;

        if (selectedWord != null) {
          NavigationService.Navigate(new GUICreateRoomView(selectedWord, selectedLanguage, privacyIndex));
        }
      }
    }

    private void ButtonBack_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUICreateRoomView(null, selectedLanguage, privacyIndex));
    }
  }
}
