using UnityEngine.SceneManagement;
using WizardsCode.CommandTerminal;

public class PlaytestCommands
{
    [RegisterCommand(Help = "Load a playtest scene by name.", MinArgCount = 0, MaxArgCount = 1)]
    static void LoadPlaytest(CommandArg[] args)
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
    Terminal.LogError("This command is only available in the editor or development builds.");
#endif

        if (Terminal.IssuedError) return;

        Terminal.Instance.Close();

        string sceneName = "Playtest Dev";
        if (args.Length > 0)
        {
            Terminal.LogError("Loading scenes by name is not yet supported.");
        }

#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.LoadScene(sceneName);
#endif
#if DEVELOPMENT_BUILD
        SceneManager.LoadScene(sceneName);
#endif
    }
}
