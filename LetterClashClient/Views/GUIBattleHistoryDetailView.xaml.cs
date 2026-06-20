using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using LetterClashClient.Models;
using LetterClashClient.Services;

namespace LetterClashClient.Views {
  public partial class GUIBattleHistoryDetailView : Page {
    private BattleHistoryItem selectedBattle;

    public GUIBattleHistoryDetailView(BattleHistoryItem battle) {
      InitializeComponent();
      selectedBattle = battle;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = (string) Application.Current.FindResource("HistoryDetail_WindowTitle") ?? "Partida (Historial)";
      }

      string roleText = (string) Application.Current.FindResource("HistoryDetail_RoleText") ?? "Tu Rol fue: {0}";
      string rivalText = (string) Application.Current.FindResource("HistoryDetail_RivalText") ?? "Adversario: {0}";
      string langText = (string) Application.Current.FindResource("HistoryDetail_LangText") ?? "Idioma: {0}";
      string wordText = (string) Application.Current.FindResource("HistoryDetail_WordText") ?? "Palabra: {0}";
      string typeText = (string) Application.Current.FindResource("HistoryDetail_TypeText") ?? "Tipo: {0}";
      string dateText = (string) Application.Current.FindResource("HistoryDetail_DateText") ?? "Fecha de juego: {0}";

      TextBlockRole.Text = string.Format(roleText, selectedBattle.Rol);
      TextBlockRival.Text = string.Format(rivalText, selectedBattle.Rival);
      TextBlockLanguage.Text = string.Format(langText, selectedBattle.Idioma);
      TextBlockWord.Text = string.Format(wordText, selectedBattle.Palabra);
      TextBlockType.Text = string.Format(typeText, selectedBattle.Tipo);
      TextBlockDate.Text = string.Format(dateText, selectedBattle.Fecha);
      TextBlockBattleStatus.Text = selectedBattle.Resultado;
      TextBlockScore.Text = selectedBattle.Puntuacion;
      ProgressBarBattle.Value = selectedBattle.Progreso;
      TextBlockProgress.Text = $"{selectedBattle.Progreso}/5";

      // Cargar avatar local (o default si no tiene)
      var usuario = SessionContext.UsuarioLogueado;
      if (usuario != null) {
        AvatarHelper.AsignarAImageControl(ImageUserAvatar, usuario.Avatar);
      }
    }

    private void ButtonViewProfile_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIDifferentUserView(selectedBattle.Rival));
    }

    private void ButtonBack_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIHistoryView());
    }

    private void ButtonMainMenu_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIMainMenuView());
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
