#if STEAMWORKS_ENABLED && !STEAMWORKS_DISABLED
using NeoFPS;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WizardsCode.CommandTerminal;

public class SteamworksCommands
{
    [RegisterCommand(Help = "View the current status of the Steam client", MaxArgCount = 0, RuntimeLevel = 0)]
    static void SteamClientStatus(CommandArg[] args)
    {
        if (Terminal.IssuedError) return;

        Terminal.Log("Steam Client Status: " + (SteamClient.IsValid ? "Active" : "Inactive"));
        if (SteamClient.IsValid)
        {
            var playername = SteamClient.Name;
            var playersteamid = SteamClient.SteamId;

            Terminal.Log($"Steam ID: {playersteamid} ({playername})");

            Terminal.Log($"{SteamFriends.GetFriends().Count()} Friends:");
            Terminal.Log("Steam ID, Name, Level, Relationship, State");
            foreach (Friend friend in SteamFriends.GetFriends())
            {
                Terminal.Log($"Steam ID: {friend.Id}, {friend.Name}, {friend.SteamLevel}, {friend.Relationship}, {friend.State}");
            }

        }
    }

    [RegisterCommand(Help = "Take a screenshot.", MaxArgCount = 0, RuntimeLevel = 0)]
    static void TakeScreenshot(CommandArg[] args)
    {
        if (Terminal.IssuedError) return;

        SteamScreenshots.TriggerScreenshot();
    }
}
#endif