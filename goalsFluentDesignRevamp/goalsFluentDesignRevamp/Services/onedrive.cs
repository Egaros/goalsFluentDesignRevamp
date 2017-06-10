using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Services;
using Microsoft.Toolkit.Uwp.Services.OneDrive;
using static Microsoft.Toolkit.Uwp.Services.OneDrive.OneDriveEnums;
using goalsFluentDesignRevamp.Model;
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace goalsFluentDesignRevamp.Services
{
    class onedrive
    {
        public string fileBeingsynced ="";
        public async static Task<bool> startOneDriveInstance()
        {
            // OneDriveService works for OneDrive Consumer as well as OneDrive For Business (Office 365)
            //
            // Authentication 
            // In order to use the OneDriveService you need to authenticate the user and get an access token

            // OneDrive consumer you have two options:
            // 1) If the user is connected to a Windows session with a Microsoft Account, the service is able to silently get an access token.
            //    For that you need to associate your application to the Store (Project->Store->Associate App With Store...)
            // 2) Or you have to register your app
            //  - go to https://dev.onedrive.com/app-registration.htm
            //  - When prompted, sign in with your Microsoft Account credentials.
            //  - Scroll to the bottom of the page (Live (SDK)), and click Add App
            //  - Enter your app's name and click Create application.
            //  - Copy the Application ID
            //  - Add a platform and select mobile application
            //  - Save

            // OneDrive For Business you need to register your app from the Azure Management Portal
            // For more information to manualy register your app see go to the following article
            // https://docs.microsoft.com/en-US/azure/active-directory/develop/active-directory-authentication-scenarios#basics-of-registering-an-application-in-azure-ad
            // When registering your application don't forget to add the Office 365 Sharepoint Online application with the "Read and Write user Files" permissions. You should set your Redirect URI to "urn:ietf:wg:oauth:2.0:oob". You may also need to add the following capabilities to your Package.appxmanifest: privateNetworkClientServer; enterpriseAuthentication

            // First get the root of your OneDrive
            // By default the service silently connects the current Windows user if Windows is associated with a Microsoft Account


            //var folder = await OneDriveService.Instance.RootFolderAsync();
            //rootFolder = folder;

            {

                
                // if Windows is not associated with a Microsoft Account, you need
                // 1) Initialize the service using an authentication provider AccountProviderType.Msa or AccountProviderType.Adal
                OneDriveService.Instance.Initialize("00000000401DA758", AccountProviderType.OnlineId, OneDriveScopes.OfflineAccess | OneDriveScopes.ReadWrite);
                // 2) Sign in
                if (!await OneDriveService.Instance.LoginAsync())
                {
                    return false;
                    throw new Exception("Unable to sign in");
                }
                else
                {
                    return true;
                }
            }

        }

        public async static Task<string> getStatusFileContents(OneDriveStorageFolder rootFolder)
        {
            var fileToReadTextFrom = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("statusContent.txt", CreationCollisionOption.ReplaceExisting);

            var statusFile = await rootFolder.GetFileAsync("status.txt");
            
            using (var fileStream = await statusFile.OpenAsync())
            {
                byte[] buffer = new byte[fileStream.Size];
                var localBuffer = await fileStream.ReadAsync(buffer.AsBuffer(), (uint)fileStream.Size, InputStreamOptions.ReadAhead);
                using (var localStream = await fileToReadTextFrom.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await localStream.WriteAsync(localBuffer);
                    await localStream.FlushAsync();
                }
            }

            string statusFileContent = System.IO.File.ReadAllText(fileToReadTextFrom.Path);
            await fileToReadTextFrom.DeleteAsync();
            return statusFileContent;
        }

        public async static void writeCurrentTimeToStatusFile(DateTime lastTimeSynced, OneDriveStorageFolder rootFolder)
        {
            //Gonna make the status file in tempoary folder, write lastTimeSynced in there then replace the one in the cloud with the one created in temporay folder.
            var fileToUpload = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("status.txt", CreationCollisionOption.ReplaceExisting);
            long dateAsTicks = lastTimeSynced.Ticks;
            string dateAsTicksString = dateAsTicks.ToString();
            System.IO.File.WriteAllText(fileToUpload.Path, dateAsTicksString);

           
            if (fileToUpload != null)
            {
                using (var localStream = await fileToUpload.OpenReadAsync())
                {
                    var fileCreated = await rootFolder.CreateFileAsync(fileToUpload.Name, CreationCollisionOption.ReplaceExisting, localStream);
                }
            }

            await fileToUpload.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        public async static Task<List<OneDriveStorageFile>> CreateDataFilesForSyncing(OneDriveStorageFolder rootFolder)
        {
            var cloudGoalData = await rootFolder.CreateFileAsync("golaso.json");
            var cloudCompletedGoalData = await rootFolder.CreateFileAsync("noGolaso.json");
            var cloudHistoryData = await rootFolder.CreateFileAsync("history.json");

                bool areFilesUploaded = false;
            do
            {

                long uploadStatus = await rootFolder.GetUploadStatusAsync();
                if (uploadStatus == 100)
                {
                    areFilesUploaded = true;
                }
               


            } while (areFilesUploaded == false);


            return new List<OneDriveStorageFile> { cloudGoalData, cloudCompletedGoalData, cloudHistoryData };

        }

        public async static Task<List<object>> startSyncing()
        {
            var rootFolder = await OneDriveService.Instance.AppRootFolderAsync();
            var imageFolder = await rootFolder.CreateFolderAsync("ImageFolder",CreationCollisionOption.OpenIfExists);

            return new List<object> { rootFolder, imageFolder };
        }

        public async static void CreateStatusFile(OneDriveStorageFolder rootFolder)
        {
           await rootFolder.CreateFileAsync("status.txt", CreationCollisionOption.ReplaceExisting);
        }

        public async static Task<bool> checkForStatusFile(OneDriveStorageFolder rootFolder)
        {
            bool statusFileExists = false;
            try
            {
              await  rootFolder.GetFileAsync("status.txt");
                statusFileExists = true;
            }

            catch
            {

            }
            return statusFileExists;
        }

        public async static Task<List<OneDriveStorageFile>> getCloudDataFiles(OneDriveStorageFolder rootFolder)
        {
            var cloudGoalData = await rootFolder.GetFileAsync("golaso.json");
            var cloudCompletedGoalData = await rootFolder.GetFileAsync("noGolaso.json");
            var cloudHistoryData = await rootFolder.GetFileAsync("history.json");
            return new List<OneDriveStorageFile> { cloudGoalData, cloudCompletedGoalData, cloudHistoryData};
        }

        public static bool checkIfImageFolderIsEmpty(OneDriveStorageFolder imageFolder)
        {
            bool isImageFolderEmpty = true;
            if (imageFolder.OneDriveItem.Folder.ChildCount != 0)
            {
                isImageFolderEmpty = false;
            }

            return isImageFolderEmpty;
        }
    }
}
