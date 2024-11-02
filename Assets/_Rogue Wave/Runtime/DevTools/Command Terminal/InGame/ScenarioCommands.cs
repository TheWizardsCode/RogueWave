using UnityEngine;
using WizardsCode.CommandTerminal;
using WizardsCode.RogueWave;
using UnityEngine.SceneManagement;
using RogueWave;
using NeoSaveGames.SceneManagement;

public class ScenarioCommands
{
    [RegisterCommand(Help = "Load a Scenario. Provide no argument to get a list of available scenarios. Provide either the scenario index or a unique portion of the name to load it.", MinArgCount = 0, MaxArgCount = 1)]
    static void LoadScenario(CommandArg[] args)
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
    Terminal.LogError("This command is only available in the editor or development builds.");
#endif

        if (Terminal.IssuedError) return;

        ScenarioDescriptor[] availableScenarios = Resources.LoadAll<ScenarioDescriptor>("");
        if (args.Length == 0)
        {
            ListToTerminal(availableScenarios);
            return;
        }

        ScenarioDescriptor scenarioPrototype = null;
        if (int.TryParse(args[0].String, out int index))
        {
            if (index < 0 || index >= availableScenarios.Length)
            {
                Terminal.Log("Invalid index.\n");
                ListToTerminal(availableScenarios);
                return;
            }
            else
            {
                scenarioPrototype = availableScenarios[index];
            }
        }
        else
        {
            foreach (ScenarioDescriptor lvl in availableScenarios)
            {
                if (lvl.name.ToLower().Contains(args[0].String.ToLower()))
                {
                    scenarioPrototype = lvl;
                    break;
                }
            }

            if (scenarioPrototype == null)
            {
                Terminal.Log("Invalid level name.\n");
                ListToTerminal(availableScenarios);
                return;
            }
        }

        Terminal.Instance.Close();

        // TODO: This should create and load a temporary profile that gets deleted at the end of the game.
        RogueLiteManager.LoadProfile(0);
        NeoSceneManager.LoadScene(ScenarioManager.SCENARIO_PLAYER_SCENE);
    }

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
            Terminal.Log($"{i}: {levels[i].DisplayName} - {levels[i].Description}");
        }
    }
}
