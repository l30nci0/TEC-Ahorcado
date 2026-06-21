using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using LetterClashClient.Models;
using LetterClashClient.Properties;
using LetterClashClient.Services;

using LetterClashServer.Contracts;
using LetterClashServer.Domain.Models;

namespace LetterClashClient.Views {
  public partial class GUIGameView : Page {
    private const int MaxMistakes = 6;
    private static readonly int[] HangmanImageByMistakes = { 6, 5, 4, 3, 2, 7, 1 };
    private bool isHost;
    private string opponentName;
    private string selectedLanguage;
    private string targetWord;
    private string fullTargetWord;
    private char[] guessedWord;
    private int mistakes;
    private readonly string codigoAcceso;
    private IGameService gameServiceProxy;
    private GameCallbackHandler callbackHandler;
    private int idPalabra;
    private string wordDescription;
    private int pistasUsadas = 0;
    private int pistasMaximas = 0;
    private bool partidaCerrada;
    private bool abandonoEnviado;
    private bool esPartidaPublica;
    private Window hostWindow;

    public GUIGameView() : this(new PalabraDTO { PalabraTexto = "SOFTWARE", Descripcion = "Conjunto de programas y rutinas que permiten a la computadora realizar determinadas tareas.", Idioma = Idiomas.ESPANOL }, "000000") { }

    // Host constructor
    public GUIGameView(PalabraDTO selectedWord, string codigoAcceso) : this(selectedWord, codigoAcceso, false) { }

    public GUIGameView(PalabraDTO selectedWord, string codigoAcceso, bool esPartidaPublica) {
      InitializeComponent();
      this.isHost = true;
      this.opponentName = null;
      this.targetWord = selectedWord != null && !string.IsNullOrWhiteSpace(selectedWord.PalabraTexto) 
          ? selectedWord.PalabraTexto.ToUpper() 
          : "SOFTWARE";
      this.fullTargetWord = this.targetWord;
      this.wordDescription = selectedWord != null ? selectedWord.Descripcion : "";
      this.selectedLanguage = selectedWord != null ? selectedWord.Idioma : Idiomas.ESPANOL;
      this.codigoAcceso = string.IsNullOrWhiteSpace(codigoAcceso) ? "------" : codigoAcceso;
      this.esPartidaPublica = esPartidaPublica;
      this.mistakes = 0;
    }

    // Guesser constructor
    public GUIGameView(string opponentName, string selectedLanguage, string codigoAcceso, int idPalabra) : this(opponentName, selectedLanguage, codigoAcceso, idPalabra, false) { }

    public GUIGameView(string opponentName, string selectedLanguage, string codigoAcceso, int idPalabra, bool esPartidaPublica) {
      InitializeComponent();
      this.isHost = false;
      this.opponentName = opponentName;
      this.selectedLanguage = selectedLanguage;
      this.codigoAcceso = codigoAcceso;
      this.idPalabra = idPalabra;
      this.esPartidaPublica = esPartidaPublica;
      this.targetWord = "SOFTWARE";
      this.fullTargetWord = "SOFTWARE";
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
        StackPanelAccessCodeHost.Visibility = esPartidaPublica ? Visibility.Collapsed : Visibility.Visible;
        TextBlockAccessCode.Text = esPartidaPublica ? "" : codigoAcceso;
        ReiniciarTecladoHost();
        
        TextBlockLanguageHost.Text = selectedLanguage == Idiomas.INGLES ? "ENGLISH" : "ESPAÑOL";
        TextBlockWordDescriptionHost.Text = wordDescription;
      } else {
        if (window != null) {
          window.Title = (string) Application.Current.FindResource("Game_GuesserWindowTitle") ?? "Partida (Adivinador)";
        }
        GridHostSection.Visibility = Visibility.Collapsed;
        GridGuesserSection.Visibility = Visibility.Visible;

        StackPanelAccessCodeGuesser.Visibility = esPartidaPublica ? Visibility.Collapsed : Visibility.Visible;
        TextBlockAccessCodeGuesser.Text = esPartidaPublica ? "" : codigoAcceso;
        TextBlockLanguageGuesser.Text = selectedLanguage == Idiomas.INGLES ? "ENGLISH" : "ESPAÑOL";
        CargarPistaAdivinador();

        guessedWord = new char[targetWord.Length];
        for (int index = 0; index < guessedWord.Length; index++) {
          guessedWord[index] = '_';
        }
        UpdateHiddenWord();
      }

