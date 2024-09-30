using RogueWave;
using RogueWave.GameStats;
using WizardsCode.CommandTerminal;

namespace WizardsCode.RogueWave.CommandTerminal
{
    public class ResourceCommands
    {
        [RegisterCommand(Help = "Add resources to the current profile, Defaulting to 10000, but can be set as a parameter.", MinArgCount = 0, MaxArgCount = 1)]
        public static void ResourcesAdd(CommandArg[] args)
        {
            if (string.IsNullOrEmpty(RogueLiteManager.currentProfile))
            {
                Terminal.Log("No profile selected.");
                return;
            }

            int resources = 10000;
            if (args.Length > 0)
            {
                resources = args[0].Int;
            }

            GameStatsManager.Instance.GetIntStat("RESOURCES").Add(resources);
            Terminal.Log($"Added {resources} resources to {RogueLiteManager.currentProfile}.");
        }

        [RegisterCommand(Help = "Remove resources from the current profile, Defaulting to 10000, but can be set as a parameter. Resource will not go below 0.", MinArgCount = 0, MaxArgCount = 1)]
        public static void ResourcesRemove(CommandArg[] args)
        {
            if (string.IsNullOrEmpty(RogueLiteManager.currentProfile))
            {
                Terminal.Log("No profile selected.");
                return;
            }

            int resources = 10000;
            if (args.Length > 0)
            {
                resources = args[0].Int;
            }

            GameStatsManager.Instance.GetIntStat("RESOURCES").Subtract(resources);
            Terminal.Log($"Removed {resources} resources to {RogueLiteManager.currentProfile}.");
        }
    }
}
