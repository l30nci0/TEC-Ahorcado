using System.Windows;
using System.Windows.Controls;

namespace LetterClashClient.Views {
  public partial class GUIWelcomeView : Page {
    public GUIWelcomeView() {
      InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e) {
      Window.GetWindow(this).Title = "Menú de Entrada";
    }

    private void ButtonLogin_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUILoginView());
    }

    private void ButtonRegister_Click(object sender, RoutedEventArgs e) {
      NavigationService.Navigate(new GUIRegisterView());
    }
  }
}
