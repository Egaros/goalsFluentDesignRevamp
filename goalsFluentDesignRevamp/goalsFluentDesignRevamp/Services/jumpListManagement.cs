using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
                
                clearJumpList(jumpList);
            }

            foreach (var goal in listOfGoals)
            {


              
            var item = JumpListItem.CreateWithArguments(goal.tileID, goal.name);
            item.Description = $"Quickly update your progress on for {goal.name}";
            item.GroupName = "Shortcuts To Goals In Progress";

                if (isItemInJumpList(item,jumpList) == false)
                {
                    jumpList.Items.Add(item);

                }
            

            }

           
            jumpList.SystemGroupKind = JumpListSystemGroupKind.Recent;

            await jumpList.SaveAsync();

        }

        private static void clearJumpList(JumpList jumpList)
        {
            jumpList.Items.Clear();
        }

        private static bool isItemInJumpList(JumpListItem item, JumpList jumpList)
        {
            bool itemIsInJumplist = false;
            if (jumpList.Items.Contains(item))
            {
                itemIsInJumplist = true;
            }
            return itemIsInJumplist;
        }

    }
}
