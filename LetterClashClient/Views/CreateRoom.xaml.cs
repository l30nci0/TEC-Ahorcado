using System.Windows;
using System.Windows.Controls;

namespace LetterClashClient.Views
{
    public partial class CreateRoom : Page
    {
        private string selectedWord;

        public CreateRoom()
        {
            InitializeComponent();
            selectedWord = "";
        }

        public CreateRoom(string selectedWord)
        {
            InitializeComponent();
            this.selectedWord = selectedWord;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);

            if (window != null)
            {
                window.Title = "Menu Principal (Crear Sala)";
            }

            TextBlockUsername.Text = "\"jugador1\"";
            TextBlockAge.Text = "\"20\"";
            TextBlockSelectedWord.Text = selectedWord;
        }

        private void ButtonSelectWord_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SelectWord());
        }

        private void ButtonCreateRoom_Click(object sender, RoutedEventArgs e)
        {
            bool hasLanguage = ComboBoxLanguage.SelectedIndex > 0;
            bool hasGameType = ComboBoxGameType.SelectedIndex > 0;
            bool hasWord = !string.IsNullOrWhiteSpace(TextBlockSelectedWord.Text);

            if (!hasLanguage || !hasGameType || !hasWord)
            {
                MessageBox.Show("Seleccione idioma, palabra y tipo de partida.",
                                "TecnoHorcado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            NavigationService.Navigate(new GameHost(TextBlockSelectedWord.Text));
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MainMenu());
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