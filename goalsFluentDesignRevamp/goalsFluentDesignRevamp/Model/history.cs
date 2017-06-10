using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace goalsFluentDesignRevamp.Model
{
    public enum eventType { PositiveUpdate, NegativeUpdate, NewGoal, DeletedGoal, CompletedGoal };
    public class history
    {
        public DateTime dateOfEvent { get; set; }
        public string description { get; set; }
        public eventType eventTakenPlace { get; set; }
        public string title { get; set; }
        public static ObservableCollection<history> listOfHistory;




        public static void initializeHistory()
        {
            listOfHistory = new ObservableCollection<history>();
        }

        public static void makeHistory(string goalName, string Description, DateTime DateOfEvent, eventType EventTakenPlace)
        {

            if (listOfHistory == null)
            {
                history.initializeHistory();
            }
            history newHistory = new history
            {
                dateOfEvent = DateOfEvent,
                description = Description,
                eventTakenPlace = EventTakenPlace,
                title = decideOnTitle(goalName, EventTakenPlace)


            };


            listOfHistory.Add(newHistory);
        }

        private static string decideOnTitle(string goalName, eventType eventTakenPlace)
        {
            string titleToUse = "YEAH";



            if (eventTakenPlace == eventType.DeletedGoal)
            {
                titleToUse = $"Gave up on {goalName}";
            }

            if (eventTakenPlace == eventType.CompletedGoal)
            {
                titleToUse = "Goal Completed!";
            }
            if (eventTakenPlace == eventType.NegativeUpdate || eventTakenPlace == eventType.PositiveUpdate)
            {
                titleToUse = "Updated Progress";
            }
            if (eventTakenPlace == eventType.NewGoal)
            {
                titleToUse = "Created a New Goal!";
            }
            return titleToUse;

        }


        public async static void saveHistory()
        {
            bool fileLocked = true;
            //saves historys
             StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile newFile;


            newFile = await localFolder.CreateFileAsync("history.json", CreationCollisionOption.ReplaceExisting);
            var file = new FileInfo(newFile.Path);
            Stream jsonStream = await newFile.OpenStreamForWriteAsync();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ObservableCollection<history>));
            ser.WriteObject(jsonStream, listOfHistory);
            jsonStream.Flush();
            jsonStream.Dispose();

            do
            {


                FileStream stream = null;

                try
                {
                    stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    fileLocked = false;
                }
                catch (IOException)
                {
                    fileLocked = true;
                }

                stream.Flush();
                stream.Dispose();
                //file is not locked


            } while (fileLocked == true);
            fileLocked = true;
        }

        public async static Task<ObservableCollection<history>> loadHistory()
        {
            bool fileLocked = true;
             StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            ObservableCollection<history> savedHistory = new ObservableCollection<history>();

            StorageFile savedFile = await localFolder.GetFileAsync("history.json");
            var file = new FileInfo(savedFile.Path);

            Stream jsonStream = await savedFile.OpenStreamForReadAsync();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ObservableCollection<history>));
            savedHistory = (ObservableCollection<history>)ser.ReadObject(jsonStream);

            jsonStream.Flush();
            jsonStream.Dispose();

            do
            {


                FileStream stream = null;

                try
                {
                    stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    fileLocked = false;
                }
                catch (IOException)
                {
                    fileLocked = true;
                }
                stream.Flush();
                stream.Dispose();




            } while (fileLocked == true);
            fileLocked = true;
            return savedHistory;

        }
    }
}
