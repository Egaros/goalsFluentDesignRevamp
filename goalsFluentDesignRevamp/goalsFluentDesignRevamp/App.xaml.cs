using goalsFluentDesignRevamp.Model;
using Microsoft.Services.Store.Engagement;
using System;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.SpeechRecognition;

namespace goalsFluentDesignRevamp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static Navigation NavService { get; set; }
        public static Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        private bool newSaveFilesNeeded = false;
        public bool isTaskRegistered = false;
        private StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
        private bool newHistoryNeeded = false;
        public static MediaPlayer SFXSystem = new MediaPlayer { AudioCategory = MediaPlayerAudioCategory.Other, IsLoopingEnabled = false, Volume = 0.1 };
        public static MediaSource clickSFXSource = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/click.mp3"));
        public static MediaSource deleteClickSFXSource = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/deleteClick.mp3"));
        public static MediaSource selectClickSFXSource = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/selectClick.mp3"));
        public static MediaSource selectModeClickSFXSource = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/selectModeClick.mp3"));
        public static MediaSource cancelClickSFXSource = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/cancelClick.mp3"));
        public static MediaSource congratsSFXSource = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/congratulations.mp3"));
        public static UISettings uiSettings = new UISettings();
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {

            this.InitializeComponent();
            this.Suspending += OnSuspending;
            Application.Current.Resuming += new EventHandler<Object>(OnResuming);
            Application.Current.LeavingBackground += updateTitleBar; 
            createScreenOrientationEvenHandlers();
            createJumpList();
           

            checkIfYouCanLoadFiles();
            if (newSaveFilesNeeded == true)
            {
                //If new save files are needed, you can assume that this is the first time the app has been opened on a device
                //Or started from scratch so set last synced ticks to 0 here
                createSaveFiles();
                createImageFolder();
                localSettings.Values["lastTimeSynced"] = "Haven't synced with OneDrive yet.";
            }
            else
            {
                loadSaveFiles();
            }

            if (newHistoryNeeded == true)
            {
                createHistory();
            }
            else
            {
                loadHistory();
            }

            handleThemeSettings();

        }

      

        private void handleThemeSettings()
        {
            if (localSettings.Values["isThemeColorOverrideEnabled"] != null && localSettings.Values["themeColor"] != null)
            {

                int overrideEnabledValue = (int)localSettings.Values["isThemeColorOverrideEnabled"];



                if (overrideEnabledValue == 1)
                {
                    int themeColorValue = (int)App.localSettings.Values["themeColor"];

                    if (themeColorValue == 0)
                    {
                        Application.Current.RequestedTheme = ApplicationTheme.Dark;
                    }

                    else
                    {
                        Application.Current.RequestedTheme = ApplicationTheme.Light;
                    }


                }

            }


        }

        private async void createJumpList()
        {
            // Get the app's jump list.
            
            var jumpList = await Windows.UI.StartScreen.JumpList.LoadCurrentAsync();
            
            // Set the system to autogenerate a Frequent group for the app jump list.
            // Alternatively, this property could be set to JumpListSystemGroupKind.Recent to autogenerate a Recent group.
            jumpList.SystemGroupKind = Windows.UI.StartScreen.JumpListSystemGroupKind.Recent;
            var jumpListItems = jumpList.Items;
            // No changes were made to the jump list Items property, so any custom tasks and groups remain intact.
            await jumpList.SaveAsync();
        }

        private void updateTitleBar(object sender, LeavingBackgroundEventArgs e)
        {
            var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();

            var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;

            if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Desktop")
            {
                var accentColorBrush = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]).Color;
                ApplicationViewTitleBar formattableTitleBar = appView.TitleBar;
                formattableTitleBar.ButtonForegroundColor = accentColorBrush;
                CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar.ExtendViewIntoTitleBar = true;
                appView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);


            }

        }

        private async void createImageFolder()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            await localFolder.CreateFolderAsync("ImageFolder", CreationCollisionOption.OpenIfExists);
        }

        private void createScreenOrientationEvenHandlers()
        {
            Window.Current.SizeChanged += async (sender, args) =>
            {
                ApplicationView currentView = ApplicationView.GetForCurrentView();
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();


                    if (currentView.Orientation == ApplicationViewOrientation.Landscape)
                    {

                        await statusBar.HideAsync();
                    }


                    else if (currentView.Orientation == ApplicationViewOrientation.Portrait)
                    {
                        await statusBar.ShowAsync();
                    }
                }
            };
        }

        private async void loadHistory()
        {
            history.listOfHistory = await history.loadHistory();
        }

        private void createHistory()
        {
            history.initializeHistory();
            history.saveHistory();
        }

        public static BackgroundTaskRegistration RegisterBackgroundTask(
                                                string taskEntryPoint,
                                                string name,
                                                IBackgroundTrigger trigger,
                                                IBackgroundCondition condition)
        {

            // We'll add code to this function in subsequent steps.
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {

                if (cur.Value.Name == name)
                {
                    //
                    // The task is already registered.
                    //

                    return (BackgroundTaskRegistration)(cur.Value);
                }
            }

            // We'll register the task in the next step.

            var builder = new BackgroundTaskBuilder()
            {
                Name = name
            };

            // in-process background tasks don't set TaskEntryPoint
            if (taskEntryPoint != null && taskEntryPoint != String.Empty)
            {
                builder.TaskEntryPoint = taskEntryPoint;
            }
            builder.SetTrigger(trigger);

            if (condition != null)
            {
                builder.AddCondition(condition);
            }

            BackgroundTaskRegistration task = builder.Register();
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);

            return task;
        }

        private static void OnCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {

        }

        protected async override void OnActivated(IActivatedEventArgs e)
        {



            


         


            // Get the root frame
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;
                //rootFrame.Navigated += OnNavigated;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;


                StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();
                engagementManager.RegisterNotificationChannelAsync();



                await BackgroundExecutionManager.RequestAccessAsync();
                unregisterBackgroundTasks();
                var backgroundTask = RegisterBackgroundTask("tasks.Class1", "Class1", new TimeTrigger(1440, false), new SystemCondition(SystemConditionType.UserNotPresent));



                var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {

                    var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                    statusBar.BackgroundOpacity = 1;
                    statusBar.ForegroundColor = ((SolidColorBrush)Application.Current.Resources["SystemControlForegroundAccentBrush"]).Color;
                    statusBar.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["SystemControlBackgroundChromeMediumLowBrush"]).Color;
                }

                var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;

                if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Desktop")
                {
                    var accentColorBrush = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]).Color;
                    ApplicationViewTitleBar formattableTitleBar = appView.TitleBar;
                    formattableTitleBar.ButtonBackgroundColor = Colors.Transparent;
                    formattableTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                    formattableTitleBar.ButtonForegroundColor = accentColorBrush;
                    CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                    coreTitleBar.ExtendViewIntoTitleBar = true;
                    appView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);


                }

                appView.SetPreferredMinSize(new Size(400, 400));


            }



            if (rootFrame.Content == null)
            {
                NavService = new Navigation(ref rootFrame);
              

            }


            // Handle toast activation
            if (e is ToastNotificationActivatedEventArgs)
            {
                var toastActivationArgs = e as ToastNotificationActivatedEventArgs;

                // Parse the query string
                string args = toastActivationArgs.Argument;

                // See what action is being requested 
                if (args == "comeBack" || args == "Yes")
                {
                    logger.Log("Launched app from encouraging toast");
                    App.NavService.NavigateTo(typeof(MainPage), "comeBack");
                }
            }




                // TODO: Handle other types of activation

                if (e.Kind == ActivationKind.VoiceCommand)
                {
                    // Event args can represent many different activation types. 
                    // Cast it so we can get the parameters we care about out.
                    var commandArgs = e as VoiceCommandActivatedEventArgs;

                    Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult = commandArgs.Result;

                    // Get the name of the voice command and the text spoken. 
                    // See VoiceCommands.xml for supported voice commands.
                    string voiceCommandName = speechRecognitionResult.RulePath[0];
                    string textSpoken = speechRecognitionResult.Text;

                    // commandMode indicates whether the command was entered using speech or text.
                    // Apps should respect text mode by providing silent (text) feedback.
                    string commandMode = this.SemanticInterpretation("commandMode", speechRecognitionResult);

                    switch (voiceCommandName)
                    {
                        case "createNewGoal":
                        const string nullValue = "(null)";
                        string goalName = this.SemanticInterpretation("goalName", speechRecognitionResult);
                        string target = this.SemanticInterpretation("target", speechRecognitionResult);
                        string[] goalDetails = new string[] { goalName, target };

                        
                       

                        
                       
                        
                        // Create a navigation command object to pass to the page. 
                        NavService.NavigateTo(typeof(addNewGoalPage), goalDetails);
                        if (rootFrame.BackStackDepth > 0)
                        {
                            if (rootFrame.BackStack.Last().SourcePageType != typeof(MainPage))
                            {
                                int backStackSize = rootFrame.BackStackDepth;
                                rootFrame.BackStack.RemoveAt(backStackSize - 1);
                            }
                        }

                            break;

                    case "showGoalInProgress":

                         goalName = this.SemanticInterpretation("goalInProgress", speechRecognitionResult);

                        try
                        {
                            
                            int itemCount = goal.listOfGoals.Where(item => item.name == goalName).Count();
                            if (itemCount > 0)
                            {
                                var goalInContext = goal.listOfGoals.Where(item => item.name == goalName).First();
                                NavService.NavigateTo(typeof(selectedGoalPage), goalInContext);
                            }
                        }
                        catch (Exception)
                        {

                            NavService.NavigateTo(typeof(MainPage), "cortanaFailed");
                        }
                       

                        break;

                    default:
                            NavService.NavigateTo(typeof(MainPage), "cortanaFailed");
                            break;
                    }

                }

