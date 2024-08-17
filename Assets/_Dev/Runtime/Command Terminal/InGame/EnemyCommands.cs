#if UNITY_EDITOR || DEVELOPMENT_BUILD
using RogueWave;
using UnityEngine;
using WizardsCode.CommandTerminal;
using static UnityEngine.UI.DefaultControls;

namespace WizardsCode.RogueWave.CommandTerminal.InGame
{
    public class EnemyCommands
    {
        [RegisterCommand(Help = "Kill a % of enemies on this level. Default is 75.")]
        static void KillPercentageEnemies(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            float percentage = 0.75f;
            if (args.Length > 0)
            {
                if (args[0].Float < 0 || args[0].Float > 1)
                {
                    Terminal.Log("Percentage must be between 0 and 1.");
                    return;
                }   
                percentage = args[0].Float;
            }

            AIDirector director = GameObject.FindObjectOfType<AIDirector>();
            director.Kill(percentage);
            
            Terminal.Log($"Killed {percentage}% of enemies.");
        }
    }
}
#endif
