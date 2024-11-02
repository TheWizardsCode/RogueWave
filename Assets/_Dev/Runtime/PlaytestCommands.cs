using WizardsCode.CommandTerminal;
using WizardsCode.RogueWave;

#if UNITY_EDITOR
using UnityEngine.SceneManagement;
#endif

public class PlaytestCommands
{

    private static void ListToTerminal(ScenarioDescriptor[] levels)
    {
        if (levels.Length == 0)
        {
            Terminal.Log("No test levels available.");
            return;
        }

        Terminal.Log("Available levels to load:");
        for (int i = 0; i < levels.Length; i++)
        {
            Terminal.Log($"{i}: {levels[i].DisplayName}: {levels[i].Description})");
        }
    }
}
