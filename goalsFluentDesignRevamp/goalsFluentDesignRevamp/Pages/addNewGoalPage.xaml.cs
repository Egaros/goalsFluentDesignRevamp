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
using Microsoft.Toolkit.Uwp;
using Windows.UI.ViewManagement;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.Phone.Devices.Notification;

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
        bool formHelperIsVisible = false;
        public enum focusedObject { Name, Target, Description };
        focusedObject textBoxSelected = new focusedObject();
        public int currentDay = DateTime.Now.Day;
        public int currentMonth = DateTime.Now.Month;
        public int currentYear = DateTime.Now.Year;
        public addNewGoalPage()
        {
            this.InitializeComponent();
            InputPane.GetForCurrentView().Hiding += softwareKeyboardHiding;
        }

        private void softwareKeyboardHiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            hideFormHelper();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            animateFormHelperHiding();
            try
            {
                StorageFile droppedImageFile = (StorageFile)e.Parameter;
                assignDroppedImageToGoalImage(droppedImageFile);
            }
            catch (Exception)
            {

                
            }
        }

        private void assignDroppedImageToGoalImage(StorageFile droppedImageFile)
        {

            filePath = droppedImageFile.Path.ToString();
            goalImage.Source = new BitmapImage(new Uri(filePath, UriKind.Relative));
            noImagePlaceholderTextBlock.Visibility = Visibility.Collapsed;
            addImageTextBlock.Text = "Change Image";
            logger.Log("Drag and drop image to create new goal");
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
                    if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification.VibrationDevice"))
                    {
                        VibrationDevice testVibrationDevice = VibrationDevice.GetDefault();
                        makeVibrations(testVibrationDevice);
                    }
                    DateTime goalEndTime = new DateTime();
                    if (timeLimitCheckBox.IsChecked == true)
                    {
                       goalEndTime = goalDatePicker.Date.DateTime;

                    }
                    else
                    {
                        goalEndTime = new DateTime(1, 1, 1);
                    }
                    errorTextBlock.Visibility = Visibility.Collapsed;
                    nameErrorTextBlock.Visibility = Visibility.Collapsed;
                    string description = descriptionTextBox.Text;
                    string imagePath;
                    imagePath = filePath;
                    goal.addNewGoal(name, target, description, imagePath, goalEndTime);
                    goal.saveGoals();
                    string historicalEvent = $"Added new goal called {name}.";
                    history.makeHistory(name, historicalEvent, DateTime.Now, eventType.NewGoal);
                    history.saveHistory();
                    logger.Log("Goals Created");
                    App.NavService.NavigateTo(typeof(MainPage), "addedOrUpdatedGoal");
                }
            }
            catch
            {
                errorTextBlock.Visibility = Visibility.Visible;

            }

        }

        private void makeVibrations(VibrationDevice testVibrationDevice)
        {
            testVibrationDevice.Vibrate(TimeSpan.FromSeconds(0.1));
        }
    

        private async void waitAWhile()
        {
            await Task.Delay(3000);
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
            var imageFolder = await folder.GetFolderAsync("ImageFolder");
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
                noImagePlaceholderTextBlock.Visibility = Visibility.Collapsed;
                addImageTextBlock.Text = "Change Image";
                logger.Log("Times image added during goal creation");
            }
        }

        private void previousButton_Click(object sender, RoutedEventArgs e)
        {

            switch (textBoxSelected)
            {
                case focusedObject.Target:
                    disablePreviousButton();
                    selectNameTextBox();
                    break;
                case focusedObject.Description:
                    enableNextButton();
                    selectTargetTextBox();
                    break;
                default:
                    break;
            }

           
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            switch (textBoxSelected)
            {
                case focusedObject.Name:
                    enablePreviousButton();
                    selectTargetTextBox();
                    break;
                case focusedObject.Target:
                    disableNextButton();
                    selectDescriptionTextBox();
                    break;
                default:
                    break;
            }
            
        }

        private void disablePreviousButton()
        {
            previousButton.IsEnabled = false;
        }

        private void disableNextButton()
        {
            nextButton.IsEnabled = false;
        }

        private void enablePreviousButton()
        {
            previousButton.IsEnabled = true;
        }

        private void enableNextButton()
        {
            nextButton.IsEnabled = true;
        }


        private void doneButton_Click(object sender, RoutedEventArgs e)
        {
            hideSoftwareKyboard();
            hideFormHelper();
        }

        private void hideSoftwareKyboard()
        {
            var softwareKeyboard = InputPane.GetForCurrentView();
            softwareKeyboard.TryHide();
        }

        private void nameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!formHelperIsVisible)
            {
            showFormHelper();
                hideCommandBar();
            }
            disablePreviousButton();
            enableNextButton();
           
            textBoxSelected = focusedObject.Name;

            
            
        }
        private void targetTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!formHelperIsVisible)
            {
                showFormHelper();
                hideCommandBar();
            }
                enablePreviousButton();
                enableNextButton();
            
            textBoxSelected = focusedObject.Target;

        }

        private void descriptionTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!formHelperIsVisible)
            {
                showFormHelper();
                hideCommandBar();
            }
                disableNextButton();
                enablePreviousButton();
           

            textBoxSelected = focusedObject.Description;
           
        }

        private void selectNameTextBox()
        {
            nameTextBox.Focus(FocusState.Programmatic);
            textBoxSelected = focusedObject.Name;
        }

        private void selectTargetTextBox()
        {
            targetTextBox.Focus(FocusState.Programmatic);
            textBoxSelected = focusedObject.Target;
        }

        private void selectDescriptionTextBox()
        {
            descriptionTextBox.Focus(FocusState.Programmatic);
            textBoxSelected = focusedObject.Description;
        }



        private void showFormHelper()
        {
            animateFormHelperShowing();
            formHelper.Visibility = Visibility.Visible;
            formHelperIsVisible = true;
        }

        private async void animateFormHelperShowing()
        {

            await formHelper.Scale(1f, 1f, 0, 48, 500).StartAsync();
            
        }

        private void hideFormHelper()
        {
            
            showCommandBar();
            animateFormHelperHiding();
            formHelper.Visibility = Visibility.Collapsed;
            formHelperIsVisible = false;
            mainCommandBar.Focus(FocusState.Programmatic);
           
        }

        private async void animateFormHelperHiding()
        {

            await formHelper.Scale(0.01f, 0.01f, 0, 48,500).StartAsync();
           
            
            
        }

        

        private void showCommandBar()
        {
            animateCommandBarShowing();
            mainCommandBar.Visibility = Visibility.Visible;
        }

        private async void animateCommandBarShowing()
        {
           await mainCommandBar.Fade(1f).StartAsync();
        }

        private async void hideCommandBar()
        {
            animateCommandBarHiding();
            await Task.Delay(500);
            mainCommandBar.Visibility = Visibility.Collapsed;
        }

        private async void animateCommandBarHiding()
        {
           await mainCommandBar.Fade(0.01f).StartAsync();
        }
    }
}
