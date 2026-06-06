using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LetterClashClient.Views
{
    public partial class History : Page
    {
        public History()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);

            if (window != null)
            {
                window.Title = "Historial";
            }

            TextBlockUsername.Text = "\"jugador1\"";
            TextBlockAge.Text = "\"20\"";

            List<BattleHistoryItem> battles = new List<BattleHistoryItem>
      {
        new BattleHistoryItem { Rival = "Usuario 1", Fecha = "27/03/2026", Palabra = "Software", Resultado = "Victoria", Puntuacion = "+10", Rol = "Adivino", Idioma = "Español", Tipo = "Privado", Progreso = 5 },
        new BattleHistoryItem { Rival = "Usuario 2", Fecha = "27/03/2026", Palabra = "Servidor", Resultado = "Derrota", Puntuacion = "-5", Rol = "Verdugo", Idioma = "Español", Tipo = "Publico", Progreso = 3 },
        new BattleHistoryItem { Rival = "Usuario 3", Fecha = "27/03/2026", Palabra = "Cliente", Resultado = "Desconectada", Puntuacion = "-3", Rol = "Adivino", Idioma = "Ingles", Tipo = "Privado", Progreso = 2 },
        new BattleHistoryItem { Rival = "Usuario 4", Fecha = "28/03/2026", Palabra = "Ahorcado", Resultado = "Victoria", Puntuacion = "+10", Rol = "Verdugo", Idioma = "Español", Tipo = "Publico", Progreso = 5 },
        new BattleHistoryItem { Rival = "Usuario 5", Fecha = "28/03/2026", Palabra = "Codigo", Resultado = "Victoria", Puntuacion = "+10", Rol = "Adivino", Idioma = "Español", Tipo = "Privado", Progreso = 4 },
        new BattleHistoryItem { Rival = "Usuario 6", Fecha = "29/03/2026", Palabra = "Variable", Resultado = "Derrota", Puntuacion = "-5", Rol = "Verdugo", Idioma = "Ingles", Tipo = "Publico", Progreso = 3 },
        new BattleHistoryItem { Rival = "Usuario 7", Fecha = "29/03/2026", Palabra = "Interfaz", Resultado = "Victoria", Puntuacion = "+10", Rol = "Adivino", Idioma = "Español", Tipo = "Privado", Progreso = 5 },
        new BattleHistoryItem { Rival = "Usuario 8", Fecha = "30/03/2026", Palabra = "Partida", Resultado = "Desconectada", Puntuacion = "-3", Rol = "Verdugo", Idioma = "Español", Tipo = "Publico", Progreso = 1 },
        new BattleHistoryItem { Rival = "Usuario 9", Fecha = "30/03/2026", Palabra = "Jugador", Resultado = "Victoria", Puntuacion = "+10", Rol = "Adivino", Idioma = "Ingles", Tipo = "Privado", Progreso = 5 },
        new BattleHistoryItem { Rival = "Usuario 10", Fecha = "31/03/2026", Palabra = "Pantalla", Resultado = "Derrota", Puntuacion = "-5", Rol = "Verdugo", Idioma = "Español", Tipo = "Publico", Progreso = 2 }
      };

            DataGridHistory.ItemsSource = battles;
            LoadStatistics(battles);
        }

        private void LoadStatistics(List<BattleHistoryItem> battles)
        {
            int total = battles.Count;
            int wins = battles.Count(battle => battle.Resultado == "Victoria");
            int losses = battles.Count(battle => battle.Resultado == "Derrota");
            int disconnected = battles.Count(battle => battle.Resultado == "Desconectada");

            TextBlockTotalWins.Text = $"Total Ganadas = {wins}";
            TextBlockPercentWins.Text = $"%{GetPercentage(wins, total)}";
            TextBlockTotalLosses.Text = $"Total Perdidas = {losses}";
            TextBlockPercentLosses.Text = $"%{GetPercentage(losses, total)}";
            TextBlockTotalDisconnected.Text = $"Total Desconectadas = {disconnected}";
            TextBlockPercentDisconnected.Text = $"%{GetPercentage(disconnected, total)}";
        }

        private int GetPercentage(int amount, int total)
        {
            if (total == 0)
            {
                return 0;
            }

            return amount * 100 / total;
        }

        private void DataGridHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BattleHistoryItem selectedBattle = DataGridHistory.SelectedItem as BattleHistoryItem;

            if (selectedBattle != null)
            {
                NavigationService.Navigate(new BattleHistoryDetail(selectedBattle));
            }
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

    public class BattleHistoryItem
    {
        public string Rival { get; set; }
        public string Fecha { get; set; }
        public string Palabra { get; set; }
        public string Resultado { get; set; }
        public string Puntuacion { get; set; }
        public string Rol { get; set; }
        public string Idioma { get; set; }
        public string Tipo { get; set; }
        public int Progreso { get; set; }
    }
}