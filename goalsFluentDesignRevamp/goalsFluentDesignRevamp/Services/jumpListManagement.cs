using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using goalsFluentDesignRevamp.Model;
using Windows.UI.StartScreen;

namespace goalsFluentDesignRevamp.Services
{
    class jumpListManagement
    {
        public async static void UpdateJumpList(ObservableCollection<goal> listOfGoals)
        {

            var jumpList = await JumpList.LoadCurrentAsync();

            if (jumpList.Items != null && jumpList.Items.Count > 0)
            {
                removeJumpListItemsThatAreNotInListOfGoals(listOfGoals, jumpList);
            }

            foreach (var goal in listOfGoals)
            {


              
            var item = JumpListItem.CreateWithArguments(goal.tileID, goal.name);
            item.Description = $"Quickly update your progress on for {goal.name}";
            item.GroupName = "Shortcuts To Goals In Progress";
            

            jumpList.Items.Add(item);

            }

           
            jumpList.SystemGroupKind = JumpListSystemGroupKind.Recent;

            await jumpList.SaveAsync();

        }

        private static void removeJumpListItemsThatAreNotInListOfGoals(ObservableCollection<goal> listOfGoals, JumpList jumpList)
        {
            List<JumpListItem> jumpListItemsToBeDeleted = new List<JumpListItem>();
            foreach (var item in jumpList.Items)
            {
                bool isJumpListItemInListOfGoals = false;

                if (listOfGoals.Where(p => p.name == item.DisplayName).Count() > 0)
                {
                    isJumpListItemInListOfGoals = true;
                }
                if (isJumpListItemInListOfGoals == false)
                {
                    jumpListItemsToBeDeleted.Add(item);
                }
            }
            foreach (var item in jumpListItemsToBeDeleted)
            {
                jumpList.Items.Remove(item);
            }
        }
    }
}
