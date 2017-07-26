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
        const string ukCommandSet = "TrackGoalsCommandSet_en-gb";
        const string usCommandSet = "TrackGoalsCommandSet_en-us";
        static VoiceCommandDefinition dab;
        public async static void updatePhraseList()
        {
            List<string> goalNames = getGoalNames();
            //repeat FOREACH commandset
            List<string> commandSetNames = new List<string> { ukCommandSet, usCommandSet };
            foreach (string commandSetName in commandSetNames)
            {
                if (VoiceCommandDefinitionManager.InstalledCommandDefinitions.TryGetValue(commandSetName, out dab))
                {
                    await dab.SetPhraseListAsync("goalInProgress", goalNames);

                }

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
