using System.Windows;
using System.Windows.Controls;

namespace LetterClashClient.Views
{
    public partial class BattleHistoryDetail : Page
    {
        private BattleHistoryItem selectedBattle;

        public BattleHistoryDetail(BattleHistoryItem battle)
        {
            InitializeComponent();
            selectedBattle = battle;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);

            if (window != null)
            {
                window.Title = "Partida (Historial)";
            }

            TextBlockRole.Text = $"Tu Rol fue: {selectedBattle.Rol}";
            TextBlockRival.Text = $"Adversario: {selectedBattle.Rival}";
            TextBlockLanguage.Text = $"Idioma: {selectedBattle.Idioma}";
            TextBlockWord.Text = $"Palabra: {selectedBattle.Palabra}";
            TextBlockType.Text = $"Tipo: {selectedBattle.Tipo}";
            TextBlockDate.Text = $"Fecha de juego: {selectedBattle.Fecha}";
            TextBlockBattleStatus.Text = selectedBattle.Resultado;
            TextBlockScore.Text = selectedBattle.Puntuacion;
            ProgressBarBattle.Value = selectedBattle.Progreso;
            TextBlockProgress.Text = $"{selectedBattle.Progreso}/5";
        }

        private void ButtonViewProfile_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DifferentUser(selectedBattle.Rival));
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new History());
        }

        private void ButtonMainMenu_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainMenu());
        }

        private void ButtonProfile_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Profile());
        }

        private void ButtonHistory_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new History());
        }

        private void ButtonScoreboard_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Scoreboard());
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Settings());
        }
    }
}