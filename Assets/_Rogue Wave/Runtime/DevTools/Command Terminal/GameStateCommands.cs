#if UNITY_EDITOR || DEVELOPMENT_BUILD
using RogueWave.GameStats;
using RogueWave.Story;
using System.Linq;
using UnityEngine;
using WizardsCode.CommandTerminal;

namespace WizardsCode.RogueWave.CommandTerminal
{
    public class GameStateCommands
    {
        [RegisterCommand(Help = "Clears all tutorial progress so that the tutorial will be displayed again.", RuntimeLevel = 0)]
        static void ResetTutorial(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            StoryManager.ClearStoryProgress();
            Terminal.Log("Tutorial progress cleared.");
        }

        [RegisterCommand(Help = "Clears all stats and achievements.", RuntimeLevel = 0)]
        static void ResetStats(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            GameStatsManager.ResetLocalStatsAndAchievements();
            Terminal.Log("Local stats and achievements cleared.");
        }


        [RegisterCommand(Help = "Dump stats to the console. If no parameter is provided then all stats will be displayed. If a parameter is provided it is search string for the displayname of the stat.", RuntimeLevel = 0, 
            MinArgCount = 0, MaxArgCount = 1)]
        static void DumpStats(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            IntGameStat[] gameStats = Resources.LoadAll<IntGameStat>("").OrderBy(stat => stat.displayName).ToArray();

            foreach (IntGameStat stat in gameStats)
            {
                if (stat.key != null)
                {
                    if (args.Length == 1 && !stat.key.ToLower().Contains(args[0].ToString())) 
                    {
                        continue;
                    }

                    Terminal.Log($"{stat.key} = {stat.value}");
                }
            }
        }
    }
}
#endif
