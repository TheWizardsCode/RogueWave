using NeoFPS.Samples;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace RogueWave.UI
{
    public class RW_MainMenu : MainMenu
    {
        [SerializeField, Tooltip("The URL to open when the user clicks the 'Join Discord' button.")]
        private string joinDiscordUrl = "https://discord.gg/Mp6XAz9T6w";

        public void JoinDiscord()
        {
            try
            {
                System.Diagnostics.Process.Start(joinDiscordUrl);
            }
            catch (Exception ex)
            {
                GameLog.LogError($"User attempted to connect to Discord but got an error: {ex.Message}");
            }
        }

    }
}
