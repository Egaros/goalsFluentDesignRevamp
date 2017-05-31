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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media.Animation;
using Windows.Storage;
using goalsFluentDesignRevamp.TileService;
using Microsoft.Services.Store.Engagement;
using Windows.Networking.Connectivity;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace goalsFluentDesignRevamp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class selectedGoalPage : Page
    {

        goal selectedGoal;
        decimal targetRemaining;
        string filePath;
        StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
        decimal amountSubmitted;
        public selectedGoalPage()
        {

            this.InitializeComponent();

        }

        

        private void navigateToMainPage()
        {

            App.NavService.NavigateTo(typeof(MainPage), "adShown");

        }


        public static bool IsInternet()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return internet;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            selectedGoal = (goal)e.Parameter;
            showPresenceOfGoalInUI(selectedGoal);
        }

        private void showPresenceOfGoalInUI(goal goal)
        {
            string name = goal.name;
            string title = name.ToUpper();
            titleTextBlock.Text = title;
            goalNameTextBlock.Text = name;
            descriptionTextBlock.Text = goal.description;
            targetRemaining = goal.target - goal.targetReached;
            targetRemainingTextBlock.Text = string.Format("{0:C} left!", targetRemaining);
            if (goal.imagePath != "ms-appx:///Assets/noImage.png")
            {
                ImageSource goalImageSource = new BitmapImage(new Uri(goal.imagePath, UriKind.Relative));
                goalImage.Source = goalImageSource;
            }

        }

        private void submitAmountButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                amountSubmitted = decimal.Parse(updateGoalTextBox.Text);
                targetRemaining -= amountSubmitted;

                if (targetRemaining < 0)
                {
                    targetRemaining = 0;

                }
                updateTargetTextBlock(targetRemaining);
                updateGoalTextBox.Text = string.Empty;
                errorTextBlock.Visibility = Visibility.Collapsed;
            }
            catch
            {
                updateGoalTextBox.Text = string.Empty;
                errorTextBlock.Visibility = Visibility.Visible;
            }
            updateProgressFlyout.Hide();
        }

        private void updateTargetTextBlock(decimal targetRemaining)
        {
            targetRemainingTextBlock.Text = string.Format("{0:C} left!", targetRemaining);
        }

        private void finishChangesButton_Click(object sender, RoutedEventArgs e)
        {
            App.SFXSystem.Source = App.clickSFXSource;
            App.SFXSystem.Play();
            selectedGoal.targetReached = selectedGoal.target - targetRemaining;
            decimal percentage = Math.Round((selectedGoal.targetReached / selectedGoal.target) * 100);
            selectedGoal.progress = $"Progress: {percentage}%";
            tile.updateExistingTile(selectedGoal.name, selectedGoal.progress, selectedGoal.description, selectedGoal.imagePath);
            string historicalEvent = String.Format("Added {0:C} towards {1}.", amountSubmitted, selectedGoal.name);
            history.makeHistory(selectedGoal.name, historicalEvent, DateTime.Now, eventType.PositiveUpdate);


            if (selectedGoal.targetReached == selectedGoal.target)
            {
                determineImageToSetToGoal();
                goal.makeCompletedGoal(selectedGoal, DateTime.Now);
                goal.saveGoals();
                historicalEvent = $"You achieved the goal: {selectedGoal.name}";
                history.makeHistory(selectedGoal.name, historicalEvent, DateTime.Now, eventType.CompletedGoal);
                history.saveHistory();
                logger.Log("Goals Completed");
                App.NavService.NavigateTo(typeof(goalCompletedPage));

            }
            else
            {

                determineImageToSetToGoal();
                goal.replaceGoal(selectedGoal);
                logger.Log("Goals Updated");
                goal.saveGoals();
                history.saveHistory();
               
                    App.NavService.NavigateTo(typeof(MainPage));
         
                

            }


        }

       

        private void determineImageToSetToGoal()
        {

            if (filePath != null)
            {
                selectedGoal.imagePath = filePath;
            }


        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.SFXSystem.Source = App.cancelClickSFXSource;
            App.SFXSystem.Play();
            App.NavService.NavigateBack();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            App.SFXSystem.Source = App.deleteClickSFXSource;
            App.SFXSystem.Play();
            String historicalEvent = $"You quit the goal: {selectedGoal.name}";
            history.makeHistory(selectedGoal.name, historicalEvent, DateTime.Now, eventType.DeletedGoal);
            history.saveHistory();
            List<goal> goalToRemove = goal.listOfGoals.Where(p => p.name == selectedGoal.name).ToList();
            goal.listOfGoals.Remove(goalToRemove[0]);
            goal.saveGoals();
            App.NavService.NavigateTo(typeof(MainPage));
        }

        private async void changeImageButton_Click(object sender, RoutedEventArgs e)
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
                logger.Log("Times Image changed after goal creation");
            }
        }

        private void pinTileButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Around end of June when you come back, remove this and if statement. It's only there for compaitablilty reasons
            //It basically fixes the bug and insures every goal has a tileID
            if (selectedGoal.tileID == null)
            {

                selectedGoal.tileID = goal.generateUniqueID();
            }
            try
            {
                tile.makeOrUpdateGoalTile(selectedGoal.name, selectedGoal.tileID, selectedGoal.progress, selectedGoal.description, selectedGoal.imagePath);
            }
            catch
            {

            }

            logger.Log("Times goal has been Pinned");
        }
    }
}