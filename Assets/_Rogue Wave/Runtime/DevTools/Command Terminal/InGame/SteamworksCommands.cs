#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
using System.Linq;
using HeathenEngineering.SteamworksIntegration.API;
using Steamworks;
using UnityEngine;
using WizardsCode.CommandTerminal;
using WizardsCode.RogueWave;

public class SteamworksCommands
{
    [RegisterCommand(Help = "View the current status of the Steam client", MaxArgCount = 0, RuntimeLevel = 0)]
    static void SteamClientStatus(CommandArg[] args)
    {
        if (Terminal.IssuedError) return;

        Terminal.Log("Steam Running: " + (SteamAPI.IsSteamRunning() ? "Yes" : "No"));

        Terminal.Log("Steam Client Status: " + (App.Initialized ? "Initialized" : "Inactive"));
    }

    [RegisterCommand(Help = "Take a screenshot.", MaxArgCount = 0, RuntimeLevel = 0)]
    static void TakeScreenshot(CommandArg[] args)
    {
        if (Terminal.IssuedError) return;

        SteamworksController.Instance.TakeScreenshot();
    }
}
#endif