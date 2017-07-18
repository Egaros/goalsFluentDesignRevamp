using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using goalsFluentDesignRevamp.Model;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media.Animation;
using Windows.Storage;
using goalsFluentDesignRevamp.TileService;
using Microsoft.Services.Store.Engagement;
using Windows.UI.Composition;

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
        Compositor _compositor;
        public selectedGoalPage()
        {

            this.InitializeComponent();
            ConnectedAnimation imageAnimation =
ConnectedAnimationService.GetForCurrentView().GetAnimation("image");
            if (imageAnimation != null)
            {
                titleBar.Opacity = 0;
                formGrid.Opacity = 0;
                goalImage.Opacity = 0;


                // Wait for image opened. In future Insider Preview releases, this won't be necessary.
                goalImage.ImageOpened += (sender_, e_) =>
                {
                    goalImage.Opacity = 1;


                    imageAnimation.TryStart(goalImage, new UIElement[] { titleBar, formGrid });

                    titleBar.Opacity = 1;
                    formGrid.Opacity = 1;

                };


            }

        }

      
       

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
           
    
            ConnectedAnimationService.GetForCurrentView().DefaultDuration = new TimeSpan(0, 0, 0, 0, 500);


            selectedGoal = (goal)e.Parameter;
            MainPage.persistedItem = selectedGoal;
            showPresenceOfGoalInUI(selectedGoal);
            bool timeLimitIsReached = checkIfTimeLimitReached();
            if (timeLimitIsReached)
            {
                updateProgressButton.IsEnabled = false;
                showErrorDialogBox();
            }

            
        }

        private async void showErrorDialogBox()
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = "Goal has reached its time limit 😞",
                Content = "You must delete this goal now but you can always try again (hopefully soon!)",
                IsPrimaryButtonEnabled = true,
                PrimaryButtonText = "Delete",
                CloseButtonText = "Go Back",
            };

            errorDialog.PrimaryButtonClick += ErrorDialog_PrimaryButtonClick;
            errorDialog.CloseButtonClick += ErrorDialog_CloseButtonClick;
            await errorDialog.ShowAsync();
        }

        private void ErrorDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            App.NavService.NavigateBack();
        }

        private void ErrorDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            App.SFXSystem.Source = App.deleteClickSFXSource;
            App.SFXSystem.Play();
            String historicalEvent = $"You quit the goal: {selectedGoal.name}";
            history.makeHistory(selectedGoal.name, historicalEvent, DateTime.Now, eventType.DeletedGoal);
            history.saveHistory();
            List<goal> goalToRemove = goal.listOfGoals.Where(p => p.name == selectedGoal.name).ToList();
            goal.listOfGoals.Remove(goalToRemove[0]);
            goal.saveGoals();
            App.NavService.NavigateTo(typeof(MainPage), "addedOrUpdatedGoal");
        }

        private void showErrorTextBlock()
        {
            errorTextBlock.Visibility = Visibility.Visible;
        }

        private bool checkIfTimeLimitReached()
        {
            bool isTimeLimitReached = false;
            if (selectedGoal.unitsOfTimeRemaining == "No more time left.")
            {
                isTimeLimitReached = true;
            }
            return isTimeLimitReached;
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
            decimal percentage = Math.Floor((selectedGoal.targetReached / selectedGoal.target) * 100);
            selectedGoal.progress = $"Progress: {percentage}%";
            string historicalEvent = String.Format("Added {0:C} towards {1}.", amountSubmitted, selectedGoal.name);
            history.makeHistory(selectedGoal.name, historicalEvent, DateTime.Now, eventType.PositiveUpdate);
            determineImageToSetToGoal();
            tryToUpdateTile();
           

            if (selectedGoal.targetReached == selectedGoal.target)
            {
                
                goal.makeCompletedGoal(selectedGoal, DateTime.Now);
                goal.saveGoals();
                historicalEvent = $"You achieved the goal: {selectedGoal.name}";
                history.makeHistory(selectedGoal.name, historicalEvent, DateTime.Now, eventType.CompletedGoal);
                history.saveHistory();
                logger.Log("Goals Completed");
                App.NavService.NavigateTo(typeof(goalCompletedPage), "addedOrUpdatedGoal");

            }
            else
            {

                goal.replaceGoal(selectedGoal);
                logger.Log("Goals Updated");
                goal.saveGoals();
                history.saveHistory();
               
                    App.NavService.NavigateTo(typeof(MainPage), "addedOrUpdatedGoal");
         
                

            }


        }

        private void tryToUpdateTile()
        {
            bool tileExists = tile.checkIfTileIsPinned(selectedGoal.tileID);
            if (tileExists)
            {
                tile.updateExistingTile(selectedGoal.name, selectedGoal.progress, selectedGoal.description, selectedGoal.imagePath, selectedGoal.tileID);
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
            //App.NavService.NavigateBack();
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("returnImage", goalImage);
            if (Frame.BackStack.Count == 0)
            {
                Frame.Navigate(typeof(MainPage));
            }
            else
            {
            Frame.GoBack(new SuppressNavigationTransitionInfo());

            }
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
            App.NavService.NavigateTo(typeof(MainPage), "addedOrUpdatedGoal");
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
                goalImage.Source = new BitmapImage(new Uri(filePath, UriKind.Relative));
                logger.Log("Times Image changed after goal creation");
            }
        }

        private void pinTileButton_Click(object sender, RoutedEventArgs e)
        {
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