#if UNITY_EDITOR || DEVELOPMENT_BUILD
using RogueWave.GameStats;
using RogueWave.Tutorial;
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
    }
}
#endif
