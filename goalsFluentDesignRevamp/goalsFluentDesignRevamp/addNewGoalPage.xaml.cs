using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using goalsFluentDesignRevamp.Model;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media.Animation;
using Microsoft.Services.Store.Engagement;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace goalsFluentDesignRevamp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class addNewGoalPage : Page
    {
        string filePath;
        StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
        public addNewGoalPage()
        {
            this.InitializeComponent();
        }

        private void confirmNewGoalBottom_Click(object sender, RoutedEventArgs e)
        {
            App.SFXSystem.Source = App.clickSFXSource;
            App.SFXSystem.Play();
            try
            {
                decimal target = decimal.Parse(targetTextBox.Text);
                string name = nameTextBox.Text;
                var listOfGoals = goal.listOfGoals.Where(p => p.name == name).ToList();
                if (listOfGoals.Count > 0)
                {
                    nameErrorTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    errorTextBlock.Visibility = Visibility.Collapsed;
                    nameErrorTextBlock.Visibility = Visibility.Collapsed;
                    string description = descriptionTextBox.Text;
                    string imagePath;
                    imagePath = filePath;
                    goal.addNewGoal(name, target, description, imagePath);
                    goal.saveGoals();
                    string historicalEvent = $"Added new goal called {name}.";
                    history.makeHistory(name, historicalEvent, DateTime.Now, eventType.NewGoal);
                    history.saveHistory();
                    logger.Log("Goals Created");
                    App.NavService.NavigateTo(typeof(MainPage));
                }
            }
            catch
            {
                errorTextBlock.Visibility = Visibility.Visible;

            }

        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.SFXSystem.Source = App.cancelClickSFXSource;
            App.SFXSystem.Play();
            App.NavService.NavigateBack();
        }

        private async void addImageButton_Click(object sender, RoutedEventArgs e)
        {
            var folder = ApplicationData.Current.LocalFolder;
            var imageFolder = await folder.CreateFolderAsync("ImageFolder", CreationCollisionOption.OpenIfExists);
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation =
            Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                StorageFile usedFile = await file.CopyAsync(imageFolder, file.Name, NameCollisionOption.ReplaceExisting);
                filePath = usedFile.Path.ToString();
                goalImage.Source = new BitmapImage(new Uri(filePath, UriKind.Absolute));
                noImagePlaceholderTextBlock.Visibility = Visibility.Collapsed;
                addImageTextBlock.Text = "Change Image";
                logger.Log("Times image added during goal creation");
            }
        }
    }
}
