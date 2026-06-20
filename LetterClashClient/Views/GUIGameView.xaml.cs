using System;
using System.Linq;
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
    private int idPalabra;
    private string wordDescription;

    public GUIGameView() : this(new PalabraDTO { PalabraTexto = "SOFTWARE", Descripcion = "Conjunto de programas y rutinas que permiten a la computadora realizar determinadas tareas.", Idioma = Idiomas.ESPANOL }, "000000") { }

    // Host constructor
    public GUIGameView(PalabraDTO selectedWord, string codigoAcceso) {
      InitializeComponent();
      this.isHost = true;
      this.opponentName = "Usuario 1";
      this.targetWord = selectedWord != null && !string.IsNullOrWhiteSpace(selectedWord.PalabraTexto) 
          ? selectedWord.PalabraTexto.ToUpper() 
          : "SOFTWARE";
      this.wordDescription = selectedWord != null ? selectedWord.Descripcion : "";
      this.selectedLanguage = selectedWord != null ? selectedWord.Idioma : Idiomas.ESPANOL;
      this.codigoAcceso = string.IsNullOrWhiteSpace(codigoAcceso) ? "------" : codigoAcceso;
      this.mistakes = 0;
    }

    // Guesser constructor
    public GUIGameView(string opponentName, string selectedLanguage, string codigoAcceso, int idPalabra) {
      InitializeComponent();
      this.isHost = false;
      this.opponentName = opponentName;
      this.selectedLanguage = selectedLanguage;
      this.codigoAcceso = codigoAcceso;
      this.idPalabra = idPalabra;
      this.targetWord = "SOFTWARE";
      this.mistakes = 0;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (isHost) {
        if (window != null) {
          window.Title = (string) Application.Current.FindResource("Game_HostWindowTitle") ?? "Partida (Anfitrión)";
        }
        GridHostSection.Visibility = Visibility.Visible;
        GridGuesserSection.Visibility = Visibility.Collapsed;

        TextBlockWord.Text = string.Join(" ", targetWord.ToCharArray());
        TextBlockAccessCode.Text = codigoAcceso;
        TextBlockSelectedLetter.Text = "-";
        
        TextBlockLanguageHost.Text = selectedLanguage == Idiomas.INGLES ? "ENGLISH" : "ESPAÑOL";
        TextBlockWordDescriptionHost.Text = wordDescription;
      } else {
        if (window != null) {
          window.Title = (string) Application.Current.FindResource("Game_GuesserWindowTitle") ?? "Partida (Adivinador)";
        }
        GridHostSection.Visibility = Visibility.Collapsed;
        GridGuesserSection.Visibility = Visibility.Visible;

        TextBlockAccessCodeGuesser.Text = codigoAcceso;
        TextBlockLanguageGuesser.Text = selectedLanguage == Idiomas.INGLES ? "ENGLISH" : "ESPAÑOL";
        CargarPistaAdivinador();

        guessedWord = new char[targetWord.Length];
        for (int index = 0; index < guessedWord.Length; index++) {
          guessedWord[index] = '_';
        }
        UpdateHiddenWord();
      }

      TextBlockOpponent.Text = opponentName;
      string chatTitle = (string) Application.Current.FindResource("Game_ChatTitleTemplate") ?? "Chat con {0}";
      TextBlockChatTitle.Text = string.Format(chatTitle, opponentName);
      TextBoxChatMessages.Text = "";
      UpdateHangmanImage();
      UpdateAttempts();

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario == null) {
        string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        string invalidSession = (string) Application.Current.FindResource("Msg_InvalidUserSession") ?? "Sesión de usuario inválida.";
        MessageBox.Show(invalidSession, errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        NavigationService.Navigate(new GUIGameHubView());
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
        string connTitle = (string) Application.Current.FindResource("Msg_ConnectionErrorTitle") ?? "Error de Conexión";
        string connMsg = (string) Application.Current.FindResource("Game_ErrorConnect") ?? "Error al conectar al servidor:";
        MessageBox.Show($"{connMsg} {ex.Message}", connTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        NavigationService.Navigate(new GUIGameHubView());
      }
    }

    private void OnJugadorSeUnio(JugadorPublicoDTO jugadorDTO) {
      Dispatcher.Invoke(() => {
        opponentName = jugadorDTO.NombreDeUsuario;
        TextBlockOpponent.Text = opponentName;
        string chatTitle = (string) Application.Current.FindResource("Game_ChatTitleTemplate") ?? "Chat con {0}";
        TextBlockChatTitle.Text = string.Format(chatTitle, opponentName);
        AvatarHelper.AsignarAImageControl(ImageOpponentAvatar, jugadorDTO.Avatar);
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
        string wonMsg = (string) Application.Current.FindResource("Game_ResultWon") ?? "¡Felicidades! Ganaste la partida y obtuviste {0} puntos.";
        string lostMsg = (string) Application.Current.FindResource("Game_ResultLost") ?? "El juego ha concluido. Ganador: {0}.";
        string resultTitle = (string) Application.Current.FindResource("Game_ResultTitle") ?? "Partida Finalizada";

        string mensajeResult = (ganador == SessionContext.UsuarioLogueado?.NombreDeUsuario)
            ? string.Format(wonMsg, puntuacionObtenida)
            : string.Format(lostMsg, ganador);

        MessageBox.Show(mensajeResult, resultTitle, MessageBoxButton.OK, MessageBoxImage.Information);
        NavigationService.Navigate(new GUIGameHubView());
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
        string abandonHost = (string) Application.Current.FindResource("Game_AbandonHost") ?? "El adivinador ({0}) ha abandonado la partida. ¡Ganaste por abandono (+50 puntos)!";
        string abandonGuesser = (string) Application.Current.FindResource("Game_AbandonGuesser") ?? "El anfitrión ({0}) ha abandonado la partida. ¡Ganaste por abandono (+50 puntos)!";
        string resultTitle = (string) Application.Current.FindResource("Game_ResultTitle") ?? "Partida Finalizada";

        string abandonMsg = isHost
            ? string.Format(abandonHost, oponenteNombre)
            : string.Format(abandonGuesser, oponenteNombre);
        MessageBox.Show(abandonMsg, resultTitle, MessageBoxButton.OK, MessageBoxImage.Information);
        NavigationService.Navigate(new GUIGameHubView());
      });
    }

    private void OnErrorOcurrido(ServiceFault fault) {
      Dispatcher.Invoke(() => {
        DesconectarJuego();
        string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        string serverErr = (string) Application.Current.FindResource("Game_ServerError") ?? "Ocurrió un error en el servidor: {0}";
        MessageBox.Show(string.Format(serverErr, fault.Mensaje), errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        NavigationService.Navigate(new GUIGameHubView());
      });
    }

    private void DesconectarJuego() {
      if (gameServiceProxy != null) {
        try {
          var clientChannel = (System.ServiceModel.ICommunicationObject) gameServiceProxy;
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
          string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
          string letterErr = (string) Application.Current.FindResource("Game_ErrorLetter") ?? "No se pudo registrar la letra en el servidor: {0}";
          MessageBox.Show(string.Format(letterErr, ex.Message), errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }

    private void UpdateHiddenWord() {
      TextBlockHiddenWord.Text = string.Join(" ", guessedWord);
    }

    private void CargarPistaAdivinador() {
      TextBlockWordDescription.Text = "";

      if (idPalabra <= 0) {
        return;
      }

      try {
        var palabraService = ServiceProxyManager.GetPalabraService();
        var result = palabraService.ObtenerPalabrasPorIdioma(selectedLanguage);
        if (result == null || !result.IsSuccess) {
          return;
        }

        var palabra = result.Value.FirstOrDefault(p => p.IDPalabra == idPalabra);
        if (palabra != null) {
          this.wordDescription = palabra.Descripcion;
          TextBlockWordDescription.Text = palabra.Descripcion;
        }
      } catch {
        // Ignorar o registrar error
      }
    }

    private void UpdateHangmanImage() {
      int imageNumber = 6 - mistakes;
      if (imageNumber < 1) {
        imageNumber = 1;
      }
      ImageHangman.Source = new BitmapImage(new Uri($"/Assets/Images/Hangedman{imageNumber}.png", UriKind.Relative));
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
          string youStr = (string) Application.Current.FindResource("Game_ChatYou") ?? "Tú";
          TextBoxChatMessages.Text += $"\n{youStr}: {message}";
          TextBoxChatInput.Text = "";
          TextBoxChatMessages.ScrollToEnd();
        } catch (Exception ex) {
          string connTitle = (string) Application.Current.FindResource("Msg_ConnectionErrorTitle") ?? "Error de Conexión";
          string sendErr = (string) Application.Current.FindResource("Game_ErrorSend") ?? "No se pudo enviar el mensaje: {0}";
          MessageBox.Show(string.Format(sendErr, ex.Message), connTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }

    private void ButtonAbandon_Click(object sender, RoutedEventArgs e) {
      string confirmMsg = (string) Application.Current.FindResource("Game_ConfirmAbandon") ?? "¿Seguro que deseas abandonar la partida?";
      MessageBoxResult result = MessageBox.Show(confirmMsg,
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
        NavigationService.Navigate(new GUIGameHubView());
      }
    }
  }
}
