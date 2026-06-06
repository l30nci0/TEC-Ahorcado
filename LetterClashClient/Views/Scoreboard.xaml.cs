using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LetterClashClient.Views
{
    public partial class Scoreboard : Page
    {
        public Scoreboard()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);

            if (window != null)
            {
                window.Title = "Marcadores";
            }

            DataGridScoreboard.ItemsSource = new List<ScoreboardItem>
      {
        new ScoreboardItem { Nombre = "Usuario 1", PartidasGanadas = 324, Porcentaje = "%90", Puntuacion = 59033, Puesto = "1*" },
        new ScoreboardItem { Nombre = "Usuario 2", PartidasGanadas = 246, Porcentaje = "%89", Puntuacion = 42213, Puesto = "2*" },
        new ScoreboardItem { Nombre = "Usuario 3", PartidasGanadas = 150, Porcentaje = "%82", Puntuacion = 23244, Puesto = "3*" },
        new ScoreboardItem { Nombre = "Usuario 4", PartidasGanadas = 94, Porcentaje = "%74", Puntuacion = 121212, Puesto = "4*" },
        new ScoreboardItem { Nombre = "Usuario 5", PartidasGanadas = 12, Porcentaje = "%65", Puntuacion = 5666, Puesto = "5*" },
        new ScoreboardItem { Nombre = "Usuario 6", PartidasGanadas = 10, Porcentaje = "%62", Puntuacion = 4300, Puesto = "6*" },
        new ScoreboardItem { Nombre = "Usuario 7", PartidasGanadas = 8, Porcentaje = "%55", Puntuacion = 3900, Puesto = "7*" },
        new ScoreboardItem { Nombre = "Usuario 8", PartidasGanadas = 7, Porcentaje = "%50", Puntuacion = 3100, Puesto = "8*" },
        new ScoreboardItem { Nombre = "Usuario 9", PartidasGanadas = 6, Porcentaje = "%48", Puntuacion = 2800, Puesto = "9*" },
        new ScoreboardItem { Nombre = "Usuario 10", PartidasGanadas = 5, Porcentaje = "%45", Puntuacion = 2500, Puesto = "10*" }
      };
        }
        private void DataGridScoreboard_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ScoreboardItem selectedUser = DataGridScoreboard.SelectedItem as ScoreboardItem;

            if (selectedUser != null)
            {
                NavigationService.Navigate(new DifferentUser(selectedUser.Nombre));
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

    public class ScoreboardItem
    {
        public string Nombre { get; set; }
        public int PartidasGanadas { get; set; }
        public string Porcentaje { get; set; }
        public int Puntuacion { get; set; }
        public string Puesto { get; set; }
    }
}