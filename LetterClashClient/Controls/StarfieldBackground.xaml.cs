using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LetterClashClient.Controls {
  public partial class StarfieldBackground : UserControl {

    private struct Star {
      public double X;
      public double Y;
      public double Speed;
      public double Size;
      public double Opacity;
      public double OpacityDelta;
      public Rectangle Rect;
    }

    private readonly List<Star> _stars = new List<Star>();
    private readonly DispatcherTimer _timer;
    private readonly Random _rng = new Random();
    private const int StarCount = 120;

    public StarfieldBackground() {
      InitializeComponent();
      _timer = new DispatcherTimer {
        Interval = TimeSpan.FromMilliseconds(33) // ~30 fps
      };
      _timer.Tick += OnTick;
      Loaded += OnLoaded;
      Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) {
      SpawnStars();
      _timer.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) {
      _timer.Stop();
    }

    private void StarCanvas_SizeChanged(object sender, SizeChangedEventArgs e) {
      // Re-spawn stars when size changes so they fill the new area
      if (StarCanvas.ActualWidth > 0 && StarCanvas.ActualHeight > 0) {
        SpawnStars();
      }
    }

    private void SpawnStars() {
      StarCanvas.Children.Clear();
      _stars.Clear();

      double w = StarCanvas.ActualWidth;
      double h = StarCanvas.ActualHeight;

      if (w <= 0 || h <= 0) return;

      for (int i = 0; i < StarCount; i++) {
        double size = _rng.NextDouble() < 0.25 ? 3 : 2; // 25% big stars (3px), 75% normal (2px)
        double opacity = 0.15 + _rng.NextDouble() * 0.75;
        double opacityDelta = (0.003 + _rng.NextDouble() * 0.007)
                              * (_rng.NextDouble() < 0.5 ? 1 : -1);
        double speed = 0.08 + _rng.NextDouble() * 0.22;

        var rect = new Rectangle {
          Width = size,
          Height = size,
          Fill = Brushes.White,
          Opacity = opacity,
          SnapsToDevicePixels = true
        };

        double x = _rng.NextDouble() * w;
        double y = _rng.NextDouble() * h;

        Canvas.SetLeft(rect, x);
        Canvas.SetTop(rect, y);
        StarCanvas.Children.Add(rect);

        _stars.Add(new Star {
          X = x,
          Y = y,
          Speed = speed,
          Size = size,
          Opacity = opacity,
          OpacityDelta = opacityDelta,
          Rect = rect
        });
      }
    }

    private void OnTick(object sender, EventArgs e) {
      double h = StarCanvas.ActualHeight;
      double w = StarCanvas.ActualWidth;

      if (w <= 0 || h <= 0) return;

      for (int i = 0; i < _stars.Count; i++) {
        var s = _stars[i];

        // Drift slowly downward (falling pixel stars)
        s.Y += s.Speed;

        // Twinkle: oscillate opacity
        s.Opacity += s.OpacityDelta;
        if (s.Opacity >= 0.9) {
          s.Opacity = 0.9;
          s.OpacityDelta = -Math.Abs(s.OpacityDelta);
        } else if (s.Opacity <= 0.08) {
          s.Opacity = 0.08;
          s.OpacityDelta = Math.Abs(s.OpacityDelta);
        }

        // Wrap star back to top when it falls off screen
        if (s.Y > h + s.Size) {
          s.Y = -s.Size;
          s.X = _rng.NextDouble() * w;
        }

        s.Rect.Opacity = s.Opacity;
        Canvas.SetLeft(s.Rect, s.X);
        Canvas.SetTop(s.Rect, s.Y);

        _stars[i] = s;
      }
    }
  }
}
