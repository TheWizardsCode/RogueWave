using NeoFPS.SinglePlayer;
using RogueWave;
using UnityEditor;
using UnityEngine;
using WizardsCode.CommandTerminal;

namespace WizardsCode.RogueWave.CommandTerminal.InGame
{
    public class LevelManagementCommands
    {
#if UNITY_EDITOR
        [RegisterCommand(Help = "Set the time in level to a given number of seconds", MinArgCount = 1, MaxArgCount = 1)]
        static void SetTimeInLevel(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            float time = 0;
            if (args.Length > 0)
            {
                time = args[0].Float;
            }

            RogueWaveGameMode gameMode = Object.FindObjectOfType<RogueWaveGameMode>();
            // use reflection to set the m_TimeInLevel field in gameMode
            var field = gameMode.GetType().GetField("timeInLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(gameMode, time);
        }
#endif
    }
}
