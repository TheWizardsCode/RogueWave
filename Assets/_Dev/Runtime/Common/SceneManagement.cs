using System.IO;
using UnityEngine.SceneManagement;

namespace WizardsCode.Common
{
    public class SceneManagement
    {

        public static string SceneNameFromIndex(int BuildIndex)
        {
            return Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(BuildIndex));
        }

        public static int SceneBuildIndexFromName(string SceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                if (SceneNameFromIndex(i) == SceneName)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}