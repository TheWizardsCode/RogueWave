using System.Text;
using UnityEngine;

namespace WizardsCode.CommandTerminal
{
    public class SceneManagementCommands
    {

        #region Lifecycle
        public static void OnDestroy()
        {
        }
        #endregion

        [RegisterCommand(Help = "Lists all the scenes that are available in this build.")]
        public static void ListScenes(CommandArg[] args)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                sb.AppendLine(sceneName);
            }

            Terminal.Log(sb.ToString());
        }
    }
}