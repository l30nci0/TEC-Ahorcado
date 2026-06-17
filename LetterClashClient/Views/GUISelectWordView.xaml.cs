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
        window.Title = (string) Application.Current.FindResource("SelectWord_WindowTitle") ?? "Crea Lobby (Escoger palabra)";
      }

      LoadWords();
    }

    private void LoadWords() {
      // Update header text based on selected language
      string langStr = selectedLanguage == Idiomas.INGLES
        ? (string) Application.Current.FindResource("SelectWord_LangNameEN") ?? "Inglés"
        : (string) Application.Current.FindResource("SelectWord_LangNameES") ?? "Español";
      string headerTemplate = (string) Application.Current.FindResource("SelectWord_HeaderTemplate") ?? "Palabras en {0}";
      TextBlockHeader.Text = string.Format(headerTemplate, langStr.ToLower());

      // Dynamically toggle styles of the segmented buttons
      if (selectedLanguage == Idiomas.INGLES) {
        ButtonLangEN.Style = (Style) FindResource("ModernPrimaryButton");
        ButtonLangES.Style = (Style) FindResource("ModernSecondaryButton");
      } else {
        ButtonLangES.Style = (Style) FindResource("ModernPrimaryButton");
        ButtonLangEN.Style = (Style) FindResource("ModernSecondaryButton");
      }

      try {
        var palabraService = ServiceProxyManager.GetPalabraService();
        var result = palabraService.ObtenerPalabrasPorIdioma(selectedLanguage);

        if (result != null && result.IsSuccess) {
          DataGridWords.ItemsSource = result.Value;
        } else {
          string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
          string errRetrieve = (string) Application.Current.FindResource("SelectWord_ErrorRetrieve") ?? "Error al recuperar palabras del catálogo.";
          MessageBox.Show(result?.Error?.Mensaje ?? errRetrieve, errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
      } catch (CommunicationException) {
        string connTitle = (string) Application.Current.FindResource("Msg_ConnectionErrorTitle") ?? "Error de Conexión";
        string connMsg = (string) Application.Current.FindResource("SelectWord_ErrorConnection") ?? "No se pudo conectar con el servidor para obtener las palabras.";
        MessageBox.Show(connMsg, connTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      } catch (Exception ex) {
        string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        string unexpMsg = (string) Application.Current.FindResource("Msg_UnexpectedError") ?? "Ocurrió un error inesperado:";
        MessageBox.Show($"{unexpMsg} {ex.Message}", errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void ButtonLangES_Click(object sender, RoutedEventArgs e) {
      if (selectedLanguage != Idiomas.ESPANOL) {
        selectedLanguage = Idiomas.ESPANOL;
        LoadWords();
      }
    }

    private void ButtonLangEN_Click(object sender, RoutedEventArgs e) {
      if (selectedLanguage != Idiomas.INGLES) {
        selectedLanguage = Idiomas.INGLES;
        LoadWords();
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
