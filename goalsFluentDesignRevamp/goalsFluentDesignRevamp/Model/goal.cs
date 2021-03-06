﻿using goalsFluentDesignRevamp.Services;
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
    
    public class goal
    {
        public string name { get; set; }
        public decimal target { get; set; }
        public string description { get; set; }
        public string imagePath { get; set; }
        public string targetToDisplay { get; set; }
        public decimal targetReached { get; set; }
        public string progress { get; set; }
        public string tileID { get; set; }
        public DateTime startTime { get; set; } = DateTime.Now;
        public DateTime endTime { get; set; }
        public string unitsOfTimeRemaining { get; set; } = "N/A";
        public class completedGoal : goal
        {
            public DateTime dateOfCompletion { get; set; }
        }

        public static ObservableCollection<goal> listOfGoals;
        public static ObservableCollection<completedGoal> listOfCompletedGoals;
        public static DateTime noTimeLimitDate = new DateTime(1, 1, 1);

        public static void initializeGoalClass()
        {

            listOfGoals = new ObservableCollection<goal>();
            listOfCompletedGoals = new ObservableCollection<completedGoal>();


        }

        public static void saveGoals()
        {
             saveIncompleteGoals();
             saveCompleteGoals();
            jumpListManagement.UpdateJumpList(listOfGoals);
            speech.updatePhraseList();
            //saves completedGoals



        }

        private static async void saveCompleteGoals()
        {
            bool fileLocked = true;
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile newFile;
            newFile = await localFolder.CreateFileAsync("noGolaso.json", CreationCollisionOption.ReplaceExisting);
            var file = new FileInfo(newFile.Path);
            Stream jsonStream = await newFile.OpenStreamForWriteAsync();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ObservableCollection<completedGoal>));
            ser.WriteObject(jsonStream, listOfCompletedGoals);


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
           
        }

        public async static Task<List<StorageFile>> getGoalDataFilesReadyForSyncing()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var savedGoals = await localFolder.GetFileAsync("golaso.json");
            var savedCompletedGoals = await localFolder.GetFileAsync("noGolaso.json");
            var savedHistory = await localFolder.GetFileAsync("history.json");

            
            return new List<StorageFile> { savedGoals, savedCompletedGoals, savedHistory };
        }

        private static async void saveIncompleteGoals()
        {
            bool fileLocked = true;
            //saves goals
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile newFile;


            newFile = await localFolder.CreateFileAsync("golaso.json", CreationCollisionOption.ReplaceExisting);
            var file = new FileInfo(newFile.Path);
            Stream jsonStream = await newFile.OpenStreamForWriteAsync();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ObservableCollection<goal>));
            ser.WriteObject(jsonStream, listOfGoals);
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

        public async static Task<List<StorageFile>> getImagesReadyForSyncing()
        {

            var imageFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("ImageFolder");
           var  imagesUsed = await imageFolder.GetFilesAsync();

            List<StorageFile> listOfImagesToSync = new List<StorageFile>();
            foreach (var  file in imagesUsed)
            {
                listOfImagesToSync.Add(file);
            }
            return listOfImagesToSync;
        }

        public static async Task<ObservableCollection<goal>> loadGoals()
        {
            bool fileLocked = true;
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            ObservableCollection<goal> savedListOfGoals = new ObservableCollection<goal>();

            StorageFile savedFile = await localFolder.GetFileAsync("golaso.json");
            var file = new FileInfo(savedFile.Path);

            Stream jsonStream = await savedFile.OpenStreamForReadAsync();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ObservableCollection<goal>));
            savedListOfGoals = (ObservableCollection<goal>)ser.ReadObject(jsonStream);

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
            return savedListOfGoals;


        }

        public static async Task<ObservableCollection<completedGoal>> loadCompletedGoals()
        {
            bool fileLocked = true;
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            ObservableCollection<completedGoal> savedListOfCompletedGoals = new ObservableCollection<completedGoal>();

            StorageFile savedFile = await localFolder.GetFileAsync("noGolaso.json");
            var file = new FileInfo(savedFile.Path);
            Stream jsonStream = await savedFile.OpenStreamForReadAsync();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ObservableCollection<completedGoal>));
            savedListOfCompletedGoals = (ObservableCollection<completedGoal>)ser.ReadObject(jsonStream);

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
            return savedListOfCompletedGoals;
        }

        public static void addNewGoal(string Name, decimal Target, string Description, string ImagePath, DateTime endTime)
        {
            goal goalToSave = new goal();
            goalToSave.name = Name;
            goalToSave.target = Target;
            String TargetToDisplay = string.Format("Target: {0:C}", Target);
            goalToSave.targetToDisplay = TargetToDisplay;
            goalToSave.description = Description;
            goalToSave.targetReached = 0;
            if (ImagePath != null)
            {
                goalToSave.imagePath = ImagePath;
            }
            else
            {
                goalToSave.imagePath = "ms-appx:///Assets/noImage.png";
            }


            goalToSave.progress = "Progress: 0%";
            goalToSave.tileID = generateUniqueID();

            if (endTime != noTimeLimitDate )
            {
                goalToSave.endTime = endTime;
                goalToSave.unitsOfTimeRemaining =  determineTimeLeft(goalToSave);
            }

            

            listOfGoals.Add(goalToSave);

        }

        public static string determineTimeLeft(goal goalItem)
        {
            int calculation;
            int remainder;
            bool yearsMatch = false;
            bool monthsMatch = false;
            bool daysMatch = false;

            calculation = goalItem.endTime.Year - DateTime.Now.Year;
            if (calculation <= 0)
            {
                yearsMatch = true;
                
            }

            if (yearsMatch == true)
            {
                calculation = goalItem.endTime.Month - DateTime.Now.Month;
                if (calculation <=0)
                {
                    monthsMatch = true;
                }
            }

            if (monthsMatch == true)
            {
                calculation = goalItem.endTime.Day - DateTime.Now.Day;

                if (calculation <=0)
                {
                    daysMatch = true;
                }
            }

            string timeLeftDescription = "";
            if (daysMatch == true)
            {
                timeLeftDescription = "No more time left.";
            }
            else
            {
                remainder = calculation;
               timeLeftDescription = determineDescriptionToReturn(timeLeftDescription, yearsMatch, monthsMatch,remainder);

                
            }


            return timeLeftDescription;

        }

        private static string determineDescriptionToReturn(string timeLeftDescription, bool yearsMatch, bool monthsMatch, int remainder)
        {
            if (yearsMatch == false)
            {
                timeLeftDescription = $"{remainder} year(s) left.";
            }
            if (yearsMatch == true && monthsMatch == false)
            {
                timeLeftDescription = $"{remainder} month(s) left.";
            }
            if (yearsMatch == true && monthsMatch == true)
            {
                timeLeftDescription = $"{remainder} day(s) left.";
            }

            return timeLeftDescription;
        }

        public static string generateUniqueID()
        {
            Random rnd = new Random();
            string generatedID = string.Empty;
            bool isUnique = false;
            do
            {

                for (int i = 0; i < 7; i++)
                {
                    int numberToAdd = rnd.Next(0, 9);
                    generatedID += numberToAdd.ToString();
                }
                var possibleGoalWithSameId = goal.listOfGoals.FirstOrDefault(p => p.tileID == generatedID);
                if (possibleGoalWithSameId == null)
                {
                    isUnique = true;
                }

            } while (isUnique == false);
            return generatedID;
        }

        public static void makeCompletedGoal(goal achievedGoal, DateTime DateOfCompletion)
        {
            completedGoal completeGoalToSave = new completedGoal();
            completeGoalToSave.name = achievedGoal.name;
            completeGoalToSave.target = achievedGoal.target;
            completeGoalToSave.imagePath = achievedGoal.imagePath;
            completeGoalToSave.description = achievedGoal.description;
            completeGoalToSave.targetReached = achievedGoal.targetReached;
            completeGoalToSave.dateOfCompletion = DateOfCompletion;
            completeGoalToSave.progress = "Progress: 100%";
            completeGoalToSave.targetToDisplay = achievedGoal.targetToDisplay;
            List<goal> goalToRemove = listOfGoals.Where(p => p.name == achievedGoal.name).ToList();
            listOfGoals.Remove(goalToRemove[0]);
            listOfCompletedGoals.Add(completeGoalToSave);

        }

        public static void replaceGoal(goal changedGoal)
        {
            List<goal> goalContainer = goal.listOfGoals.Where(p => p.name == changedGoal.name).ToList();
            goal goalToReplace = goalContainer[0];
            int index = goal.listOfGoals.IndexOf(goalToReplace);
            if (index != -1)
            {
                goal.listOfGoals[index] = changedGoal;
            }


        }

    }
}
