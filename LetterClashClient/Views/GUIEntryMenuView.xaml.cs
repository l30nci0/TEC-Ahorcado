using System.Windows;
using System.Windows.Controls;

namespace LetterClashClient.Views {
  public partial class GUIEntryMenuView : Page {
    public GUIEntryMenuView() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window.GetWindow(this).Title = "Menú de Entrada";
    }

    private void ButtonStart_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUILoginView());
    }
  }
}