// Ensure the current window is active
            Window.Current.Activate();
            }
                

            
            



        

        private string SemanticInterpretation(string interpretationKey, SpeechRecognitionResult speechRecognitionResult)
        {
            string valueFromKey;
            try
            {
                valueFromKey = speechRecognitionResult.SemanticInterpretation.Properties[interpretationKey].FirstOrDefault();
            }
            catch (Exception)
            {

                valueFromKey = "(null)";
            }
             
            return valueFromKey;
        }


        private async void requestAccessForBackgroundTasksOnSpecialActivation()
        {
           await BackgroundExecutionManager.RequestAccessAsync();
        }

        private async void loadSaveFiles()
        {
            goal.listOfGoals = await goal.loadGoals();
            goal.listOfCompletedGoals = await goal.loadCompletedGoals();

        }

        private void createSaveFiles()
        {
            goal.initializeGoalClass();
            goal.saveGoals();

        }

        private void checkIfYouCanLoadFiles()
        {
            bool loadFailed = false;
            bool loadFailed2 = false;
            bool loadFailed3 = false;
            loadFailed = attemptLoadGoals();
            loadFailed2 = attemptLoadCompletedGoals();
            loadFailed3 = attemptToLoadHistory();
            if (loadFailed == true || loadFailed2 == true)
            {
                newSaveFilesNeeded = true;
            }

            if (loadFailed3 == true)
            {
                newHistoryNeeded = true;
            }
        }

        private bool attemptToLoadHistory()
        {
            bool failure = false;
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string localPath = localFolder.Path;
            string path = $"{localPath}\\history.json";
            FileInfo file = new FileInfo(path);
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                failure = false;
                stream.Flush();
                stream.Dispose();
            }
            catch (IOException)
            {
                failure = true;
            }

            return failure;
        }

        private bool attemptLoadCompletedGoals()
        {
            bool failure = false;
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string localPath = localFolder.Path;
            string path = $"{localPath}\\noGolaso.json";
            FileInfo file = new FileInfo(path);

            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                failure = false;
                stream.Flush();
                stream.Dispose();
            }
            catch (IOException)
            {
                failure = true;
            }

            return failure;
        }

        private bool attemptLoadGoals()
        {
            bool failure = false;
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            string localPath = localFolder.Path;
            string path = $"{localPath}\\golaso.json";
            FileInfo file = new FileInfo(path);

            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                failure = false;
                stream.Flush();
                stream.Dispose();
            }
            catch (IOException)
            {
                failure = true;
            }

            return failure;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            InstallCommandDefinitionsFromStorageFileAsync();
            StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();
             engagementManager.RegisterNotificationChannelAsync();



            await BackgroundExecutionManager.RequestAccessAsync();
            unregisterBackgroundTasks();
            var backgroundTask = RegisterBackgroundTask("tasks.Class1", "Class1", new TimeTrigger(1440, false), new SystemCondition(SystemConditionType.UserNotPresent));



            var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {

                var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                statusBar.BackgroundOpacity = 1;
                statusBar.ForegroundColor = ((SolidColorBrush)Application.Current.Resources["SystemControlForegroundAccentBrush"]).Color;
                statusBar.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["SystemControlBackgroundChromeMediumLowBrush"]).Color;
            }

            var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;

            if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Desktop")
            {
               var accentColorBrush = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]).Color;
                ApplicationViewTitleBar formattableTitleBar = appView.TitleBar;
                formattableTitleBar.ButtonBackgroundColor = Colors.Transparent;
                formattableTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                formattableTitleBar.ButtonForegroundColor = accentColorBrush;
                CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar.ExtendViewIntoTitleBar = true;
                appView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);


            }

            appView.SetPreferredMinSize(new Size(400, 400));



            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;
                //rootFrame.Navigated += OnNavigated;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;

                // Register a handler for BackRequested events and set the
                // visibility of the Back button
                //SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

                //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                //    rootFrame.CanGoBack ?
                //    AppViewBackButtonVisibility.Visible :
                //    AppViewBackButtonVisibility.Collapsed;
            }

            if (e.PrelaunchActivated == false)
            {
                var activationKind = e.Kind;
                

                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    PackageVersion pv = Package.Current.Id.Version;
                    object currentAppVersion = localSettings.Values["currentAppVersion"];
                    NavService = new Navigation(ref rootFrame);
                    string applicationVersion = $"{pv.Major}.{pv.Minor}.{pv.Build}.{pv.Revision}";
                    prepareForTheCheckAndAskForReviewsAlgorithm(); 
                    if (currentAppVersion == null)
                    {
                        //Uncomment currentAppVersion change when you finish with testing onBoardingPage
                        //localSettings.Values["currentAppVersion"] = applicationVersion;
                        //When you finish the onBoardingPage, show it instead of the whatsNewPage
                        App.NavService.NavigateTo(typeof(onBoardingPage), applicationVersion);

                    }
                    else if (currentAppVersion.ToString() != applicationVersion)
                    {
                        logger.Log($"Users updated to app version: {applicationVersion}");
                        localSettings.Values["currentAppVersion"] = applicationVersion;
                        //Bring back the whatsNewPage for users who update at the end of your exams when you are officially back!
                        //rootFrame.Navigate(typeof(whatsNewPage));
                        rootFrame.Navigate(typeof(whatsNewPage), e.Arguments);

                    }
                    else
                    {
                       
                        if (!string.IsNullOrEmpty(e.Arguments))
                        {
                            string tileID = e.Arguments;
                            navigateBasedOnTileID(tileID, rootFrame);
                        }

                        else if (e.TileId != "")
                        {
                            if (e.TileId != "App")
                            {
                            navigateBasedOnTileID(e.TileId, rootFrame);

                            }
                            else
                            {
                                App.NavService.NavigateTo(typeof(MainPage), e.Arguments);
                            }
                        }
                        else
                        {
                            App.NavService.NavigateTo(typeof(MainPage), e.Arguments);
                        }
                    }

                }

                else if (!string.IsNullOrEmpty(e.Arguments) && (string)e.Arguments == "Args")
                {
                    string tileID = e.Arguments;
                    navigateBasedOnTileID(tileID, rootFrame);
                }
                else
                {
                    if (e.TileId != "")
                    {

                        if (e.TileId != "App")
                        {
                            navigateBasedOnTileID(e.TileId, rootFrame);

                        }
                        else
                        {
                            App.NavService.NavigateTo(typeof(MainPage), e.Arguments);
                        }
                    }
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }

        }

        private async void InstallCommandDefinitionsFromStorageFileAsync()
        {
            try
            {
                // Install the main VCD. 
                StorageFile vcdStorageFile =
                  await Package.Current.InstalledLocation.GetFileAsync(
                    @"cortanaCommands.xml");

                await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.
                  InstallCommandDefinitionsFromStorageFileAsync(vcdStorageFile);
                speech.updatePhraseList();
                // TODO: Update phrase list. (If required)
               
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Installing Voice Commands Failed: " + ex.ToString());
            }
        }

        private void navigateBasedOnTileID(string tileId, Frame rootFrame)
        {
            var listOfGoals = goal.listOfGoals;
            var listOfNeededGoals = listOfGoals.Where(p => p.tileID == tileId);

            if (listOfNeededGoals != null && listOfNeededGoals.Count() > 0)
            {
                var goalSelectedByUser = (goal)listOfNeededGoals.First();
                logger.Log("App launched from pinned goal tile");
                App.NavService.NavigateTo(typeof(selectedGoalPage), goalSelectedByUser);
            }

            
            else
            {
                App.NavService.NavigateTo(typeof(MainPage), "goalNotFound");

            }
        }

        private void prepareForTheCheckAndAskForReviewsAlgorithm()
        {
            if (App.localSettings.Values["stopAskingForReviews"] == null)
            {
                App.localSettings.Values["stopAskingForReviews"] = 0;
            }
        }

        private void unregisterBackgroundTasks()
        {
            var taskList = BackgroundTaskRegistration.AllTasks.Values.Where((i => i.Name.Equals("Class1"))).ToList();
            foreach (var task in taskList)
            {
                if (task != null)
                {
                    task.Unregister(true);
                }

            }

        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            // Each time a navigation event occurs, update the Back button's visibility
            //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
            //    ((Frame)sender).CanGoBack ?
            //    AppViewBackButtonVisibility.Visible :
            //    AppViewBackButtonVisibility.Collapsed;
        }



        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }


        private void OnResuming(object sender, object e)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() =>
            {
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar")) { 
                    var statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                    statusBar.ForegroundColor = ((SolidColorBrush)Application.Current.Resources["SystemControlForegroundAccentBrush"]).Color;
                    statusBar.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["SystemControlBackgroundChromeMediumLowBrush"]).Color;
                }

               
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

           

        }


    }
}



