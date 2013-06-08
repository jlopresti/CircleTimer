using Microsoft.Expression.Shapes;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CircleTimerControl.Sample
{
    [TemplatePart(Name = EllapsedText_PART, Type = typeof(TextBlock))]
    [TemplateVisualState(GroupName = CommonStatesGroup_PART, Name = DeterminateState_PART)]
    [TemplatePart(Name = EllapsedCircle_PART, Type = typeof(Arc))]
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
        public CircleTimerState State { get; private set; }

        private Arc _ellapsedCircle;
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

        public override void OnApplyTemplate()
        {
            _ellapsedCircle = GetTemplateChild("EllapsedCircle") as Arc;
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
            if (_currentStoryboard != null)
                _currentStoryboard.Stop();
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

        private async void timer_Tick(object sender, EventArgs e)
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

        private Storyboard _currentStoryboard;
        private Storyboard GetAngleStoryboard()
        {
            _currentStoryboard = new Storyboard();
            DoubleAnimation da = new DoubleAnimation();
            da.From = _ellapsedCircle.EndAngle;
            da.To = Math.Min(_ellapsedCircle.EndAngle + (360 / this.GetDurationInSeconds()), 360);
            da.FillBehavior = FillBehavior.HoldEnd;
            da.Duration = TimeSpan.FromMilliseconds(300);
            da.EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut };
            _currentStoryboard.Children.Add(da);
            Storyboard.SetTarget(da, _ellapsedCircle);
            Storyboard.SetTargetProperty(da, new PropertyPath(Arc.EndAngleProperty));
            return _currentStoryboard;
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
