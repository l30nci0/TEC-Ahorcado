using System.Windows;
using LetterClashClient.Views;

namespace LetterClashClient
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new EntryMenu());
        }
    }
}