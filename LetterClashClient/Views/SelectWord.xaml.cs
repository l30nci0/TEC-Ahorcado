using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LetterClashClient.Views {
  public partial class SelectWord : Page {
    public SelectWord() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window window = Window.GetWindow(this);

      if (window != null) {
        window.Title = "Crea Lobby (Escoger palabra)";
      }

      DataGridWords.ItemsSource = new List<WordItem>
{
        new WordItem { Palabra = "Ahorcado", Descripcion = "Juego donde se adivina una palabra antes de completar el dibujo." },
        new WordItem { Palabra = "Castillo", Descripcion = "Construcción grande protegida por muros y torres." },
        new WordItem { Palabra = "Espada", Descripcion = "Arma larga usada por guerreros o caballeros." },
        new WordItem { Palabra = "Dragón", Descripcion = "Criatura fantástica que puede volar y lanzar fuego." },
        new WordItem { Palabra = "Escudo", Descripcion = "Objeto usado para protegerse en combate." },
        new WordItem { Palabra = "Corona", Descripcion = "Objeto que representa a un rey o reina." },
        new WordItem { Palabra = "Batalla", Descripcion = "Enfrentamiento entre dos o más oponentes." },
        new WordItem { Palabra = "Guerrero", Descripcion = "Persona que participa en combates." },
        new WordItem { Palabra = "Reino", Descripcion = "Territorio gobernado por un rey o reina." },
        new WordItem { Palabra = "Victoria", Descripcion = "Resultado obtenido al ganar una partida." }
      };
    }

    private void ButtonUseWord_Click(object sender, RoutedEventArgs e) {
      Button button = sender as Button;

      if (button != null) {
        WordItem selectedWord = button.DataContext as WordItem;

        if (selectedWord != null) {
          NavigationService.Navigate(new CreateRoom(selectedWord.Palabra));
        }
      }
    }

    private void ButtonBack_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new CreateRoom());
    }
  }

  public class WordItem {
    public string Palabra { get; set; }
    public string Descripcion { get; set; }
  }
}
