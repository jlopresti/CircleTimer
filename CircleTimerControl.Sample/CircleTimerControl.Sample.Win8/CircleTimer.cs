using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace CircleTimerControl.Sample
{
    [TemplatePart(Name = EllapsedText_PART, Type = typeof(TextBlock))]
    [TemplateVisualState(GroupName = CommonStatesGroup_PART, Name = DeterminateState_PART)]
    [TemplatePart(Name = EllapsedCircle_PART, Type = typeof(RingSlice))]
    [TemplateVisualState(GroupName = CommonStatesGroup_PART, Name = IndeterminateState_PART)]
    public class CircleTimer : Control
    {
        private const string EllapsedCircle_PART = "EllapsedCircle";
        private const string EllapsedText_PART = "EllapsedText";
        private const string CommonStatesGroup_PART = "CommonStates";
        private const string DeterminateState_PART = "Determinate";
        private const string IndeterminateState_PART = "Indeterminate";

        public static readonly DependencyProperty ArcThicknessProperty = DependencyProperty.Register("ArcThickness", typeof(double), typeof(CircleTimer), new PropertyMetadata(10.0));
        public static readonly DependencyProperty IsIndeterminateProperty = DependencyProperty.Register("IsIndeterminate", typeof(bool), typeof(CircleTimer), new PropertyMetadata(false, new PropertyChangedCallback(CircleTimer.OnIndeterminateChanged)));
        public static readonly DependencyProperty DurationInMsProperty = DependencyProperty.Register("DurationInMs", typeof(int), typeof(CircleTimer), new PropertyMetadata(10000));
        public static readonly DependencyProperty CountdownVisibilityProperty = DependencyProperty.Register("CountdownVisibility", typeof(Visibility), typeof(CircleTimer), new PropertyMetadata(Visibility.Collapsed));
        public static readonly DependencyProperty CircleFillProperty = DependencyProperty.Register("CircleFill", typeof(SolidColorBrush), typeof(CircleTimer), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(127, 139, 0, 0))));
        public static readonly DependencyProperty ProgressCircleFillProperty = DependencyProperty.Register("ProgressCircleFill", typeof(SolidColorBrush), typeof(CircleTimer), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 139, 0, 0))));
        public static readonly DependencyProperty IndeterminateCursorFillProperty = DependencyProperty.Register("IndeterminateCursorFill", typeof(SolidColorBrush), typeof(CircleTimer), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 139, 0, 0))));



        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(CircleTimer), new PropertyMetadata(50.0));




        public double InnerRadius
        {
            get { return (double)GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InnerRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register("InnerRadius", typeof(double), typeof(CircleTimer), new PropertyMetadata(25.0));


        public CircleTimerState State { get; private set; }

        private RingSlice _ellapsedCircle;
        private TextBlock _ellapsedText;
        private DispatcherTimer _timer;
        private int _ellapsedTime;

        public double ArcThickness
        {
            get
            {
                return (double)this.GetValue(CircleTimer.ArcThicknessProperty);
            }
            set
            {
                this.SetValue(CircleTimer.ArcThicknessProperty, value);
            }
        }

        public bool IsIndeterminate
        {
            get
            {
                return (bool)this.GetValue(CircleTimer.IsIndeterminateProperty);
            }
            set
            {
                this.SetValue(CircleTimer.IsIndeterminateProperty, value);
            }
        }

        public int DurationInMs
        {
            get
            {
                return (int)this.GetValue(CircleTimer.DurationInMsProperty);
            }
            set
            {
                this.SetValue(CircleTimer.DurationInMsProperty, value);
            }
        }

        public Visibility CountdownVisibility
        {
            get
            {
                return (Visibility)this.GetValue(CircleTimer.CountdownVisibilityProperty);
            }
            set
            {
                this.SetValue(CircleTimer.CountdownVisibilityProperty, value);
            }
        }

        public SolidColorBrush CircleFill
        {
            get
            {
                return (SolidColorBrush)this.GetValue(CircleTimer.CircleFillProperty);
            }
            set
            {
                this.SetValue(CircleTimer.CircleFillProperty, value);
            }
        }

        public SolidColorBrush ProgressCircleFill
        {
            get
            {
                return (SolidColorBrush)this.GetValue(CircleTimer.ProgressCircleFillProperty);
            }
            set
            {
                this.SetValue(CircleTimer.ProgressCircleFillProperty, value);
            }
        }

        public SolidColorBrush IndeterminateCursorFill
        {
            get
            {
                return (SolidColorBrush)this.GetValue(CircleTimer.IndeterminateCursorFillProperty);
            }
            set
            {
                this.SetValue(CircleTimer.IndeterminateCursorFillProperty, value);
            }
        }

        public event EventHandler TimerCompleted;

        public CircleTimer()
        {
            DefaultStyleKey = typeof(CircleTimer);
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += timer_Tick;
            State = CircleTimerState.Stopped;
        }

        protected override void OnApplyTemplate()
        {
            _ellapsedCircle = GetTemplateChild("EllapsedCircle") as RingSlice;
            _ellapsedText = GetTemplateChild("EllapsedText") as TextBlock;
        }

        private static void OnIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ct = (CircleTimer)d;
            ct.Cancel();
            if ((bool)e.NewValue)
            {
                ct.State = CircleTimerState.Indeterminate;
            }
            else
            {
                ct.State = CircleTimerState.Stopped;
            }
            ct.UpdateVisualState((bool)e.NewValue);
        }

        private void UpdateVisualState(bool isIndeterminate)
        {
            VisualStateManager.GoToState((Control)this, isIndeterminate ? IndeterminateState_PART : DeterminateState_PART, true);
        }

        public void Start()
        {
            if (IsIndeterminate) return;

            if (DurationInMs <= 0) throw new ArgumentOutOfRangeException("DurationInMs must be superior to 0");

            Cancel();
            _ellapsedText.Text = GetDurationInSeconds().ToString();
            _timer.Start();
            State = CircleTimerState.Running;
        }

        public void Pause()
        {
            _timer.Stop();
            State = CircleTimerState.Paused;
        }

        public void Resume()
        {
            _timer.Start();
            State = CircleTimerState.Running;
        }

        public void Cancel()
        {
            _timer.Stop();
            _ellapsedTime = 0;
            _ellapsedCircle.EndAngle = 0;
            _ellapsedText.Text = string.Empty;
            State = CircleTimerState.Cancelled;
        }

        private int GetDurationInSeconds()
        {
            return this.DurationInMs / 1000;
        }

        private void Stop()
        {
            this._timer.Stop();
            State = CircleTimerState.Stopped;
        }

        private async void timer_Tick(object sender, object o)
        {
            _ellapsedTime++;
            _ellapsedText.Text = (GetDurationInSeconds() - _ellapsedTime).ToString();
            var sb = GetAngleStoryboard();
            sb.Begin();

            if (GetDurationInSeconds() == _ellapsedTime)
            {
                Stop();
                if (TimerCompleted != null)
                {
                    await Task.Delay(350);
                    TimerCompleted(this, new EventArgs());
                }
            }

        }

        private Storyboard GetAngleStoryboard()
        {
            Storyboard sb = new Storyboard();
            DoubleAnimationUsingKeyFrames da = new DoubleAnimationUsingKeyFrames();
            var from = new EasingDoubleKeyFrame()
                {
                    KeyTime = TimeSpan.FromSeconds(0),
                    Value = _ellapsedCircle.EndAngle
                };
            var to = new EasingDoubleKeyFrame()
                {
                    KeyTime = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new ExponentialEase(){ EasingMode =  EasingMode.EaseOut},
                    Value = _ellapsedCircle.EndAngle + (360/this.GetDurationInSeconds())
                };
            da.FillBehavior = FillBehavior.HoldEnd;
            da.KeyFrames.Add(from);
            da.KeyFrames.Add(to);
            sb.Children.Add(da);
            da.EnableDependentAnimation = true;
            Storyboard.SetTarget(da, _ellapsedCircle);
            Storyboard.SetTargetProperty(da, "(RingSlice.EndAngle)");
            return sb;
        }
    }

    public enum CircleTimerState
    {
        Stopped,
        Paused,
        Running,
        Cancelled,
        Indeterminate
    }
}
