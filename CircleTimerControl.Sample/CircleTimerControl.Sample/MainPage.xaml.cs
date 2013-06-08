using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CircleTimerControl.Sample.Resources;

namespace CircleTimerControl.Sample
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            ct.TimerCompleted += ct_TimerCompleted;
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        void ct_TimerCompleted(object sender, EventArgs e)
        {
            MessageBox.Show("Bouuum");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ct.State == CircleTimerState.Stopped 
                || ct.State == CircleTimerState.Cancelled)
            {
                stBtn.Content = "Stop";
                ct.Start();
            }
            else if(ct.State == CircleTimerState.Running 
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

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}