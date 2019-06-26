using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Exercise.View
{
    public class PaperViewer : UserControl
    {

        public static readonly DependencyProperty PaperProperty = 
            DependencyProperty.Register("Paper", typeof(string), typeof(PaperViewer), 
                new PropertyMetadata((o, e) => (o as PaperViewer).SetPaper(e.NewValue as string)));

        public Uri Paper
        {
            get => GetValue(PaperProperty) as Uri;
            set => SetValue(PaperProperty, value);
        }

        public PathGeometry Overlay { get; set; } = new PathGeometry();
        public Brush OverlayBrush
        {
            get => geometry.Brush;
            set => geometry.Brush = value;
        }
        public Pen OverlayPen
        {
            get => geometry.Pen;
            set => geometry.Pen = value;
        }

        private BitmapImage paper;
        private ImageDrawing image = new ImageDrawing();
        private DrawingGroup group = new DrawingGroup();
        private GeometryDrawing geometry = new GeometryDrawing();
        private DrawingBrush brush = new DrawingBrush();

        private double scale;
        private bool draging;
        private Point start;

        static PaperViewer()
        {
        }

        public PaperViewer()
        {
            group.Children.Add(image);
            geometry.Geometry = Overlay;
            geometry.Pen = new Pen(Brushes.Red, 2);
            group.Children.Add(geometry);
            brush.Drawing = group;
            Background = brush;
            SizeChanged += PaperViewer_SizeChanged;
        }

        private void PaperViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Adjust();
        }

        private void SetPaper(string uri)
        {
            if (uri == null)
            {
                image.ImageSource = paper = null;
            }
            paper = new BitmapImage();
            paper.BeginInit();
            paper.CacheOption = BitmapCacheOption.OnLoad;
            paper.UriSource = new Uri(uri);
            paper.EndInit();
            Paper_Changed(paper, null);
        }

        private void Paper_Changed(object sender, EventArgs e)
        {
            if (paper == sender)
            {
                image.ImageSource = paper;
                image.Rect = new Rect(0, 0, paper.Width, paper.Height);
                Adjust();
            }
        }

        private void Adjust()
        {
            if (this.RenderSize.Width <= 0 || this.RenderSize.Height <= 0)
                return;
            if (paper == null)
                return;
            double s = (RenderSize.Width * paper.Height) / (RenderSize.Height * paper.Width);
            if (s > 1)
            {
                scale = 1 / s / this.RenderSize.Height;
                brush.Viewbox = new Rect(0, 0, 1, 1 / s);
            }
            else
            {
                scale = s / this.RenderSize.Width;
                brush.Viewbox = new Rect(0, 0, s, 1);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (paper == null)
                return;
            CaptureMouse();
            start = e.GetPosition(this);
            draging = true;
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (!draging)
                return;
            ReleaseMouseCapture();
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!draging)
                return;
            Point pt = e.GetPosition(this);
            Vector off = Point.Subtract(start, pt);
            Vector off2 = Vector.Multiply(off, scale);
            if (brush.Viewbox.Left + off2.X < 0)
                off2.X = -brush.Viewbox.Left;
            else if (brush.Viewbox.Right + off2.X > 1)
                off2.X = 1 - brush.Viewbox.Right;
            if (brush.Viewbox.Top + off2.Y < 0)
                off2.Y = -brush.Viewbox.Top;
            else if (brush.Viewbox.Bottom + off2.Y > 1)
                off2.Y = 1 - brush.Viewbox.Bottom;
            Rect rect = Rect.Offset(brush.Viewbox, off2);
            brush.Viewbox = rect;
            //off2 = Vector.Divide(off2, scale);
            //start = Point.Add(start, Vector.Subtract(off, off2));
            start = pt;
            e.Handled = true;
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            draging = false;
        }
    }
}
