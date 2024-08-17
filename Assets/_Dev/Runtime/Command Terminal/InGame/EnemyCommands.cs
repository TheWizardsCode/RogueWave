#if UNITY_EDITOR || DEVELOPMENT_BUILD
using RogueWave;
using UnityEngine;
using WizardsCode.CommandTerminal;

namespace WizardsCode.RogueWave.CommandTerminal.InGame
{
    public class EnemyCommands
    {
        static AIDirector m_Director;
        static AIDirector director
        {
            get
            {
                if (m_Director == null)
                {
                    m_Director = GameObject.FindObjectOfType<AIDirector>();
                }

                return m_Director;
            }
        }

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

            director.Kill(percentage);
            
            Terminal.Log($"Killed {percentage}% of enemies.");
        }

        [RegisterCommand(Help = "Disable spawning in this level.")]
        static void DisableSpawning(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            director.DisableSpawning();
            
            Terminal.Log("Spawning disabled.");
        }

        [RegisterCommand(Help = "Enable spawning in this level.")]
        static void EnableSpawning(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            director.EnableSpawning();
            
            Terminal.Log("Spawning enabled.");
        }
    }
}
#endif
