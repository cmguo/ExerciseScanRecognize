using Base.Misc;
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

        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center", typeof(Point), typeof(PaperViewer),
                new PropertyMetadata((o, e) => (o as PaperViewer).SetCenter((Point)e.NewValue)));

        public static readonly DependencyProperty ScaleModeProperty =
            DependencyProperty.Register("ScaleMode", typeof(ScaleModes), typeof(PaperViewer),
                new PropertyMetadata(ScaleModes.Manual, (o, e) => (o as PaperViewer).SetScaleMode((ScaleModes)e.NewValue)));

        public static readonly DependencyProperty FocusRectProperty =
            DependencyProperty.Register("FocusRect", typeof(Rect), typeof(PaperViewer),
                new PropertyMetadata((o, e) => (o as PaperViewer).SetFocusRect((Rect)e.NewValue)));

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

        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public Point Center
        {
            get => (Point)GetValue(CenterProperty);
            set => SetValue(CenterProperty, value);
        }

        public ScaleModes ScaleMode
        {
            get => (ScaleModes) GetValue(ScaleModeProperty);
            set => SetValue(ScaleModeProperty, value);
        }

        public Rect FocusRect
        {
            get => (Rect)GetValue(FocusRectProperty);
            set => SetValue(FocusRectProperty, value);
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

        private int adjust = 0;

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
                image.Rect = new Rect(0, 0, paper.PixelWidth, paper.PixelHeight);
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
            adjust |= 1;
            Adjust();
        }

        private void SetFocusRect(Rect f)
        {
            adjust |= 2;
            Adjust();
        }

        private void SetScale(double v)
        {
            Adjust();
        }

        private void SetCenter(Point c)
        {
            Adjust();
        }

        private void AdjustMode()
        {
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

        private void AdjustFocusRect()
        {
            double s = (RenderSize.Width * paper.Height) / (RenderSize.Height * paper.Width);
            double sw = paper.PixelWidth / FocusRect.Width;
            double sh = paper.PixelHeight / FocusRect.Height;
            if (s > 1)
                sh /= s;
            else
                sw *= s;
            Scale = sw < sh ? sw : sh;
            Center = new Point((FocusRect.Left + FocusRect.Right) / 2 / paper.PixelWidth,
                (FocusRect.Top + FocusRect.Bottom) / 2 / paper.PixelHeight);
        }

        private void Adjust()
        {
            if (RenderSize.Width <= 0 || RenderSize.Height <= 0)
                return;
            if (paper == null)
                return;
            if (adjust != 0)
            {
                if ((adjust & 4) != 0)
                    return;
                adjust |= 4;
                if ((adjust & 2) != 0)
                    AdjustFocusRect();
                else if ((adjust & 1) != 0)
                    AdjustMode();
                adjust = 0;
                Adjust();
                return;
            }
            double s = (RenderSize.Width * paper.PixelHeight) / (RenderSize.Height * paper.PixelWidth);
            if (s > 1)
            {
                scaleX = 1 / Scale;
                scaleY = 1 / s / Scale;
            }
            else
            {
                scaleX = s / Scale;
                scaleY = 1 / Scale;
            }
            Rect rect = new Rect(0, 0, scaleX, scaleY);
            if (rect.Right > 1)
                rect.Offset((1 - rect.Right) / 2, 0);
            if (rect.Bottom > 1)
                rect.Offset(0, (1 - rect.Bottom) / 2);
            Point center = new Point((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2);
            Vector off = Point.Subtract(Center, center);
            if (rect.Left + off.X < 0)
                off.X = rect.Left < 0 ? 0 : -rect.Left;
            else if (rect.Right + off.X > 1)
                off.X = rect.Right > 1 ? 0 : 1 - rect.Right;
            if (rect.Top + off.Y < 0)
                off.Y = rect.Top < 0 ? 0 : -rect.Top;
            else if (rect.Bottom + off.Y > 1)
                off.Y = rect.Bottom > 1 ? 0 : 1 - rect.Bottom;
            rect.Offset(off);
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
            Rect rect = brush.Viewbox;
            Point center = new Point((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2);
            Center = Point.Add(center, off2);
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
