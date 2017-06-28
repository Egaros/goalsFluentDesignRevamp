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
using System.Collections.ObjectModel;
using Windows.System;
using Microsoft.Services.Store.Engagement;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;
using goalsFluentDesignRevamp.TileService;
using Windows.Media.Core;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Composition;
using Windows.UI;
using System.Numerics;
using goalsFluentDesignRevamp.Control;
using Microsoft.Toolkit.Uwp;
using goalsFluentDesignRevamp.Services;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Windows.Storage;
using Microsoft.Toolkit.Uwp.UI.Animations;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace goalsFluentDesignRevamp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<goal> goalsToDisplay;
        public ObservableCollection<goal.completedGoal> completedGoalsToDisplay;
        public ObservableCollection<history> historyToDisplay = new ObservableCollection<history>();
        StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
        bool archiveEditingMode = false;
        goal.completedGoal goalInContextContainer;
        public MainPage()
        {
            this.InitializeComponent();
            loadHistory();
            loadGoals();
            checkIfDeviceHasFeedbackHub();


        }

        private void loadHistory()
        {
            List<history> orderedHistory = history.listOfHistory.Reverse().ToList();
            foreach (history historicalEvent in orderedHistory)
            {
                historyToDisplay.Add(historicalEvent);

            }

        }

        private void loadGoals()
        {
            goalsToDisplay = goal.listOfGoals;
            completedGoalsToDisplay = goal.listOfCompletedGoals;
        }

        private void checkIfDeviceHasFeedbackHub()
        {
            if (Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported())
            {
                this.feedbackButton.Visibility = Visibility.Visible;
            }
        }






        private void decideIfTutorialTextWillShow()
        {
            if (!(goalsToDisplay.Count == 0 || goalsToDisplay == null))
            {
                tutorialTextBlock.Visibility = Visibility.Collapsed;
                goalsGridView.Visibility = Visibility.Visible;
            }

            if (!(completedGoalsToDisplay.Count == 0 || completedGoalsToDisplay == null))
            {
                noCompletedGoalsTextBlock.Visibility = Visibility.Collapsed;
                completedGoalGridView.Visibility = Visibility.Visible;
            }

            if (!(historyToDisplay.Count == 0 || historyToDisplay == null))
            {
                noHistoryTextBlock.Visibility = Visibility.Collapsed;
                historyGridView.Visibility = Visibility.Visible;
            }

        }


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            decideIfTutorialTextWillShow();
            if ((string)e.Parameter == "goalCreated")
            {
                await Task.Delay(100);


                
                showTheDialog();
                
            }
            
        }

        private async void showTheDialog()
        {
            reviewDialog.CloseButtonText = "Maybe Later";
            await reviewDialog.ShowAsync();
        }

        private void hideMainMenu()
        {
            mainPivot.Visibility = Visibility.Collapsed;
        }

        private void newGoalButton_Click(object sender, RoutedEventArgs e)
        {

            App.SFXSystem.Source = App.clickSFXSource;
            App.SFXSystem.Play();
            logger.Log("New Goal Button Clicked");
            App.NavService.NavigateTo(typeof(addNewGoalPage));


        }

        private void goalsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            App.SFXSystem.Source = App.clickSFXSource;
            App.SFXSystem.Play();
            var selectedGoal = (goal)e.ClickedItem;
            App.NavService.NavigateTo(typeof(selectedGoalPage), selectedGoal);

        }







        private async void privacyPolicyButton_Click(object sender, RoutedEventArgs e)
        {
            App.SFXSystem.Source = App.clickSFXSource;
            App.SFXSystem.Play();
            await Launcher.LaunchUriAsync(new Uri("https://docs.google.com/document/d/1rqNmjFQaLBLVTluXYLXgImp65tcBy7vf0uIpFUCItlI/edit?usp=sharing"));

        }

        private async void feedbackButton_Click(object sender, RoutedEventArgs e)
        {
            App.SFXSystem.Source = App.clickSFXSource;
            App.SFXSystem.Play();
            logger.Log("Times Feedback Button Clicked");
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();

        }

        private async void reviewButton_Click(object sender, RoutedEventArgs e)
        {
            App.SFXSystem.Source = App.clickSFXSource;
            App.SFXSystem.Play();
            await Launcher.LaunchUriAsync(new Uri(string.Format("ms-windows-store:REVIEW?PFN={0}", Windows.ApplicationModel.Package.Current.Id.FamilyName)));
            logger.Log("Times Review button is clicked");
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            App.SFXSystem.Source = App.deleteClickSFXSource;
            App.SFXSystem.Play();

            do
            {
                foreach (goal.completedGoal item in completedGoalGridView.SelectedItems)
                {
                    completedGoalsToDisplay.Remove(item);
                }
            } while (completedGoalGridView.SelectedItems.Count != 0);

            goal.completedGoal.listOfCompletedGoals = completedGoalsToDisplay;
            goal.saveGoals();
            if (completedGoalsToDisplay.Count == 0)
            {
                completedGoalGridView.Visibility = Visibility.Collapsed;
                noCompletedGoalsTextBlock.Visibility = Visibility.Visible;

            }
            disableArchiveEditingMode();

        }

        private void completedGoalGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            App.SFXSystem.Source = App.selectClickSFXSource;
            App.SFXSystem.Play();
            completedGoalGridView.SelectionMode = ListViewSelectionMode.Multiple;
            enableArchiveEditingMode();


        }

        private void enableArchiveEditingMode()
        {

            completedGoalGridView.SelectionMode = ListViewSelectionMode.Multiple;
            deleteButton.Visibility = Visibility.Visible;
            cancelButton.Visibility = Visibility.Visible;
            newGoalButton.Visibility = Visibility.Collapsed;
            editListButton.Visibility = Visibility.Collapsed;
            archiveEditingMode = true;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.SFXSystem.Source = App.cancelClickSFXSource;
            App.SFXSystem.Play();
            disableArchiveEditingMode();
        }

        private void disableArchiveEditingMode()
        {
            deleteButton.Visibility = Visibility.Collapsed;
            cancelButton.Visibility = Visibility.Collapsed;
            newGoalButton.Visibility = Visibility.Visible;
            editListButton.Visibility = Visibility.Visible;
            completedGoalGridView.SelectionMode = ListViewSelectionMode.None;
            archiveEditingMode = false;
        }

        private void mainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (mainPivot.SelectedItem == goalsPivotItem)
            {
                disableArchiveEditingMode();
                editListButton.Visibility = Visibility.Collapsed;
            }
            if (mainPivot.SelectedItem == archivePivotItem)
            {
                if (archiveEditingMode == false)
                {
                    editListButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void editListButton_Click(object sender, RoutedEventArgs e)
        {
            App.SFXSystem.Source = App.selectModeClickSFXSource;
            App.SFXSystem.Play();
            enableArchiveEditingMode();
            completedGoalGridView.SelectionMode = ListViewSelectionMode.Multiple;
        }

        private void deleteGoalFromContext(object sender, RoutedEventArgs e)
        {
            goal.completedGoal goalToDelete = goalInContextContainer;
            completedGoalsToDisplay.Remove(goalToDelete);
            goal.completedGoal.listOfCompletedGoals = completedGoalsToDisplay;
            goal.saveGoals();
            if (completedGoalsToDisplay.Count == 0)
            {
                completedGoalGridView.Visibility = Visibility.Collapsed;
                noCompletedGoalsTextBlock.Visibility = Visibility.Visible;

            }

        }

        private void completedGoalView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            GridView gridView = (GridView)sender;
            archiveContextMenu.ShowAt(gridView, e.GetPosition(gridView));
            var goalInContext = ((FrameworkElement)e.OriginalSource).DataContext;
            goalInContextContainer = (goal.completedGoal)goalInContext;

        }

        private void MenuFlyout_Closed(object sender, object e)
        {
            completedGoalGridView.SelectedItem = null;
            completedGoalGridView.SelectionMode = ListViewSelectionMode.None;
        }


        private async void rootGrid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Let go to start creating a new goal."; // Sets custom UI text
            e.DragUIOverride.IsCaptionVisible = true; // Sets if the caption is visible
             await mainPivot.Fade(0.1f, 800).StartAsync();
          
        }

        private async void rootGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                var item = items[0] as StorageFile;
                
                var localFolder = ApplicationData.Current.LocalFolder;
                var localImageFolder = await localFolder.GetFolderAsync("ImageFolder");
                var newLocalImageFile = await localImageFolder.CreateFileAsync($"{item.Name}", CreationCollisionOption.ReplaceExisting);

                using (var stream = await item.OpenReadAsync())
                {
                    byte[] buffer = new byte[stream.Size];
                    var localBuffer = await stream.ReadAsync(buffer.AsBuffer(), (uint)stream.Size, InputStreamOptions.ReadAhead);
                    
                    using (var localStream = await newLocalImageFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await localStream.WriteAsync(localBuffer);
                        await localStream.FlushAsync();
                    }
                }

                App.NavService.NavigateTo(typeof(addNewGoalPage), newLocalImageFile);
                
            }
        }

        private async void rootGrid_DragLeave(object sender, DragEventArgs e)
        {
          bool dab = await mainPivot.Fade(1, 200).StartAsync();
        }

        private async void reviewDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri(string.Format("ms-windows-store:REVIEW?PFN={0}", Windows.ApplicationModel.Package.Current.Id.FamilyName)));
        }
    }


}

