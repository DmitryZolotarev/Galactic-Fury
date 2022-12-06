using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System;

namespace WpfApp1 {
    public partial class Window1 : Window {      
        BitmapImage back_image;
        public Window1() {
            InvalidateVisual();            
            Background = Brushes.Transparent;
            back_image = new BitmapImage
            (new Uri("Sprites//9.png", UriKind.Relative));
            InitializeComponent();         
        }
        protected override void OnRender(DrawingContext draw) => 
        draw.DrawImage(back_image, new Rect(0, 0, RenderSize.Width, RenderSize.Height));
    }
}
