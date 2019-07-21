using Base.Misc;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Exercise.View
{
    public class PaperViewer : UserControl
    {

        private static readonly Logger Log = Logger.GetLogger<PaperViewer>();

        public static readonly DependencyProperty PaperProperty =
            DependencyProperty.Register("Paper", typeof(string), typeof(PaperViewer),
                new PropertyMetadata((o, e) => (o as PaperViewer).SetPaper(e.NewValue as string)));

        public static readonly DependencyProperty OverlayProperty =
            DependencyProperty.Register("Overlay", typeof(Geometry), typeof(PaperViewer),
                new PropertyMetadata((o, e) => (o as PaperViewer).SetOverlay(e.NewValue as Geometry)));

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), typeof(PaperViewer),
                new PropertyMetadata(1.0, (o, e) => (o as PaperViewer).SetScale((double)e.NewValue)));

        public static readonly DependencyProperty ScaleModeProperty =
            DependencyProperty.Register("ScaleMode", typeof(ScaleModes), typeof(PaperViewer),
                new PropertyMetadata(ScaleModes.Manual, (o, e) => (o as PaperViewer).SetScaleMode((ScaleModes)e.NewValue)));

        public enum ScaleModes
        {
            Manual, 
            Full,
            Ratio, 
            Clip
        }

        public Uri Paper
        {
            get => GetValue(PaperProperty) as Uri;
            set => SetValue(PaperProperty, value);
        }

        public Geometry Overlay
        {
            get => GetValue(OverlayProperty) as Geometry;
            set => SetValue(OverlayProperty, value);
        }

        public ScaleModes ScaleMode
        {
            get => (ScaleModes) GetValue(ScaleModeProperty);
            set => SetValue(ScaleModeProperty, value);
        }

         public double Scale
        {
            get => (double) GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public Brush OverlayBrush
        {
            get => geometry.Brush;
            set => geometry.Brush = value;
        }

        public Brush OverlayPenBrush
        {
            get => geometry.Pen.Brush;
            set => geometry.Pen.Brush = value;
        }

        private BitmapImage paper;
        private ImageDrawing image = new ImageDrawing();
        private DrawingGroup group = new DrawingGroup();
        private GeometryDrawing geometry = new GeometryDrawing();
        private DrawingBrush brush = new DrawingBrush();

        private double scaleX;
        private double scaleY;
        private bool draging;
        private Point start;

        static PaperViewer()
        {
        }

        public PaperViewer()
        {
            group.Children.Add(image);
            geometry.Pen = new Pen(new SolidColorBrush(Color.FromRgb(253, 113, 42)), 1);
            geometry.Brush = new SolidColorBrush(Color.FromArgb(12, 253, 113, 42));
            group.Children.Add(geometry);
            brush.Drawing = group;
            Background = brush;
            SizeChanged += PaperViewer_SizeChanged;
            IsVisibleChanged += PaperViewer_IsVisibleChanged;
        }

        private void PaperViewer_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Adjust();
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
                return;
            }
            try
            {
                paper = new BitmapImage();
                paper.BeginInit();
                paper.CacheOption = BitmapCacheOption.OnLoad;
                paper.UriSource = new Uri(uri);
                paper.EndInit();
                image.ImageSource = paper;
                image.Rect = new Rect(0, 0, paper.Width, paper.Height);
                AdjustMode();
                Adjust();
            }
            catch (Exception e)
            {
                Log.w(e);
            }
        }

        private void SetOverlay(Geometry g)
        {
            geometry.Geometry = g;
        }

        private void SetScaleMode(ScaleModes newValue)
        {
            AdjustMode();
        }

        private void SetScale(double v)
        {
            Adjust();
        }

        private void AdjustMode()
        {
            if (RenderSize.Width <= 0 || RenderSize.Height <= 0)
                return;
            if (paper == null)
                return;
            double s = (RenderSize.Width * paper.Height) / (RenderSize.Height * paper.Width);
            switch (ScaleMode)
            {
                case ScaleModes.Manual:
                    break;
                case ScaleModes.Full:
                    break;
                case ScaleModes.Ratio:
                    Scale = s > 1 ? 1 / s : s;
                    break;
                case ScaleModes.Clip:
                    Scale = 1;
                    break;
            }
        }

        private void Adjust()
        {
            if (RenderSize.Width <= 0 || RenderSize.Height <= 0)
                return;
            if (paper == null)
                return;
            double s = (RenderSize.Width * paper.Height) / (RenderSize.Height * paper.Width);
            Rect rect;
            if (s > 1)
            {
                scaleX = 1 / Scale;
                scaleY = 1 / s / Scale;
                rect = new Rect(0, 0, scaleX, scaleY);
            }
            else
            {
                scaleX = s /  Scale;
                scaleY = 1 / Scale;
                rect = new Rect(0, 0, s / Scale, 1 / Scale);
            }
            if (rect.Right > 1)
                rect.Offset((1 - rect.Right) / 2, 0);
            if (rect.Bottom > 1)
                rect.Offset(0, (1 - rect.Bottom) / 2);
            brush.Viewbox = rect;
            scaleX /= RenderSize.Width;
            scaleY /= RenderSize.Height;
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
            Vector off2 = new Vector(off.X * scaleX, off.Y * scaleY);
            if (brush.Viewbox.Left + off2.X < 0)
                off2.X = brush.Viewbox.Left  < 0 ? 0 : - brush.Viewbox.Left;
            else if (brush.Viewbox.Right + off2.X > 1)
                off2.X = brush.Viewbox.Right > 1 ? 0 : 1 - brush.Viewbox.Right;
            if (brush.Viewbox.Top + off2.Y < 0)
                off2.Y = brush.Viewbox.Top < 0 ? 0 : - brush.Viewbox.Top;
            else if (brush.Viewbox.Bottom + off2.Y > 1)
                off2.Y = brush.Viewbox.Bottom > 1 ? 0 : 1 - brush.Viewbox.Bottom;
            Rect rect = Rect.Offset(brush.Viewbox, off2);
            brush.Viewbox = rect;
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
