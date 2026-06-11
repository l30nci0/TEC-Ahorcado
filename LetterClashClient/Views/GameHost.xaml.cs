using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using LetterClashClient.Models;
using LetterClashClient.Services;
using LetterClashServer.Contracts;
using LetterClashServer.Domain.Models;

namespace LetterClashClient.Views {
  public partial class GameHost : Page {
    private string opponentName;
    private string selectedWord;
    private string codigoAcceso;
    private int currentHangmanState;
    private IGameService gameServiceProxy;
    private GameCallbackHandler callbackHandler;

    public GameHost() : this("SOFTWARE", "------") { }

    public GameHost(string selectedWord, string codigoAcceso) {
      InitializeComponent();
      opponentName = "Usuario 1";
      this.selectedWord = string.IsNullOrWhiteSpace(selectedWord) ? "SOFTWARE" : selectedWord.ToUpper();
      this.codigoAcceso = string.IsNullOrWhiteSpace(codigoAcceso) ? "------" : codigoAcceso;
      currentHangmanState = 5;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = "Partida (Anfitrión)";
      }

      TextBlockOpponent.Text = opponentName;
      TextBlockWord.Text = string.Join(" ", selectedWord.ToCharArray());
      TextBlockAccessCode.Text = codigoAcceso;
      TextBlockChatTitle.Text = $"Chat con {opponentName}";
      TextBoxChatMessages.Text = "";
      TextBlockSelectedLetter.Text = "-";
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
        if (letra != '\0') {
          TextBlockSelectedLetter.Text = letra.ToString().ToUpper();
        } else {
          TextBlockSelectedLetter.Text = "-";
        }

        TextBlockWord.Text = string.Join(" ", palabraRevelada.ToCharArray());

        int mistakes = 5 - vidaRestante;
        currentHangmanState = 5 - mistakes;
        if (currentHangmanState < 1) {
          currentHangmanState = 1;
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
        MessageBox.Show($"El adivinador ({oponenteNombre}) ha abandonado la partida. ¡Ganaste por abandono (+50 puntos)!", 
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
