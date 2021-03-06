﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using goalsFluentDesignRevamp.Model;
using System.Collections.ObjectModel;
using Windows.System;
using Microsoft.Services.Store.Engagement;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Composition;
using System.Numerics;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Windows.Storage;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.UI.Core;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI.ViewManagement;




// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace goalsFluentDesignRevamp
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class MainPage : Page
    {


        public ObservableCollection<goal> goalsToDisplay;
        public ObservableCollection<goal.completedGoal> completedGoalsToDisplay;
        public ObservableCollection<history> historyToDisplay = new ObservableCollection<history>();
        StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
        bool archiveEditingMode = false;
        goal.completedGoal goalInContextContainer;
        Compositor _compositor;
        SpriteVisual _hostSprite;
        SpriteVisual _blurSprite;
        public static goal persistedItem;
        public static bool firstTimeCloudOption = false;
        bool showReviewContentDialog = false;

        public MainPage()
        {

            this.InitializeComponent();
            loadHistory();
            loadGoals();
            checkIfDeviceHasFeedbackHub();
            //registerWindowActivationEvents();
            var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;

            if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Desktop" && Windows.Foundation.Metadata.ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateHostBackdropBrush"))
            {
                App.uiSettings.AdvancedEffectsEnabledChanged += UiSettings_AdvancedEffectsEnabledChangedAsync;

                if (App.uiSettings.AdvancedEffectsEnabled)
                {


                    changeToAcrylicPivotStyle();

                }
            }



        }


        private async void UiSettings_AdvancedEffectsEnabledChangedAsync(UISettings sender, object args)
        {
            if (sender.AdvancedEffectsEnabled)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
            //TODO: Apply Acrylic Accent
            applyAcrylicAccent(transparentArea);
                    changeToAcrylicPivotStyle();

                    if (isCurrentPageMainPage())
                    {
                        refreshPageInFlukyManner();

                    }








                });
            }
            else
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {

            //Because changing the pivot style is going to mess up the pivot header, this "Refreshes the page".
            //SIDE EFFECT: The user will always go to the goals pivot item if they disable transparency in their computer settings.
            changeToRegularPivotStyle();
                    if (isCurrentPageMainPage())
                    {
                        refreshPageInFlukyManner();

                    }


                });
            }
        }

        private bool isCurrentPageMainPage()
        {
            bool currentPageIsMainPage = false;
            if (Frame.CurrentSourcePageType == typeof(MainPage))
            {
                currentPageIsMainPage = true;
            }
            return currentPageIsMainPage;
        }

        private void refreshPageInFlukyManner()
        {
            Frame.Navigate(typeof(MainPage));
            int numOfFrames = Frame.BackStackDepth;
            Frame.BackStack.RemoveAt(numOfFrames - 1);
        }

        private void changeToRegularPivotStyle()
        {
            mainPivot.Style = bestPivotStyle;


        }

        private void changeToAcrylicPivotStyle()
        {
            mainPivot.Style = acrylicPivotStyle;

        }



        private void loadHistory()
        {
            if (history.listOfHistory.Count > 0)
            {

                List<history> orderedHistory = history.listOfHistory.Reverse().ToList();
                foreach (history historicalEvent in orderedHistory)
                {
                    historyToDisplay.Add(historicalEvent);

                }
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

            if ((string)e.Parameter == "addedOrUpdatedGoal")
            {
                showReviewContentDialog = true;


            }

            if ((string)e.Parameter == "carryOnFromCloud")
            {
                mainPivot.SelectedItem = syncPivotItem;
                MainPage.firstTimeCloudOption = true;
            }


            if ((string)e.Parameter == "goalNotFound")
            {
                showGoalNotFoundDialog();

            }

            var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;

            if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Desktop" && Windows.Foundation.Metadata.ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateHostBackdropBrush"))
            {
                if (App.uiSettings.AdvancedEffectsEnabled)
                {

                    applyAcrylicAccent(transparentArea);


                }

            }
            else
            {

            }



        }

        private async void showGoalNotFoundDialog()
        {
            ContentDialog goalNotFoundDialog = new ContentDialog
            {
                Title = "Sorry 😔",
                Content = "The goal you wanted to update could not be found or selected.",
                CloseButtonText = "OK",
                IsPrimaryButtonEnabled = false
            };

            await goalNotFoundDialog.ShowAsync();
        }

        private void applyBlurBackDrop(Panel panel)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _blurSprite = _compositor.CreateSpriteVisual();
            _blurSprite.Size = new Vector2((float)panel.ActualWidth, (float)panel.ActualHeight);
            ElementCompositionPreview.SetElementChildVisual(panel, _blurSprite);

            // Create an effect description 
            GaussianBlurEffect blurEffect = new GaussianBlurEffect()
            {
                Name = "Blur",
                BlurAmount = 2.0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced
            };

            blurEffect.Source = new CompositionEffectSourceParameter("source");

            CompositionEffectFactory blurEffectFactory = _compositor.CreateEffectFactory(blurEffect);
            CompositionEffectBrush blurBrush = blurEffectFactory.CreateBrush();

            // Create a BackdropBrush and bind it to the EffectSourceParameter “source” 
            CompositionBackdropBrush backdropBrush = _compositor.CreateBackdropBrush();
            blurBrush.SetSourceParameter("source", backdropBrush);

            _blurSprite.Brush = blurBrush;


        }

        private void applyAcrylicAccent(Panel panel)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _hostSprite = _compositor.CreateSpriteVisual();
            _hostSprite.Size = new Vector2((float)panel.ActualWidth, (float)panel.ActualHeight);
            ElementCompositionPreview.SetElementChildVisual(panel, _hostSprite);
            _hostSprite.Opacity = 1f;



            _hostSprite.Brush = _compositor.CreateHostBackdropBrush();

        }

        private async void showTheDialog()
        {
            //Release Version App Review/Feedback System
            if (App.localSettings.Values["askReviewsCounter"] == null)
            {
                App.localSettings.Values["askReviewsCounter"] = 1;
                reviewDialog.CloseButtonText = "Maybe Later";
                await reviewDialog.ShowAsync();
            }
            else if ((int)App.localSettings.Values["askReviewsCounter"] > 0)
            {
                int oldCounterValue = (int)App.localSettings.Values["askReviewsCounter"];
                int newCounterValue = oldCounterValue - 1;
                App.localSettings.Values["askReviewsCounter"] = newCounterValue;
            }
            else
            {
                App.localSettings.Values["askReviewsCounter"] = 1;
                reviewDialog.CloseButtonText = "Maybe Later";
                await reviewDialog.ShowAsync();
            }

            //Beta Version App Review/Feedback System
            //if (App.localSettings.Values["askReviewsCounter"] == null)
            //{
            //    App.localSettings.Values["askReviewsCounter"] = 2;
            //    reviewDialog.CloseButtonText = "Maybe Later";
            //    await reviewDialog.ShowAsync();
            //}
            //else if ((int)App.localSettings.Values["askReviewsCounter"] > 0)
            //{
            //    int oldCounterValue = (int)App.localSettings.Values["askReviewsCounter"];
            //    int newCounterValue = oldCounterValue - 1;
            //    App.localSettings.Values["askReviewsCounter"] = newCounterValue;
            //}
            //else
            //{
            //    App.localSettings.Values["askReviewsCounter"] = 2;
            //    reviewDialog.CloseButtonText = "Maybe Later";
            //    await reviewDialog.ShowAsync();
            //}

            //Website Review Version App Review/Feedback System (Empty loool)


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
            persistedItem = selectedGoal;





            ConnectedAnimation connectedAnimation = goalsGridView.PrepareConnectedAnimation("image", selectedGoal, "goalImage");
            ConnectedAnimationService.GetForCurrentView().DefaultDuration = new TimeSpan(0, 0, 0, 0, 500);

            //App.NavService.NavigateTo(typeof(selectedGoalPage), selectedGoal);
            Frame.Navigate(typeof(selectedGoalPage), selectedGoal, new SuppressNavigationTransitionInfo());

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
            logger.Log("Times Review button is clicked");
            App.localSettings.Values["stopAskingForReviews"] = 1;
            await Launcher.LaunchUriAsync(new Uri(string.Format("ms-windows-store:REVIEW?PFN={0}", Windows.ApplicationModel.Package.Current.Id.FamilyName)));
        }

        private async void feedbackDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            logger.Log("Times Feedback Button Clicked");
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
        }

        private void reviewDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            reviewDialog.Hide();
            reviewDialog.Closed += showFeedbackDialog;
        }

        private async void showFeedbackDialog(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            feedbackDialog.CloseButtonText = "I will later";
            await feedbackDialog.ShowAsync();
        }



        private async void goalsGridView_Loaded(object sender, RoutedEventArgs e)
        {
            goal item;
            if (persistedItem != null)
            {
                item = persistedItem;
                goalsGridView.ScrollIntoView(item);
                ConnectedAnimation animation =
              ConnectedAnimationService.GetForCurrentView().GetAnimation("returnImage");
                if (animation != null)
                {
                    await goalsGridView.TryStartConnectedAnimationAsync(
                        animation, item, "goalImage");
                }
            }
        }


        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavService.NavigateTo(typeof(settingsPage));
        }

        private void moreButton_Click(object sender, RoutedEventArgs e)
        {
            mainCommandBarFlyout.ShowAt(mainCommandBar);
        }

        private void transparentArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_hostSprite != null)
                _hostSprite.Size = e.NewSize.ToVector2();
        }

        private void whatsNewButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(whatsNewPage));
        }

        private void page_Loaded(object sender, RoutedEventArgs e)
        {
            if (showReviewContentDialog == true)
            {
                if ((int)App.localSettings.Values["stopAskingForReviews"] == 0)
                {
                    showTheDialog();
                }

            }
        }

        private async void cortanaButton_Click(object sender, RoutedEventArgs e)
        {

            await Launcher.LaunchUriAsync(new Uri("ms-cortana://"));
        }

        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavService.NavigateTo(typeof(helpPage));
        }
    }
}

