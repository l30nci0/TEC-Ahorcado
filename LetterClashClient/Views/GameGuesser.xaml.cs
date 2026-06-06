using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LetterClashClient.Views
{
    public partial class GameGuesser : Page
    {
        private string opponentName;
        private string selectedLanguage;
        private string targetWord;
        private char[] guessedWord;
        private int mistakes;

        public GameGuesser()
        {
            InitializeComponent();
            opponentName = "Usuario 1";
            selectedLanguage = "Español";
            targetWord = "SOFTWARE";
            guessedWord = new char[targetWord.Length];
            mistakes = 0;
        }

        public GameGuesser(string opponentName, string selectedLanguage)
        {
            InitializeComponent();
            this.opponentName = opponentName;
            this.selectedLanguage = selectedLanguage;
            targetWord = "SOFTWARE";
            guessedWord = new char[targetWord.Length];
            mistakes = 0;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);

            if (window != null)
            {
                window.Title = "Partida (Adivinador)";
            }

            for (int index = 0; index < guessedWord.Length; index++)
            {
                guessedWord[index] = '_';
            }

            TextBlockOpponent.Text = opponentName;
            TextBlockChatTitle.Text = $"Chat con {opponentName}";
            TextBoxChatMessages.Text = $"{opponentName}: Hola\n{opponentName}: Juegas??";
            UpdateHiddenWord();
            UpdateAttempts();
        }

        private void ButtonLetter_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            string content = button.Content.ToString();

            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            char selectedLetter = content[0];
            bool letterFound = false;

            for (int index = 0; index < targetWord.Length; index++)
            {
                if (targetWord[index] == selectedLetter)
                {
                    guessedWord[index] = selectedLetter;
                    letterFound = true;
                }
            }

            button.IsEnabled = false;

            if (!letterFound)
            {
                mistakes++;

                if (mistakes > 5)
                {
                    mistakes = 5;
                }

                UpdateAttempts();
            }

            UpdateHiddenWord();

            if (new string(guessedWord) == targetWord)
            {
                MessageBox.Show("Ganaste la partida.",
                                "TecnoHorcado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                NavigationService.Navigate(new MainMenu());
            }

            if (mistakes == 5)
            {
                MessageBox.Show("Perdiste la partida.",
                                "TecnoHorcado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                NavigationService.Navigate(new MainMenu());
            }
        }

        private void UpdateHiddenWord()
        {
            TextBlockHiddenWord.Text = string.Join(" ", guessedWord);
        }

        private void UpdateAttempts()
        {
            ProgressBarAttempts.Value = mistakes;
            TextBlockAttempts.Text = $"{mistakes}/5";

            int imageNumber = 5 - mistakes;

            if (imageNumber < 1)
            {
                imageNumber = 1;
            }

            ImageHangman.Source = new BitmapImage(new Uri($"/Assets/Images/Hangedman{imageNumber}.jpg", UriKind.Relative));
        }

        private void ButtonSendMessage_Click(object sender, RoutedEventArgs e)
        {
            string message = TextBoxChatInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            TextBoxChatMessages.Text += $"\nTú: {message}";
            TextBoxChatInput.Text = "";
            TextBoxChatMessages.ScrollToEnd();
        }

        private void ButtonAbandon_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("¿Seguro que deseas abandonar la partida?",
                                                      "TecnoHorcado",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                NavigationService.Navigate(new MainMenu());
            }
        }
    }
}