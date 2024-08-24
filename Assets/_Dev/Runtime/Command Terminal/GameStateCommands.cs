#if UNITY_EDITOR || DEVELOPMENT_BUILD
using RogueWave.GameStats;
using RogueWave.Tutorial;
using System.Linq;
using UnityEngine;
using WizardsCode.CommandTerminal;

namespace WizardsCode.RogueWave.CommandTerminal
{
    public class GameStateCommands
    {
        [RegisterCommand(Help = "Clears all tutorial progress so that the tutorial will be displayed again.")]
        static void ResetTutorial(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            TutorialManager.ClearTutorialProgress();
            Terminal.Log("Tutorial progress cleared.");
        }

        [RegisterCommand(Help = "Clears all stats and achievements.")]
        static void ResetStats(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            GameStatsManager.ResetLocalStatsAndAchievements();
            Terminal.Log("Local stats and achievements cleared.");
        }


        [RegisterCommand(Help = "Dump stats to the console. If no parameter is provided then all stats will be displayed. If a parameter is provided it is search string for the displayname of the stat.", 
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
