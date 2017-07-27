using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;
namespace goalsFluentDesignRevamp.Model
{

    public class speech
    {
        
        public async static void updatePhraseList()
        {
            List<string> goalNames = getGoalNames();
            //repeat FOREACH commandset
          
             List<VoiceCommandDefinition> voiceCommandDefinitions = VoiceCommandDefinitionManager.InstalledCommandDefinitions.Values.ToList();
            foreach (var voiceCommandDefinition in voiceCommandDefinitions)
            {
                await voiceCommandDefinition.SetPhraseListAsync("goalInProgress", goalNames);
            }
           

        }

        private static List<string> getGoalNames()
        {
            List<string> goalNames = new List<string>();
            if (goal.listOfGoals.Count > 0)
            {
                foreach (var goal in goal.listOfGoals)
                {
                    goalNames.Add(goal.name);
                }

            }
            return goalNames;
        }
    }
}
