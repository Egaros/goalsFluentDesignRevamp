using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace goalsFluentDesignRevamp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class settingsPage : Page
    {
        bool initialOverrideSettings;
        bool initialThemeColorSettings;
        bool themeOverrideChanged = false;
        bool themeColorChanged = false;
        bool userSelection;
        string themeName = "";
        public settingsPage()
        {
            this.InitializeComponent();
            loadCurrentSettings();
            initialOverrideSettings = themeColorOverrideSwitch.IsOn;
            initialThemeColorSettings = themeColorSwitch.IsOn;
        }

        private void finishChangesButton_Click(object sender, RoutedEventArgs e)
        {
          
             if ( initialThemeColorSettings != themeColorSwitch.IsOn && themeColorSwitch.IsEnabled == true)
              {
               
                saveNewThemeSettings();
                showThemeColorChangedDialog();
              
            }

            else if (initialOverrideSettings != themeColorOverrideSwitch.IsOn)
            {
                saveNewThemeSettings();
                showThemeOverrideDialog();
            }
            else
	        {
                App.NavService.NavigateTo(typeof(MainPage));

            }
        }

        private void saveNewThemeSettings()
        {
            if (themeColorOverrideSwitch.IsOn)
            {

                
                App.localSettings.Values["isThemeColorOverrideEnabled"] = 1;
                
            }
            else
            {
                
                App.localSettings.Values["isThemeColorOverrideEnabled"] = 0;

            }



            if (themeColorSwitch.IsEnabled)
            {
                if (themeColorSwitch.IsOn == false)
                {
                   
                    App.localSettings.Values["themeColor"] = 0;
                }
                else
                {
                   
                    App.localSettings.Values["themeColor"] = 1;
                }

            }



            

        }

        private async void showThemeColorChangedDialog()
        {
            ContentDialog themeColorChangedDialog = new ContentDialog
            {
                Title = "Pay Attention To This Message",
                CloseButtonText = "I understand",
                IsPrimaryButtonEnabled = false
            };
            if (themeName == "")
            {
                themeColorChangedDialog.Content = $"In order for the new theme settings to be applied, you will need to restart the app.";

            }
            else
            {
                themeColorChangedDialog.Content = $"In order for the {themeName} theme to be applied, you will need to restart the app.";
            }

            themeColorChangedDialog.Closed += nowNavigateToMainPage;

            await themeColorChangedDialog.ShowAsync();
        }

        private async void showThemeOverrideDialog()
        {
             ContentDialog themeOverrideDialog = new ContentDialog
            {
                Title = "Pay Attention To This Message",
                Content = $"In order for these new theme settings to be applied, you will need to restart the app.",
                CloseButtonText = "I understand",
                IsPrimaryButtonEnabled = false
            };
            themeOverrideDialog.Closed += nowNavigateToMainPage;
            await themeOverrideDialog.ShowAsync();
        }

        private void themeColorOverrideSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            
            themeOverrideChanged = true;
            if (themeColorOverrideSwitch.IsOn)
            {
                
            themeColorSwitch.IsEnabled = true;
           
            }
            else
            {
                themeColorSwitch.IsEnabled = false;
                
                
            }
        }


        

        private void loadCurrentSettings()
        {
            if (App.localSettings.Values["isThemeColorOverrideEnabled"] != null && App.localSettings.Values["themeColor"] != null)
            {
            int currentThemeOverrideSettings = (int)App.localSettings.Values["isThemeColorOverrideEnabled"];
            int currentThemeColorSettings = (int)App.localSettings.Values["themeColor"];
            applyLoadedSettings(currentThemeOverrideSettings, currentThemeColorSettings);

            }
          
        }

        private void applyLoadedSettings(int currentThemeOverrideSettings, int currentThemeColorSettings)
        {
            if (currentThemeOverrideSettings == 1)
            {
                themeColorOverrideSwitch.IsOn = true;
                themeColorSwitch.IsEnabled = true;
            }

            if (currentThemeColorSettings == 1)
            {
                themeColorSwitch.IsOn = true;
            }
        }

            private void changeThemeNameBasedOnColorSwitch(bool userSelection)
        {
            
            if (userSelection == false)
            {
                themeName = "Dark";
                
            }
            else
            {
                themeName = "Light";
                
            }
           

          
        }

        private void nowNavigateToMainPage(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            App.NavService.NavigateTo(typeof(MainPage));
        }

        private void themeColorSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            themeColorChanged = true;
            bool userSelection = themeColorSwitch.IsOn;
            changeThemeNameBasedOnColorSwitch(userSelection);
        }

        private void themeColorSwitch_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (themeColorSwitch.IsEnabled == true)
            {
                themeColorChanged = true;
                bool userSelection = themeColorSwitch.IsOn;
                changeThemeNameBasedOnColorSwitch(userSelection);

            }

           
        }
    }
}
