using UnityEngine;

namespace WizardsCode.CommandTerminal
{
    /// <summary>
    /// A set of commands to control the performance of the game. For example, changing graphics quality settings.
    /// </summary>
    public class PerformanceManagementCommands
    {
        private static bool logFPS;
        private static float avgFPS;
        private int framesSinceFPSLog;


        #region Lifecycle
        public static void OnDestroy()
        {
        }
        #endregion

        [RegisterCommand(Help = "Set the quality level of the game. Provide no paramerters to list available levels. Provide a number to set the level. Higher numbers are higher quality.", MinArgCount = 0, MaxArgCount = 1)]
        public static void SetQuality(CommandArg[] args)
        {
            if (args.Length == 0 || args[0].Int < 0 || args[0].Int >= QualitySettings.names.Length)
            {
                // Iterate over the qualitysettings.names and display a numbered list of the available settings
                for (int i = 0; i < QualitySettings.names.Length; i++)
                {
                    Terminal.Log($"{i} {QualitySettings.names[i]}");
                }
                Terminal.Log("Run the command SetQuality <number> to set the quality level");
                return;
            }

            QualitySettings.SetQualityLevel(args[0].Int);
            Debug.Log($"Quality level set to {QualitySettings.names[args[0].Int]}.");
        }

// Now this is no longer a monobehaviour this won't work. We need a way of running a command as a corouting
//#if DEVELOPMENT_BUILD || UNITY_EDITOR
//        [RegisterCommand(Name = "FPS", Help = "Toggle logging FPS to terminal every 30 frames.", MinArgCount = 0, MaxArgCount = 0)]
//        public static void FPS(CommandArg[] args)
//        {
//            logFPS = !logFPS;
//            if (logFPS)
//            {
//                Terminal.Log("FPS logging turned on. Note that is can take a few seconds for this to stabalize. Run the command `fps` to turn it off again.");
//            }
//            else
//            {
//                Terminal.Log("FPS logging turned off. Run the command `fps` to turn it on again.");
//            }
//        }
        
//        private void Update()
//        {
//            // If FPS logging is enabled, log the FPS every 30 frames.
//            if (logFPS)
//            {
//                // Calculate average FPS
//                avgFPS += (Time.unscaledDeltaTime - avgFPS) * 0.03f;
//                framesSinceFPSLog++;
                
//                if (framesSinceFPSLog == 30)
//                {
//                    Terminal.LogWithScrollReset($"FPS: {(1f / avgFPS).ToString("0")}");
//                    Debug.Log($"FPS: {(1f / avgFPS).ToString("0")}");
//                    framesSinceFPSLog = 0;
//                }
//            }
//        }
//#endif
    }
}