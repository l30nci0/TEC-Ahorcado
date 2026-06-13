using System.Windows;

using LetterClashClient.Views;

namespace LetterClashClient {
  public partial class MainWindow : Window {
    public MainWindow() {
      InitializeComponent();
      MainFrame.Navigate(new GUIEntryMenuView());
    }

    private void GridTitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      if (e.ChangedButton == System.Windows.Input.MouseButton.Left) {
        this.DragMove();
      }
    }

    private void ButtonMinimize_Click(object sender, RoutedEventArgs e) {
      this.WindowState = WindowState.Minimized;
    }

    private void ButtonClose_Click(object sender, RoutedEventArgs e) {
      this.Close();
    }

    private void ButtonMaximize_Click(object sender, RoutedEventArgs e) {
      if (this.WindowState == WindowState.Maximized) {
        this.WindowState = WindowState.Normal;
      } else {
        this.WindowState = WindowState.Maximized;
      }
    }
  }
}
