#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
using System.Linq;
using HeathenEngineering.SteamworksIntegration;
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
        Terminal.Log("Steam Reported App ID: " + App.Client.Id.m_AppId.ToString());
        Terminal.Log("User Defined App ID:" + SteamSettings.ApplicationId.m_AppId.ToString());

        if (App.Client.Id.m_AppId != SteamSettings.ApplicationId.m_AppId)
        {
            Terminal.LogError("The Steam App ID in the Steam Settings does not match the Steam Client reported App ID.");
        }

        Terminal.Log("Steam User ID: " + SteamUser.GetSteamID().m_SteamID);
        Terminal.Log("Steam User Name: " + User.Client.Id.Name);
    }

    [RegisterCommand(Help = "Take a screenshot.", MaxArgCount = 0, RuntimeLevel = 0)]
    static void TakeScreenshot(CommandArg[] args)
    {
        if (Terminal.IssuedError) return;

        SteamworksController.Instance.TakeScreenshot();
    }
}
#endif