using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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
      public Shape Rect;
    }

    private readonly List<Star> _stars = new List<Star>();
    private readonly DispatcherTimer _timer;
    private static readonly RandomNumberGenerator secureRandom = RandomNumberGenerator.Create();
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
        double size = ObtenerDoubleSeguro() < 0.15 ? ObtenerEnteroSeguro(8, 13) : ObtenerEnteroSeguro(2, 4); // 15% shine stars (8-12px), 85% normal (2-3px)
        double opacity = 0.15 + ObtenerDoubleSeguro() * 0.75;
        double opacityDelta = (0.003 + ObtenerDoubleSeguro() * 0.007)
                              * (ObtenerDoubleSeguro() < 0.5 ? 1 : -1);
        double speed = 0.08 + ObtenerDoubleSeguro() * 0.22;

        Brush fillBrush;
        double p = ObtenerDoubleSeguro();
        if (p < 0.40) {
          fillBrush = Brushes.White;
        } else if (p < 0.60) {
          fillBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4DEEEA")); // Cyan
        } else if (p < 0.80) {
          fillBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F649A7")); // Hot Pink
        } else if (p < 0.90) {
          fillBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5DADE2")); // Soft Blue
        } else {
          fillBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5B041")); // Soft Yellow/Orange
        }
        if (fillBrush.CanFreeze) fillBrush.Freeze();

        Shape starShape;
        if (size >= 8) {
          // Sparkle star (lens flare cross)
          var path = new Path {
            Width = size,
            Height = size,
            Fill = fillBrush,
            Data = Geometry.Parse($"M {size/2},0 L {size*0.58},{size*0.42} L {size},{size/2} L {size*0.58},{size*0.58} L {size/2},{size} L {size*0.42},{size*0.58} L 0,{size/2} L {size*0.42},{size*0.42} Z"),
            Opacity = opacity,
            SnapsToDevicePixels = true
          };
          starShape = path;
        } else {
          // Regular dot star
          var rect = new Rectangle {
            Width = size,
            Height = size,
            Fill = fillBrush,
            Opacity = opacity,
            SnapsToDevicePixels = true
          };
          starShape = rect;
        }

        double x = ObtenerDoubleSeguro() * w;
        double y = ObtenerDoubleSeguro() * h;

        Canvas.SetLeft(starShape, x);
        Canvas.SetTop(starShape, y);
        StarCanvas.Children.Add(starShape);

        _stars.Add(new Star {
          X = x,
          Y = y,
          Speed = speed,
          Size = size,
          Opacity = opacity,
          OpacityDelta = opacityDelta,
          Rect = starShape
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
          s.X = ObtenerDoubleSeguro() * w;
        }

        s.Rect.Opacity = s.Opacity;
        Canvas.SetLeft(s.Rect, s.X);
        Canvas.SetTop(s.Rect, s.Y);

        _stars[i] = s;
      }
    }

    private static int ObtenerEnteroSeguro(int minimoInclusivo, int maximoExclusivo) {
      if (maximoExclusivo <= minimoInclusivo) {
        throw new ArgumentOutOfRangeException(nameof(maximoExclusivo));
      }

      return minimoInclusivo + ObtenerEnteroSeguro(maximoExclusivo - minimoInclusivo);
    }

    private static int ObtenerEnteroSeguro(int maximoExclusivo) {
      if (maximoExclusivo <= 0) {
        throw new ArgumentOutOfRangeException(nameof(maximoExclusivo));
      }

      byte[] bytes = new byte[4];
      uint limite = uint.MaxValue - (uint.MaxValue % (uint) maximoExclusivo);
      uint valor;

      do {
        secureRandom.GetBytes(bytes);
        valor = BitConverter.ToUInt32(bytes, 0);
      } while (valor >= limite);

      return (int) (valor % (uint) maximoExclusivo);
    }

    private static double ObtenerDoubleSeguro() {
      byte[] bytes = new byte[8];
      secureRandom.GetBytes(bytes);
      ulong valor = BitConverter.ToUInt64(bytes, 0) >> 11;
      return valor / (double) (1UL << 53);
    }
  }
}
