using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace CircleTimerControl.Sample
{
    /// <summary>
    /// A Path that represents a ring slice with a given
    /// (outer) Radius,
    /// InnerRadius,
    /// StartAngle,
    /// EndAngle and
    /// Center.
    /// </summary>    /// 
    public class RingSlice : Path
    {
        private bool _isUpdating;

        #region StartAngle

        /// <summary>
        /// The start angle property.
        /// </summary>
        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register(
                "StartAngle",
                typeof (double),
                typeof (RingSlice),
                new PropertyMetadata(
                    0d,
                    OnStartAngleChanged));

        /// <summary>
        /// Gets or sets the start angle.
        /// </summary>
        /// <value>
        /// The start angle.
        /// </value>
        public double StartAngle
        {
            get { return (double) GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        private static void OnStartAngleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var target = (RingSlice) sender;
            var oldStartAngle = (double) e.OldValue;
            var newStartAngle = (double) e.NewValue;
            target.OnStartAngleChanged(oldStartAngle, newStartAngle);
        }

        private void OnStartAngleChanged(double oldStartAngle, double newStartAngle)
        {
            UpdatePath();
        }

        #endregion

        #region EndAngle

        /// <summary>
        /// The end angle property.
        /// </summary>
        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register(
                "EndAngle",
                typeof (double),
                typeof (RingSlice),
                new PropertyMetadata(
                    0d,
                    OnEndAngleChanged));

        /// <summary>
        /// Gets or sets the end angle.
        /// </summary>
        /// <value>
        /// The end angle.
        /// </value>
        public double EndAngle
        {
            get { return (double) GetValue(EndAngleProperty); }
            set { SetValue(EndAngleProperty, value); }
        }

        private static void OnEndAngleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var target = (RingSlice) sender;
            var oldEndAngle = (double) e.OldValue;
            var newEndAngle = (double) e.NewValue;
            target.OnEndAngleChanged(oldEndAngle, newEndAngle);
        }

        private void OnEndAngleChanged(double oldEndAngle, double newEndAngle)
        {
            UpdatePath();
        }

        #endregion

        #region Radius

        /// <summary>
        /// The radius property
        /// </summary>
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register(
                "Radius",
                typeof (double),
                typeof (RingSlice),
                new PropertyMetadata(
                    0d,
                    OnRadiusChanged));

        /// <summary>
        /// Gets or sets the outer radius.
        /// </summary>
        /// <value>
        /// The outer radius.
        /// </value>
        public double Radius
        {
            get { return (double) GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        private static void OnRadiusChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var target = (RingSlice) sender;
            var oldRadius = (double) e.OldValue;
            var newRadius = (double) e.NewValue;
            target.OnRadiusChanged(oldRadius, newRadius);
        }

        private void OnRadiusChanged(double oldRadius, double newRadius)
        {
            this.Width = this.Height = 2*Radius;
            UpdatePath();
        }

        #endregion

        


        /// <summary>
        /// Initializes a new instance of the <see cref="RingSlice" /> class.
        /// </summary>
        public RingSlice()
        {
            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            UpdatePath();
        }

        private void UpdatePath()
        {
            if (_isUpdating)
            {
                return;
            }

            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure();
            pathFigure.IsClosed = false;

            var center =
                new Point(
                    Radius ,
                    Radius);

            // Starting Point
            pathFigure.StartPoint =
                new Point( 
                    center.X + Math.Sin(StartAngle * Math.PI / 180) * (Radius - StrokeThickness/2),
                    center.Y - Math.Cos(StartAngle * Math.PI / 180) * (Radius - StrokeThickness / 2));

            // Outer Arc
            var outerArcSegment = new ArcSegment();
            outerArcSegment.IsLargeArc = (EndAngle - StartAngle) >= 180.0;
            outerArcSegment.Point =
                new Point(
                    center.X + Math.Sin(Math.Min(359.99, EndAngle) * Math.PI / 180) * (Radius - StrokeThickness / 2),
                    center.Y - Math.Cos(Math.Min(359.99, EndAngle) * Math.PI / 180) * (Radius - StrokeThickness / 2));
            outerArcSegment.Size = new Size(Math.Max((Radius - StrokeThickness / 2), 0), Math.Max((Radius - StrokeThickness / 2), 0));
            outerArcSegment.SweepDirection = SweepDirection.Clockwise;

            pathFigure.Segments.Add(outerArcSegment);
            pathGeometry.Figures.Add(pathFigure);

            this.InvalidateArrange();
            this.Data = pathGeometry;
        }
    }
}
