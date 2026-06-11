using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using LetterClashClient.Models;
using LetterClashClient.Services;
using LetterClashServer.Contracts;
using LetterClashServer.Domain.Models;

namespace LetterClashClient.Views {
  public partial class GUIGameView : Page {
    private bool isHost;
    private string opponentName;
    private string selectedLanguage;
    private string targetWord;
    private char[] guessedWord;
    private int mistakes;
    private readonly string codigoAcceso;
    private IGameService gameServiceProxy;
    private GameCallbackHandler callbackHandler;

    public GUIGameView() : this("SOFTWARE", "000000") { }

    // Host constructor
    public GUIGameView(string selectedWord, string codigoAcceso) {
      InitializeComponent();
      this.isHost = true;
      this.opponentName = "Usuario 1";
      this.targetWord = string.IsNullOrWhiteSpace(selectedWord) ? "SOFTWARE" : selectedWord.ToUpper();
      this.codigoAcceso = string.IsNullOrWhiteSpace(codigoAcceso) ? "------" : codigoAcceso;
      this.mistakes = 0;
    }

    // Guesser constructor
    public GUIGameView(string opponentName, string selectedLanguage, string codigoAcceso) {
      InitializeComponent();
      this.isHost = false;
      this.opponentName = opponentName;
      this.selectedLanguage = selectedLanguage;
      this.codigoAcceso = codigoAcceso;
      this.targetWord = "SOFTWARE";
      this.mistakes = 0;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (isHost) {
        if (window != null) {
          window.Title = "Partida (Anfitrión)";
        }
        GridHostSection.Visibility = Visibility.Visible;
        GridGuesserSection.Visibility = Visibility.Collapsed;

        TextBlockWord.Text = string.Join(" ", targetWord.ToCharArray());
        TextBlockAccessCode.Text = codigoAcceso;
        TextBlockSelectedLetter.Text = "-";
      } else {
        if (window != null) {
          window.Title = "Partida (Adivinador)";
        }
        GridHostSection.Visibility = Visibility.Collapsed;
        GridGuesserSection.Visibility = Visibility.Visible;

        guessedWord = new char[targetWord.Length];
        for (int index = 0; index < guessedWord.Length; index++) {
          guessedWord[index] = '_';
        }
        UpdateHiddenWord();
      }

      TextBlockOpponent.Text = opponentName;
      TextBlockChatTitle.Text = $"Chat con {opponentName}";
      TextBoxChatMessages.Text = "";
      UpdateHangmanImage();
      UpdateAttempts();

      // Cargar avatar local
      var usuario = SessionContext.UsuarioLogueado;
      if (usuario != null && usuario.Avatar != null && usuario.Avatar.Length > 0) {
        try {
          var image = new BitmapImage();
          using (var mem = new System.IO.MemoryStream(usuario.Avatar)) {
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = mem;
            image.EndInit();
          }
          ImageUserAvatar.Source = image;
        } catch { }
      }

      if (usuario == null) {
        MessageBox.Show("Sesión de usuario inválida.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        NavigationService.Navigate(new GUIMainMenuView());
        return;
      }

      // Conectar al servicio dúplex
      try {
        callbackHandler = new GameCallbackHandler();
        callbackHandler.JugadorSeUnio += OnJugadorSeUnio;
        callbackHandler.LetraPropuesta += OnLetraPropuesta;
        callbackHandler.PartidaFinalizada += OnPartidaFinalizada;
        callbackHandler.MensajeRecibido += OnMensajeRecibido;
        callbackHandler.OponenteAbandono += OnOponenteAbandono;
        callbackHandler.ErrorOcurrido += OnErrorOcurrido;

        gameServiceProxy = ServiceProxyManager.GetGameService(callbackHandler);
        gameServiceProxy.ConectarJuego(usuario.IDJugador, codigoAcceso);
      } catch (Exception ex) {
        MessageBox.Show($"Error al conectar al servidor: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
        NavigationService.Navigate(new GUIMainMenuView());
      }
    }

    private void OnJugadorSeUnio(JugadorPublicoDTO jugadorDTO) {
      Dispatcher.Invoke(() => {
        opponentName = jugadorDTO.NombreDeUsuario;
        TextBlockOpponent.Text = opponentName;
        TextBlockChatTitle.Text = $"Chat con {opponentName}";
        if (jugadorDTO.Avatar != null && jugadorDTO.Avatar.Length > 0) {
          try {
            var image = new BitmapImage();
            using (var mem = new System.IO.MemoryStream(jugadorDTO.Avatar)) {
              image.BeginInit();
              image.CacheOption = BitmapCacheOption.OnLoad;
              image.StreamSource = mem;
              image.EndInit();
            }
            ImageOpponentAvatar.Source = image;
          } catch { }
        }
      });
    }

    private void OnLetraPropuesta(char letra, bool esCorrecta, string palabraRevelada, int vidaRestante) {
      Dispatcher.Invoke(() => {
        targetWord = palabraRevelada;
        mistakes = 5 - vidaRestante;

        if (isHost) {
          if (letra != '\0') {
            TextBlockSelectedLetter.Text = letra.ToString().ToUpper();
          } else {
            TextBlockSelectedLetter.Text = "-";
          }
          TextBlockWord.Text = string.Join(" ", palabraRevelada.ToCharArray());
        } else {
          guessedWord = palabraRevelada.ToCharArray();
          UpdateHiddenWord();
          if (letra != '\0') {
            DeshabilitarBotonLetra(letra);
          }
        }

        UpdateHangmanImage();
        UpdateAttempts();
      });
    }

    private void OnPartidaFinalizada(string ganador, int puntuacionObtenida) {
      Dispatcher.Invoke(() => {
        DesconectarJuego();
        string mensajeResult = (ganador == SessionContext.UsuarioLogueado?.NombreDeUsuario)
            ? $"¡Felicidades! Ganaste la partida y obtuviste {puntuacionObtenida} puntos."
            : $"El juego ha concluido. Ganador: {ganador}.";

        MessageBox.Show(mensajeResult, "Partida Finalizada", MessageBoxButton.OK, MessageBoxImage.Information);
        NavigationService.Navigate(new GUIMainMenuView());
      });
    }

    private void OnMensajeRecibido(string emisor, string mensaje) {
      Dispatcher.Invoke(() => {
        TextBoxChatMessages.Text += $"\n{emisor}: {mensaje}";
        TextBoxChatMessages.ScrollToEnd();
      });
    }

    private void OnOponenteAbandono(string oponenteNombre) {
      Dispatcher.Invoke(() => {
        DesconectarJuego();
        string abandonMsg = isHost
            ? $"El adivinador ({oponenteNombre}) ha abandonado la partida. ¡Ganaste por abandono (+50 puntos)!"
            : $"El anfitrión ({oponenteNombre}) ha abandonado la partida. ¡Ganaste por abandono (+50 puntos)!";
        MessageBox.Show(abandonMsg, "Partida Finalizada", MessageBoxButton.OK, MessageBoxImage.Information);
        NavigationService.Navigate(new GUIMainMenuView());
      });
    }

    private void OnErrorOcurrido(ServiceFault fault) {
      Dispatcher.Invoke(() => {
        DesconectarJuego();
        MessageBox.Show($"Ocurrió un error en el servidor: {fault.Mensaje}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        NavigationService.Navigate(new GUIMainMenuView());
      });
    }

    private void DesconectarJuego() {
      if (gameServiceProxy != null) {
        try {
          var clientChannel = (System.ServiceModel.ICommunicationObject)gameServiceProxy;
          if (clientChannel.State == System.ServiceModel.CommunicationState.Opened) {
            clientChannel.Close();
          }
        } catch { }
        gameServiceProxy = null;
      }
    }

    private void DeshabilitarBotonLetra(char letra) {
      string letraStr = letra.ToString().ToUpper();
      BuscarYDeshabilitarBoton(GridKeyboard, letraStr);
    }

    private bool BuscarYDeshabilitarBoton(DependencyObject parent, string letra) {
      int count = VisualTreeHelper.GetChildrenCount(parent);
      for (int i = 0; i < count; i++) {
        var child = VisualTreeHelper.GetChild(parent, i);
        if (child is Button button && button.Content != null && button.Content.ToString().ToUpper() == letra) {
          button.IsEnabled = false;
          return true;
        }
        if (BuscarYDeshabilitarBoton(child, letra)) {
          return true;
        }
      }
      return false;
    }

    private void ButtonLetter_Click(object sender, RoutedEventArgs e) {
      Button button = sender as Button;

      if (button == null) {
        return;
      }

      string content = button.Content.ToString();

      if (string.IsNullOrWhiteSpace(content)) {
        return;
      }

      char selectedLetter = content[0];

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario != null && gameServiceProxy != null) {
        try {
          gameServiceProxy.EscribirLetra(codigoAcceso, usuario.IDJugador, selectedLetter);
        } catch (Exception ex) {
          MessageBox.Show($"No se pudo registrar la letra en el servidor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }

    private void UpdateHiddenWord() {
      TextBlockHiddenWord.Text = string.Join(" ", guessedWord);
    }

    private void UpdateHangmanImage() {
      int imageNumber = 5 - mistakes;
      if (imageNumber < 1) {
        imageNumber = 1;
      }
      ImageHangman.Source = new BitmapImage(new Uri($"/Assets/Images/Hangedman{imageNumber}.jpg", UriKind.Relative));
    }

    private void UpdateAttempts() {
      ProgressBarAttempts.Value = mistakes;
      TextBlockAttempts.Text = $"{mistakes}/5";
    }

    private void ButtonSendMessage_Click(object sender, RoutedEventArgs e) {
      string message = TextBoxChatInput.Text.Trim();

      if (string.IsNullOrWhiteSpace(message)) {
        return;
      }

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario != null && gameServiceProxy != null) {
        try {
          gameServiceProxy.EnviarMensaje(codigoAcceso, usuario.IDJugador, message);
          TextBoxChatMessages.Text += $"\nTú: {message}";
          TextBoxChatInput.Text = "";
          TextBoxChatMessages.ScrollToEnd();
        } catch (Exception ex) {
          MessageBox.Show($"No se pudo enviar el mensaje: {ex.Message}", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }

    private void ButtonAbandon_Click(object sender, RoutedEventArgs e) {
      MessageBoxResult result = MessageBox.Show("¿Seguro que deseas abandonar la partida?",
                                                "TecnoHorcado",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Warning);

      if (result == MessageBoxResult.Yes) {
        var usuario = SessionContext.UsuarioLogueado;
        if (usuario != null && gameServiceProxy != null) {
          try {
            gameServiceProxy.AbandonarPartida(codigoAcceso, usuario.IDJugador);
          } catch { }
        }
        DesconectarJuego();
        NavigationService.Navigate(new GUIMainMenuView());
      }
    }
  }
}