      ActualizarOponenteEnPantalla();
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
        callbackHandler.OponenteDesconectado += OnOponenteDesconectado;
        callbackHandler.ErrorOcurrido += OnErrorOcurrido;

        gameServiceProxy = ServiceProxyManager.GetGameService(callbackHandler);
        gameServiceProxy.ConectarJuego(usuario.IDJugador, codigoAcceso);
        RegistrarCierreVentana(window);
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
        ActualizarOponenteEnPantalla();
        AvatarHelper.AsignarAImageControl(ImageOpponentAvatar, jugadorDTO.Avatar);
      });
    }

    private void OnLetraPropuesta(char letra, bool esCorrecta, string palabraRevelada, int vidaRestante) {
      Dispatcher.Invoke(() => {
        targetWord = palabraRevelada;
        mistakes = Math.Max(0, Math.Min(MaxMistakes, MaxMistakes - vidaRestante));

        if (isHost) {
          if (letra != '\0') {
            MarcarLetraHost(letra, esCorrecta);
          }
          TextBlockWord.Text = string.Join(" ", palabraRevelada.ToCharArray());
        } else {
          guessedWord = palabraRevelada.ToCharArray();
          UpdateHiddenWord();
          if (letra != '\0') {
            DeshabilitarBotonLetra(letra);
          }
          ActualizarEstadoPistas();
        }

        UpdateHangmanImage();
        UpdateAttempts();
        ReproducirEfectoLetra(letra, esCorrecta, palabraRevelada, vidaRestante);
      });
    }

    private void OnPartidaFinalizada(string ganador, int puntuacionObtenida) {
      Dispatcher.Invoke(() => {
        partidaCerrada = true;
        DesconectarJuego();
        string wonMsg = (string) Application.Current.FindResource("Game_ResultWon") ?? "¡Felicidades! Ganaste la partida y obtuviste {0} puntos.";
        string lostMsg = (string) Application.Current.FindResource("Game_ResultLost") ?? "El juego ha concluido. Ganador: {0}.";
        string resultTitle = (string) Application.Current.FindResource("Game_ResultTitle") ?? "Partida Finalizada";

        string mensajeResult = (ganador == SessionContext.UsuarioLogueado?.NombreDeUsuario)
            ? string.Format(wonMsg, puntuacionObtenida)
            : string.Format(lostMsg, ganador);

        ReproducirEfectoResultado(ganador);
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
        partidaCerrada = true;
        DesconectarJuego();
        string abandonHost = (string) Application.Current.FindResource("Game_AbandonHost") ?? "El adivinador ({0}) ha abandonado la partida. ¡Ganaste por abandono (+50 puntos)!";
        string abandonGuesser = (string) Application.Current.FindResource("Game_AbandonGuesser") ?? "El anfitrión ({0}) ha abandonado la partida. ¡Ganaste por abandono (+50 puntos)!";
        string resultTitle = (string) Application.Current.FindResource("Game_ResultTitle") ?? "Partida Finalizada";

        string abandonMsg = isHost
            ? string.Format(abandonHost, oponenteNombre)
            : string.Format(abandonGuesser, oponenteNombre);
        AgregarMensajeSistema(abandonMsg);
        RegresarAlMenuDespuesDeAviso();
      });
    }

    private void OnOponenteDesconectado(string oponenteNombre) {
      Dispatcher.Invoke(() => {
        partidaCerrada = true;
        DesconectarJuego();
        string disconnectedMsg = (string) Application.Current.FindResource("Game_OpponentDisconnected") ?? "{0} se desconecto. La partida fue cerrada. Regresando al menu inicial...";
        AgregarMensajeSistema(string.Format(disconnectedMsg, oponenteNombre));
        RegresarAlMenuDespuesDeAviso();
      });
    }

    private void OnErrorOcurrido(ServiceFault fault) {
      Dispatcher.Invoke(() => {
        partidaCerrada = true;
        DesconectarJuego();
        if (fault != null && !string.IsNullOrWhiteSpace(fault.Detalle) && fault.Detalle.Contains("Timeout de inactividad")) {
          string inactivityMsg = (string) Application.Current.FindResource("Game_InactivityPenalty") ?? "La partida se cerro por inactividad. Se aplico una penalizacion de -3 puntos.";
          string resultTitle = (string) Application.Current.FindResource("Game_ResultTitle") ?? "Partida Finalizada";
          MessageBox.Show(inactivityMsg, resultTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
          NavigationService.Navigate(new GUILobbyView());
          return;
        }

        string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        string serverErr = (string) Application.Current.FindResource("Game_ServerError") ?? "Ocurrió un error en el servidor: {0}";
        MessageBox.Show(string.Format(serverErr, fault.Mensaje), errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        NavigationService.Navigate(new GUIGameHubView());
      });
    }

    private void RegistrarCierreVentana(Window window) {
      if (window == null || hostWindow != null) {
        return;
      }

      hostWindow = window;
      hostWindow.Closing += Window_Closing;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      EnviarAbandonoVoluntario();
    }

    private void AgregarMensajeSistema(string mensaje) {
      string systemName = (string) Application.Current.FindResource("Game_SystemSender") ?? "Sistema";
      TextBoxChatMessages.Text += $"\n{systemName}: {mensaje}";
      TextBoxChatMessages.ScrollToEnd();
    }

    private void ActualizarOponenteEnPantalla() {
      bool esperandoJugador = string.IsNullOrWhiteSpace(opponentName);
      string nombreVisible = esperandoJugador ? "Esperando jugador..." : opponentName;
      TextBlockOpponent.Text = nombreVisible;
      ImageOpponentAvatar.Visibility = esperandoJugador ? Visibility.Hidden : Visibility.Visible;

      string chatTitle = (string) Application.Current.FindResource("Game_ChatTitleTemplate") ?? "Chat con {0}";
      TextBlockChatTitle.Text = esperandoJugador ? "Esperando jugador..." : string.Format(chatTitle, opponentName);
    }

    private void ReiniciarTecladoHost() {
      AplicarAButtons(GridHostKeyboard, button => {
        button.Background = CrearBrocha("#111827");
        button.Foreground = CrearBrocha("#8A8F98");
        button.BorderBrush = CrearBrocha("#3A3F48");
      });
    }

    private void MarcarLetraHost(char letra, bool esCorrecta) {
      Button button = BuscarBotonPorContenido(GridHostKeyboard, letra.ToString().ToUpper());
      if (button == null) {
        return;
      }

      string color = esCorrecta ? "#2DD36F" : "#FF2D66";
      button.Background = CrearBrocha(color);
      button.BorderBrush = CrearBrocha(color);
      button.Foreground = CrearBrocha("#050D14");
    }

    private Button BuscarBotonPorContenido(DependencyObject parent, string contenido) {
      if (parent == null) {
        return null;
      }

      int count = VisualTreeHelper.GetChildrenCount(parent);
      for (int index = 0; index < count; index++) {
        DependencyObject child = VisualTreeHelper.GetChild(parent, index);
        if (child is Button button && button.Content != null && button.Content.ToString().ToUpper() == contenido) {
          return button;
        }

        Button found = BuscarBotonPorContenido(child, contenido);
        if (found != null) {
          return found;
        }
      }

      return null;
    }

    private void AplicarAButtons(DependencyObject parent, Action<Button> action) {
      if (parent == null || action == null) {
        return;
      }

      int count = VisualTreeHelper.GetChildrenCount(parent);
      for (int index = 0; index < count; index++) {
        DependencyObject child = VisualTreeHelper.GetChild(parent, index);
        if (child is Button button) {
          action(button);
        }

        AplicarAButtons(child, action);
      }
    }

    private SolidColorBrush CrearBrocha(string color) {
      return new SolidColorBrush((Color) ColorConverter.ConvertFromString(color));
    }

    private void RegresarAlMenuDespuesDeAviso() {
      ButtonSendMessage.IsEnabled = false;
      ButtonAbandon.IsEnabled = false;

      var timer = new DispatcherTimer {
        Interval = TimeSpan.FromSeconds(3)
      };
      timer.Tick += (sender, args) => {
        timer.Stop();
        NavigationService?.Navigate(new GUILobbyView());
      };
      timer.Start();
    }

    private void EnviarAbandonoVoluntario() {
      if (partidaCerrada || abandonoEnviado) {
        return;
      }

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario == null || gameServiceProxy == null) {
        return;
      }

      abandonoEnviado = true;
      partidaCerrada = true;

      try {
        gameServiceProxy.AbandonarPartida(codigoAcceso, usuario.IDJugador);
        RegistrarAvisoPenalizacionPendiente();
      } catch { }
    }

    private void RegistrarAvisoPenalizacionPendiente() {
      try {
        Settings.Default.PenalizacionAbandonoPendiente = true;
        Settings.Default.Save();
      } catch { }
    }

    private void DesconectarJuego() {
      if (hostWindow != null) {
        hostWindow.Closing -= Window_Closing;
        hostWindow = null;
      }

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
          if (!isHost && EsCanalFallido(gameServiceProxy)) {
            ManejarFalloAdivinadorPorInactividad();
            return;
          }

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
      if (idPalabra <= 0) {
        pistasMaximas = ObtenerPistasMaximas(targetWord.Length);
        ActualizarEstadoPistas();
        return;
      }

      try {
        var palabraService = ServiceProxyManager.GetPalabraService();
        var result = palabraService.ObtenerPalabrasPorIdioma(selectedLanguage);
        if (result == null || !result.IsSuccess) {
          pistasMaximas = ObtenerPistasMaximas(targetWord.Length);
          ActualizarEstadoPistas();
          return;
        }

        var palabra = result.Value.FirstOrDefault(p => p.IDPalabra == idPalabra);
        if (palabra != null) {
          this.targetWord = palabra.PalabraTexto.ToUpper();
          this.fullTargetWord = this.targetWord;
        }
      } catch {
        // Ignorar o registrar error
      }

      pistasMaximas = ObtenerPistasMaximas(targetWord.Length);
      ActualizarEstadoPistas();
    }

    private int ObtenerPistasMaximas(int longitud) {
      if (longitud < 5) return 0;
      if (longitud <= 8) return 1;
      return 2;
    }

    private void ActualizarEstadoPistas() {
      if (TextBlockClueStatus == null || ButtonClue == null) {
        return;
      }

      if (pistasMaximas == 0) {
        string noCluesMsg = (string) Application.Current.FindResource("Game_ClueNoClues") ?? "No hay pistas disponibles";
        TextBlockClueStatus.Text = noCluesMsg;
        ButtonClue.IsEnabled = false;
      } else {
        string statusTemplate = (string) Application.Current.FindResource("Game_ClueStatus") ?? "Pistas usadas: {0}/{1}";
        TextBlockClueStatus.Text = string.Format(statusTemplate, pistasUsadas, pistasMaximas);

        if (pistasUsadas >= pistasMaximas) {
          ButtonClue.IsEnabled = false;
          return;
        }

        if (guessedWord == null || string.IsNullOrEmpty(fullTargetWord)) {
          ButtonClue.IsEnabled = false;
          return;
        }

        var letrasRestantes = fullTargetWord.Where((c, idx) => idx < guessedWord.Length && guessedWord[idx] == '_')
                                            .Select(c => char.ToUpper(c))
                                            .Distinct()
                                            .ToList();

        if (letrasRestantes.Count <= 1) {
          ButtonClue.IsEnabled = false;
        } else {
          ButtonClue.IsEnabled = true;
        }
      }
    }

    private void ButtonClue_Click(object sender, RoutedEventArgs e) {
      if (isHost || gameServiceProxy == null) {
        return;
      }

      if (pistasUsadas >= pistasMaximas) {
        string title = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        string msg = (string) Application.Current.FindResource("Game_ClueLimitReached") ?? "¡Límite de pistas alcanzado!";
        MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      if (guessedWord == null || string.IsNullOrEmpty(fullTargetWord)) {
        return;
      }

      var letrasRestantes = fullTargetWord.Where((c, idx) => idx < guessedWord.Length && guessedWord[idx] == '_')
                                          .Select(c => char.ToUpper(c))
                                          .Distinct()
                                          .ToList();

      if (letrasRestantes.Count <= 1) {
        string title = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
        string msg = (string) Application.Current.FindResource("Game_ClueAlmostWon") ?? "No puedes usar pistas si solo queda una letra por adivinar.";
        MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      // Choose a random letter
      var random = new Random();
      char letraPista = letrasRestantes[random.Next(letrasRestantes.Count)];

      var usuario = SessionContext.UsuarioLogueado;
      if (usuario != null) {
        try {
          gameServiceProxy.EscribirLetra(codigoAcceso, usuario.IDJugador, letraPista);
          pistasUsadas++;
          ActualizarEstadoPistas();
          AudioManager.ReproducirEfecto("Hint.mp3");
        } catch (Exception ex) {
          if (!isHost && EsCanalFallido(gameServiceProxy)) {
            ManejarFalloAdivinadorPorInactividad();
            return;
          }

          string errTitle = (string) Application.Current.FindResource("Msg_ErrorTitle") ?? "Error";
          string letterErr = (string) Application.Current.FindResource("Game_ErrorLetter") ?? "No se pudo registrar la letra en el servidor: {0}";
          MessageBox.Show(string.Format(letterErr, ex.Message), errTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
      }
    }

    private void UpdateHangmanImage() {
      int imageNumber = HangmanImageByMistakes[Math.Max(0, Math.Min(MaxMistakes, mistakes))];
      ImageHangman.Source = new BitmapImage(new Uri($"/Assets/Images/Hangedman{imageNumber}.png", UriKind.Relative));
    }

    private void UpdateAttempts() {
      ProgressBarAttempts.Maximum = MaxMistakes;
      ProgressBarAttempts.Value = mistakes;
      TextBlockAttempts.Text = $"{mistakes}/{MaxMistakes}";
    }

    private void ReproducirEfectoLetra(char letra, bool esCorrecta, string palabraRevelada, int vidaRestante) {
      if (letra == '\0' || vidaRestante <= 0 || palabraRevelada == null || !palabraRevelada.Contains("_")) {
        return;
      }

      AudioManager.ReproducirEfecto(esCorrecta ? "Correct.mp3" : "Hit.mp3");
    }

    private void ReproducirEfectoResultado(string ganador) {
      bool ganoJugadorActual = ganador == SessionContext.UsuarioLogueado?.NombreDeUsuario;
      if (ganoJugadorActual) {
        AudioManager.ReproducirEfecto(mistakes == 0 ? "Perfect.mp3" : "Victory.mp3");
      } else {
        AudioManager.ReproducirEfecto("GameOver.mp3");
      }
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
        EnviarAbandonoVoluntario();
        DesconectarJuego();
        string abandonedMsg = (string) Application.Current.FindResource("Game_AbandonedSuccess") ?? "La partida ha sido abandonada.";
        string resultTitle = (string) Application.Current.FindResource("Game_ResultTitle") ?? "Partida Finalizada";
        MessageBox.Show(abandonedMsg, resultTitle, MessageBoxButton.OK, MessageBoxImage.Information);
        NavigationService.Navigate(new GUILobbyView());
      }
    }

    private bool EsCanalFallido(IGameService service) {
      try {
        var channel = service as System.ServiceModel.ICommunicationObject;
        return channel != null && channel.State == System.ServiceModel.CommunicationState.Faulted;
      } catch {
        return false;
      }
    }

    private void ManejarFalloAdivinadorPorInactividad() {
      EnviarAbandonoConCanalNuevo();
      RegistrarAvisoPenalizacionPendiente();
      partidaCerrada = true;
      DesconectarJuego();

      string message = (string) Application.Current.FindResource("Game_InactivityPenalty") ?? "La partida se cerro por inactividad. Se aplico una penalizacion de -3 puntos.";
      string title = (string) Application.Current.FindResource("Game_ResultTitle") ?? "Partida Finalizada";
      MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
      NavigationService.Navigate(new GUILobbyView());
    }

    private void EnviarAbandonoConCanalNuevo() {
      var usuario = SessionContext.UsuarioLogueado;
      if (usuario == null) {
        return;
      }

      try {
        var nuevoCallback = new GameCallbackHandler();
        var nuevoProxy = ServiceProxyManager.GetGameService(nuevoCallback);
        nuevoProxy.AbandonarPartida(codigoAcceso, usuario.IDJugador);

        var channel = nuevoProxy as System.ServiceModel.ICommunicationObject;
        if (channel != null && channel.State == System.ServiceModel.CommunicationState.Opened) {
          channel.Close();
        }
      } catch { }
    }
  }
}
