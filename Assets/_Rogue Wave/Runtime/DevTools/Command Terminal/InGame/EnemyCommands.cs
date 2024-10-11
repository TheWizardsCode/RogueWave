#if UNITY_EDITOR || DEVELOPMENT_BUILD
using RogueWave;
using System;
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

        [RegisterCommand(Help = "Spawn enemies up to a total challenge rating. Enemies will be selected from those available in spawners near the player. Default challenge rating is 8.", MinArgCount = 0, MaxArgCount = 1)]
        static void SpawnEnemyCR(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            float challengeRating = 8;
            if (args.Length > 0)
            {
                challengeRating = args[0].Float;
            }

            director.SpawnEnemies(challengeRating);
            
            Terminal.Log($"Spawned {challengeRating} enemies.");
        }

        [RegisterCommand(Help = "Spawn specific enemies by name or index (first parameter), the second parameter is the number of these enemies to spawn. Only enemies available in spawners near the player can be spawned. If no name, or an unavailable name is provided then a list of available enemies will be provided.", MinArgCount = 0, MaxArgCount = 2)]
        static void SpawnEnemy(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            BasicEnemyController[] availableEnemies = director.GetSpawnerAvailableEnemies();
            if (args.Length == 0)
            {
                ListToTerminal(availableEnemies);
                return;
            }

            BasicEnemyController enemy = null;
            if (int.TryParse(args[0].String, out int index))
            {
                if (index < 0 || index >= availableEnemies.Length)
                {
                    Terminal.Log("Invalid index.\n");
                    ListToTerminal(availableEnemies);
                    return;
                }
                else
                {
                    enemy = availableEnemies[index];
                }
            } 
            else
            {
                foreach (BasicEnemyController e in availableEnemies)
                {
                    if (e.name == args[0].String)
                    {
                        enemy = e;
                        break;
                    }
                }

                if (enemy == null)
                {
                    Terminal.Log("Invalid enemy name.\n");
                    ListToTerminal(availableEnemies);
                    return;
                }
            }

            int count = 1;
            if (args.Length > 1)
            {
                count = args[1].Int;
            }

            director.SpawnEnemiesNearPlayer(enemy, count);
        }

        private static void ListToTerminal(BasicEnemyController[] enemies)
        {
            if (enemies.Length == 0)
                {
                Terminal.Log("No enemies available in spawners near the player.");
                return;
            }

            Terminal.Log("Available enemies in spawners near the player:");
            for (int i = 0; i < enemies.Length; i++)
            {
                Terminal.Log($"{i}: {enemies[i].name} (CR {enemies[i].challengeRating})");
            }
        }

        [RegisterCommand(Help = "Kill a % of enemies on this level. Default is 75.", MinArgCount = 1, MaxArgCount = 1)]
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

        [RegisterCommand(Help = "Disable spawning in this level.", MinArgCount = 0, MaxArgCount = 0)]
        static void DisableSpawning(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            director.DisableSpawning();
            
            Terminal.Log("Spawning disabled.");
        }

        [RegisterCommand(Help = "Enable spawning in this level.", MinArgCount = 0, MaxArgCount = 0)]
        static void EnableSpawning(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            director.EnableSpawning();
            
            Terminal.Log("Spawning enabled.");
        }
    }
}
#endif
