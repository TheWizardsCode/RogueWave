using UnityEngine;

namespace WizardsCode.CommandTerminal
{
    /// <summary>
    /// A set of commands to control the performance of the game. For example, changing graphics quality settings.
    /// </summary>
    public class EngineCommands 
    {
        private static bool logFPS;
        private static float avgFPS;
        private int framesSinceFPSLog;


        #region Lifecycle
        public static void OnDestroy()
        {
        }
        #endregion

        [RegisterCommand(Help = "Set the timeScale value. 0 = paused, 1 = normal speed, 5x = 5 times normal speed.", MinArgCount = 0, MaxArgCount = 1)]
        public static void SetTimeScale(CommandArg[] args)
        {
            if (args.Length == 0 || args[0].Int < 1)
            {
                Terminal.Log("`SetTimeScale` <number> sets the timescale provided. Requires 1 parameter from 0 = paused, 1 = normal speed, 5x = 5 times normal speed.");
                return;
            }

            Time.timeScale = args[0].Float;
        }


#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private void Update()
        {
            // If FPS logging is enabled, log the FPS every 30 frames.
            if (logFPS)
            {
                // Calculate average FPS
                avgFPS += (Time.unscaledDeltaTime - avgFPS) * 0.03f;
                framesSinceFPSLog++;
                
                if (framesSinceFPSLog == 30)
                {
                    Terminal.LogWithScrollReset($"FPS: {(1f / avgFPS).ToString("0")}");
                    framesSinceFPSLog = 0;
                }
            }
        }
#endif
    }
}