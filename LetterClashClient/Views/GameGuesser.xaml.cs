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
  public partial class GameGuesser : Page {
    private string opponentName;
    private string selectedLanguage;
    private string targetWord;
    private char[] guessedWord;
    private int mistakes;
    private readonly string codigoAcceso;
    private IGameService gameServiceProxy;
    private GameCallbackHandler callbackHandler;

    public GameGuesser() : this("Usuario 1", "Español", "000000") { }

    public GameGuesser(string opponentName, string selectedLanguage, string codigoAcceso) {
      InitializeComponent();
      this.opponentName = opponentName;
      this.selectedLanguage = selectedLanguage;
      this.codigoAcceso = codigoAcceso;
      targetWord = "SOFTWARE";
      guessedWord = new char[targetWord.Length];
      mistakes = 0;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = "Partida (Adivinador)";
      }

      for (int index = 0; index < guessedWord.Length; index++) {
        guessedWord[index] = '_';
      }

      TextBlockOpponent.Text = opponentName;
      TextBlockChatTitle.Text = $"Chat con {opponentName}";
      TextBoxChatMessages.Text = "";
      UpdateHiddenWord();
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
        NavigationService.Navigate(new MainMenu());
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
        NavigationService.Navigate(new MainMenu());
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
        guessedWord = palabraRevelada.ToCharArray();
        mistakes = 5 - vidaRestante;

        UpdateHiddenWord();
        UpdateAttempts();

        if (letra != '\0') {
          DeshabilitarBotonLetra(letra);
        }
      });
    }

    private void OnPartidaFinalizada(string ganador, int puntuacionObtenida) {
      Dispatcher.Invoke(() => {
        DesconectarJuego();
        string mensajeResult = (ganador == SessionContext.UsuarioLogueado?.NombreDeUsuario) 
            ? $"¡Felicidades! Ganaste la partida y obtuviste {puntuacionObtenida} puntos." 
            : $"El juego ha concluido. Ganador: {ganador}.";

        MessageBox.Show(mensajeResult, "Partida Finalizada", MessageBoxButton.OK, MessageBoxImage.Information);
        NavigationService.Navigate(new MainMenu());
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
        MessageBox.Show($"El anfitrión ({oponenteNombre}) ha abandonado la partida. ¡Ganaste por abandono (+50 puntos)!", 
                        "Partida Finalizada", MessageBoxButton.OK, MessageBoxImage.Information);
        NavigationService.Navigate(new MainMenu());
      });
    }

    private void OnErrorOcurrido(ServiceFault fault) {
      Dispatcher.Invoke(() => {
        DesconectarJuego();
        MessageBox.Show($"Ocurrió un error en el servidor: {fault.Mensaje}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        NavigationService.Navigate(new MainMenu());
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
      BuscarYDeshabilitarBoton(GridLeftPanel, letraStr);
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

    private void UpdateAttempts() {
      ProgressBarAttempts.Value = mistakes;
      TextBlockAttempts.Text = $"{mistakes}/5";

      int imageNumber = 5 - mistakes;

      if (imageNumber < 1) {
        imageNumber = 1;
      }

      ImageHangman.Source = new BitmapImage(new Uri($"/Assets/Images/Hangedman{imageNumber}.jpg", UriKind.Relative));
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
        NavigationService.Navigate(new MainMenu());
      }
    }
  }
}
