﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace goalsFluentDesignRevamp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class goalCompletedPage : Page
    {
        public goalCompletedPage()
        {
            this.InitializeComponent();
        }

        private void contiueButton_Click(object sender, RoutedEventArgs e)
        {
            App.SFXSystem.Source = App.clickSFXSource;
            App.SFXSystem.Play();
            App.NavService.NavigateTo(typeof(MainPage), "addedOrUpdatedGoal");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.SFXSystem.Source = App.congratsSFXSource;
            App.SFXSystem.Play();
            congratsScreenReveal.Begin();
        }

    }
}
