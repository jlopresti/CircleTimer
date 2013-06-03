using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CircleTimerControl.Sample.Win8
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            ct.TimerCompleted += ct_TimerCompleted;
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        async void ct_TimerCompleted(object sender, EventArgs e)
        {
            var dlg = new MessageDialog("Bouuuuuuuuuuuuuuuuuum");
            await dlg.ShowAsync();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ct.State == CircleTimerState.Stopped
                || ct.State == CircleTimerState.Cancelled)
            {
                stBtn.Content = "Stop";
                ct.Start();
            }
            else if (ct.State == CircleTimerState.Running
                || ct.State == CircleTimerState.Paused)
            {
                stBtn.Content = "Start";
                ct.Cancel();
            }
        }

        private void tgBtn_Click(object sender, RoutedEventArgs e)
        {
            ct.IsIndeterminate = !ct.IsIndeterminate;
        }

        private void psBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ct.State == CircleTimerState.Paused)
            {
                psBtn.Content = "Pause";
                ct.Resume();
            }
            else if (ct.State == CircleTimerState.Running)
            {
                psBtn.Content = "Resume";
                ct.Pause();
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
    }
}
