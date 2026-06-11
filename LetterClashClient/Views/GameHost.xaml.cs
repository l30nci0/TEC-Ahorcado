using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LetterClashClient.Views {
  public partial class GameHost : Page {
    private string opponentName;
    private string selectedWord;
    private int currentHangmanState;

    public GameHost() {
      InitializeComponent();
      opponentName = "Usuario 1";
      selectedWord = "SOFTWARE";
      currentHangmanState = 5;
    }

    public GameHost(string selectedWord) {
      InitializeComponent();
      opponentName = "Usuario 1";
      this.selectedWord = string.IsNullOrWhiteSpace(selectedWord) ? "SOFTWARE" : selectedWord.ToUpper();
      currentHangmanState = 5;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = "Partida (Anfitrión)";
      }

      TextBlockOpponent.Text = opponentName;
      TextBlockWord.Text = selectedWord;
      TextBlockChatTitle.Text = $"Chat con {opponentName}";
      UpdateHangmanImage();
      UpdateAttempts();
    }

    private void ButtonAddBodyPart_Click(object sender, RoutedEventArgs e) {
      if (currentHangmanState > 1) {
        currentHangmanState--;
        UpdateHangmanImage();
        UpdateAttempts();
      }

      if (currentHangmanState == 1) {
        MessageBox.Show("El cuerpo humano ha sido completado. Ganaste la partida.",
                        "TecnoHorcado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

        NavigationService.Navigate(new MainMenu());
      }
    }

    private void ButtonRemoveBodyPart_Click(object sender, RoutedEventArgs e) {
      if (currentHangmanState < 5) {
        currentHangmanState++;
        UpdateHangmanImage();
        UpdateAttempts();
      }
    }

    private void UpdateHangmanImage() {
      ImageHangman.Source = new BitmapImage(new Uri($"/Assets/Images/Hangedman{currentHangmanState}.jpg", UriKind.Relative));
    }

    private void UpdateAttempts() {
      int attempts = 5 - currentHangmanState;
      ProgressBarAttempts.Value = attempts;
      TextBlockAttempts.Text = $"{attempts}/5";
    }

    private void ButtonSendMessage_Click(object sender, RoutedEventArgs e) {
      string message = TextBoxChatInput.Text.Trim();

      if (string.IsNullOrWhiteSpace(message)) {
        return;
      }

      TextBoxChatMessages.Text += $"\nTú: {message}";
      TextBoxChatInput.Text = "";
      TextBoxChatMessages.ScrollToEnd();
    }

    private void ButtonAbandon_Click(object sender, RoutedEventArgs e) {
      MessageBoxResult result = MessageBox.Show("¿Seguro que deseas abandonar la partida?",
                                                "TecnoHorcado",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Warning);

      if (result == MessageBoxResult.Yes) {
        NavigationService.Navigate(new MainMenu());
      }
    }
  }
}
