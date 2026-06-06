using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;


namespace LetterClashClient.Views
{
    public partial class MainMenu : Page
    {
        private int currentHangmanState = 5;

        public MainMenu()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);

            if (window != null)
            {
                window.Title = "Menu Principal";
            }

            TextBlockUsername.Text = "\"jugador1\"";
            TextBlockAge.Text = "\"20\"";
            UpdateHangmanImage();
        }

        private void ButtonCreateRoom_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CreateRoom());
        }

        private void ButtonPublicLobby_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PublicLobby());
        }

        private void ButtonPrivateLobby_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PrivateLobby());
        }

        private void ButtonRemovePart_Click(object sender, RoutedEventArgs e)
        {
            if (currentHangmanState < 5)
            {
                currentHangmanState++;
                UpdateHangmanImage();
            }
        }

        private void ButtonAddPart_Click(object sender, RoutedEventArgs e)
        {
            if (currentHangmanState > 1)
            {
                currentHangmanState--;
                UpdateHangmanImage();
            }
        }

        private void UpdateHangmanImage()
        {
            ImageHangman.Source = new BitmapImage(new Uri($"/Assets/Images/Hangedman{currentHangmanState}.jpg", UriKind.Relative));
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