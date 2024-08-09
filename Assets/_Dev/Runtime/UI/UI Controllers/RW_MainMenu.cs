using NeoFPS;
using NeoFPS.Samples;
using System;
using UnityEngine;

namespace RogueWave.UI
{
    public class RW_MainMenu : MainMenu
    {
        [SerializeField, Tooltip("The URL to open when the Wishlist button is clicked.")]
        private string wishlistUrl = "https://store.steampowered.com/app/2895630/Rogue_Wave";
        [SerializeField, Tooltip("The URL to open when the Follow button is clicked.")]
        private string followUrl = "https://store.steampowered.com/app/2895630/Rogue_Wave";
        [SerializeField, Tooltip("The URL to open when the user clicks the 'Join Discord' button.")]
        private string joinDiscordUrl = "https://discord.gg/Mp6XAz9T6w";

        protected override void OnEnable()
        {
            base.OnEnable();
            NeoFpsInputManager.captureMouseCursor = false;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            NeoFpsInputManager.captureMouseCursor = true;
        }

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

        public void Wishlist()
        {
            try
            {
                System.Diagnostics.Process.Start(wishlistUrl);
            }
            catch (Exception ex)
            {
                GameLog.LogError($"User attempted to connect to the Wishlist but got an error: {ex.Message}");
            }
        }

        public void Follow()
        {
            try
            {
                System.Diagnostics.Process.Start(followUrl);
            }
            catch (Exception ex)
            {
                GameLog.LogError($"User attempted to connect to the Follow page but got an error: {ex.Message}");
            }
        }

    }
}
