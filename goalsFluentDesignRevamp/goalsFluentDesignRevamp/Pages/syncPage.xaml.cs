using goalsFluentDesignRevamp.Model;
using goalsFluentDesignRevamp.Services;
using Microsoft.Toolkit.Uwp.Services.OneDrive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace goalsFluentDesignRevamp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class syncPage : Page
    {
        public syncPage()
        {
            this.InitializeComponent();
        }

        private async void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if (await onedrive.startOneDriveInstance())
            {
                hideConnectMenu();
                showSyncDashboard();
                beginSyncing();
            }
            else
            {
                displayFailureUI();
            }
        }

        private void displayFailureUI()
        {
            connectCaptionTextBlock.Text = "Sync Failed, check if you are connected to the internet and try again";
            connectButton.Content = "Try Again";
            connectImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/onedriveFailureLogo.png", UriKind.Absolute));
            
        }

        private void showErrorUI()
        {
            throw new NotImplementedException();
        }

        private void hideConnectMenu()
        {
            connectMenu.Visibility = Visibility.Collapsed;
        }

        private async void beginSyncing()
        {
            //TODO: Create a setting which stores the date/time of last sync. Also save the exact same data into a new text file in the cloud app folder.
            //Then before you even consider downloading/uploading data, you should check if the date/time of last sync matches. If it does then you're 
            //up date and save so many CPU Cycles!!!

            List<object> listOfOneDriveFolders = await onedrive.startSyncing();
            var rootFolder = (OneDriveStorageFolder)listOfOneDriveFolders[0];
            var imageFolder = (OneDriveStorageFolder)listOfOneDriveFolders[1];
            bool statusFileExists = await onedrive.checkForStatusFile(rootFolder);



            if (statusFileExists)
            {
                string statusFileContents = await onedrive.getStatusFileContents(rootFolder);
                string syncDataFromLocalContents = (string)App.localSettings.Values["lastTimeSynced"];

               
                long cloudSyncDateInTicks = long.Parse(statusFileContents);
                

                List<StorageFile> goalDataToSync = await goal.getGoalDataFilesReadyForSyncing();
                List<OneDriveStorageFile> cloudData = await onedrive.getCloudDataFiles(rootFolder);
                syncStatusTextBlock.Text = "Syncing Goal Data";

                bool deviceHasNewerDataThanCloud = await decideWhetherCloudOrDeviceHasNewerData(goalDataToSync, rootFolder, cloudSyncDateInTicks);
                if (deviceHasNewerDataThanCloud)
                {
                    uploadDataToCloud(rootFolder, imageFolder, goalDataToSync, cloudData);
                    recordLastTimeSynced(rootFolder);
                    hideProgressRing();
                }

                else
                {
                    //the "complete" variable must be there. Without it, the app will start skipping code for no reason :'(
                   bool complete = await downloadDataFromCloud(rootFolder, imageFolder, goalDataToSync, cloudData);
                    recordLastTimeSynced(rootFolder);
                    //goal.listOfGoals = await goal.loadGoals();
                    //goal.listOfCompletedGoals = await goal.loadCompletedGoals();
                    //history.listOfHistory = await history.loadHistory();
                    hideProgressRing();
                    App.NavService.NavigateTo(typeof(syncToDeviceCompleted));
                }

            }
            else
            {
                onedrive.CreateStatusFile(rootFolder);
                
                List<StorageFile> goalDataToSync = await goal.getGoalDataFilesReadyForSyncing();
                uploadDataToCloud(rootFolder, goalDataToSync, imageFolder);
                recordLastTimeSynced(rootFolder);
                hideProgressRing();
                
                
            }

        }

        private void showSuccessImage()
        {
            syncImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/onedriveSuccessLogo.png", UriKind.Absolute));
        }

        private async void uploadDataToCloud(OneDriveStorageFolder rootFolder, List<StorageFile> goalDataToSync, OneDriveStorageFolder imageFolder)
        {
           
           
            foreach (var dataFile in goalDataToSync)
            {
                using (var localStream = await dataFile.OpenReadAsync())
                {
                    showProgeressRing();
                    var fileCreated = await rootFolder.CreateFileAsync(dataFile.Name, CreationCollisionOption.ReplaceExisting, localStream);
                    
                }
                
            }

            List<StorageFile> imagesToSync = await goal.getImagesReadyForSyncing();
            syncStatusTextBlock.Text = "Syncing Goal Image Data";

            foreach (var image in imagesToSync)
            {
                using (var localStream = await image.OpenReadAsync())
                {
                    showProgeressRing();
                    var fileCreated = await imageFolder.UploadFileAsync(image.Name, localStream, CreationCollisionOption.ReplaceExisting, 320 * 3 * 1024);
                }

            }

          
            syncStatusTextBlock.Text = "Sync Complete!";
            showSuccessImage();
            hideProgressRing();
        }

        private void recordLastTimeSynced(OneDriveStorageFolder rootFolder)
        {
            DateTime lastTimeSynced = DateTime.Now;
            App.localSettings.Values["lastTimeSynced"] = lastTimeSynced.ToString();
            onedrive.writeCurrentTimeToStatusFile(lastTimeSynced, rootFolder);
        }

        private async Task<bool> downloadDataFromCloud(OneDriveStorageFolder rootFolder, OneDriveStorageFolder imageFolder, List<StorageFile> goalDataToSync, List<OneDriveStorageFile> cloudData)
        {
            //Outline: Download and replace all goal data and images. Update the imagePath of each goal in case the user changes user name
            //on another computer (Things happen). You'll create a relativeUri using the images you've downloaded. Then you save one last time and update
            var localFolder = ApplicationData.Current.LocalFolder;
            retrieveGoalDataFromCloud(localFolder, cloudData);
            syncStatusTextBlock.Text = "Removing old data from your current device";


            List<StorageFile> newImageData = new List<StorageFile>();
            bool imageFolderIsEmpty = await onedrive.checkIfImageFolderIsEmpty(imageFolder);
            if (!imageFolderIsEmpty)
            {

                newImageData = await retrievegoalImageDataFromCloud(imageFolder, localFolder);
            }
            updateImageUriForNewGoalData(newImageData, localFolder);
            syncStatusTextBlock.Text = "Sync Complete!";
            return true;
        }

        private async void updateImageUriForNewGoalData(List<StorageFile> newImageData, StorageFolder localFolder)
        {
            //TODO: Go through every image name and check listOfGoals and listOfCompletedGoals for the imageURI containing the same image name (with the minimal characters being used for max accuracy)
            //Then you update the URI so it's $"{localFolder}/{ImageFolder}/{associatedGoalImage}.{Image File Extenstion}" 
            //All images that do not get matched in listOfGoals or listOfCompletedGoals are deleted!


            if (newImageData.Count != 0)
            {
             goal.listOfGoals = await goal.loadGoals();
            goal.listOfCompletedGoals = await goal.loadCompletedGoals();


                foreach (var imageFile in newImageData)
                {
                    bool matched = false;
                    bool noGoalMatch = false;
                    bool noCompletedGoalMatch = false;
                    foreach (var goalItem in goal.listOfGoals)
                    {
                        if (goalItem.imagePath.Contains($"{imageFile.Name}"))
                        {
                            goalItem.imagePath = $"{localFolder.Path}\\ImageFolder\\{imageFile.Name}";
                            matched = true;
                        }
                        
                    }
                    if (matched == false)
                    {
                        noGoalMatch = true;
                    }


                    foreach (var completedGoalItem in goal.listOfCompletedGoals)
                    {
                        if (completedGoalItem.imagePath.Contains($"{imageFile.Name}"))
                        {
                            completedGoalItem.imagePath = $"{localFolder.Path}\\ImageFolder\\{imageFile.Name}";
                            matched = true;
                        }

                       
                    }
                    if (matched == false)
                    {
                        noCompletedGoalMatch = true;
                    }

                    if (noGoalMatch == true && noCompletedGoalMatch == true )
                    {
                        await imageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        //Following Code might cause code to crash
                        
                    }
                  
                }

            }
              goal.saveGoals();
            
        }

        private async Task<List<StorageFile>> retrievegoalImageDataFromCloud(OneDriveStorageFolder imageFolder, StorageFolder localFolder)
        {
            var localImageFolder = await localFolder.GetFolderAsync("ImageFolder");

            List<StorageFile> newGoalImageData = new List<StorageFile>();

            List <OneDriveStorageFile> cloudImageFiles = await imageFolder.GetFilesAsync();




            foreach (var cloudImageFile in cloudImageFiles)
            {
                using (var cloudFileStream = await cloudImageFile.OpenAsync())
                {
                    byte[] buffer = new byte[cloudFileStream.Size];
                    var localBuffer = await cloudFileStream.ReadAsync(buffer.AsBuffer(), (uint)cloudFileStream.Size, InputStreamOptions.ReadAhead);
                    var newGoalImageFile = await localImageFolder.CreateFileAsync($"{cloudImageFile.Name}", CreationCollisionOption.ReplaceExisting);
                    using (var localStream = await newGoalImageFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await localStream.WriteAsync(localBuffer);
                        await localStream.FlushAsync();
                    }
                    newGoalImageData.Add(newGoalImageFile);
                }
            }
            return newGoalImageData;
        }


        private async void retrieveGoalDataFromCloud(StorageFolder localFolder, List<OneDriveStorageFile> cloudData)
        {
            List<StorageFile> newGoalData = new List<StorageFile>();
            foreach (var cloudFile in cloudData)
            {
                using (var cloudFileStream = await cloudFile.OpenAsync())
                {
                    byte[] buffer = new byte[cloudFileStream.Size];
                    var localBuffer = await cloudFileStream.ReadAsync(buffer.AsBuffer(), (uint)cloudFileStream.Size, InputStreamOptions.ReadAhead);
                    var newGoalDataFile = await localFolder.CreateFileAsync($"{cloudFile.Name}", CreationCollisionOption.ReplaceExisting);
                    using (var localStream = await newGoalDataFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await localStream.WriteAsync(localBuffer);
                        await localStream.FlushAsync();
                    }
                    newGoalData.Add(newGoalDataFile);
                }

            }


        }

       



        private async void uploadDataToCloud(OneDriveStorageFolder rootFolder, OneDriveStorageFolder imageFolder, List<StorageFile> goalDataToSync, List<OneDriveStorageFile> cloudData)
        {
            syncStatusTextBlock.Text = "Replacing old data from cloud";

            foreach (var dataFile in goalDataToSync)
            {
                using (var localStream = await dataFile.OpenReadAsync())
                {
                    showProgeressRing();
                    var fileCreated = await rootFolder.CreateFileAsync(dataFile.Name, CreationCollisionOption.ReplaceExisting, localStream);
                }

            }

            List<StorageFile> imagesToSync = await goal.getImagesReadyForSyncing();
            syncStatusTextBlock.Text = "Syncing Goal Image Data";

            foreach (var image in imagesToSync)
            {
                using (var localStream = await image.OpenReadAsync())
                {
                    showProgeressRing();
                   var fileCreated = await imageFolder.UploadFileAsync(image.Name, localStream, CreationCollisionOption.ReplaceExisting, 320* 3*1024);
                }

            }
            syncStatusTextBlock.Text = "Sync Complete!";
            showSuccessImage();
            hideProgressRing();
        }


        private async Task<bool> decideWhetherCloudOrDeviceHasNewerData(List<StorageFile> goalDataToSync, OneDriveStorageFolder rootFolder, long cloudSyncDateInTicks)
        {
            bool isDeviceNewerThanCloud = false;

            List<OneDriveStorageFile> cloudData = await onedrive.getCloudDataFiles(rootFolder);



            for (int i = 0; i < 3; i++)
            {
                var dataFileProperties = await goalDataToSync[i].GetBasicPropertiesAsync();

                if (dataFileProperties.DateModified.Ticks > cloudSyncDateInTicks)
                {
                    isDeviceNewerThanCloud = true;
                }
            }


            return isDeviceNewerThanCloud;
        }



        private void hideProgressRing()
        {
            syncProgressRing.IsActive = false;
        }

        private void showProgeressRing()
        {
            syncProgressRing.IsActive = true;
        }

        private void showSyncDashboard()
        {
            syncDashboard.Visibility = Visibility.Visible;
        }
    }
}

