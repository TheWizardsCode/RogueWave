using NeoFPS;
using NeoFPS.Samples;
using RogueWave.GameStats;
using System;
using UnityEngine;

namespace RogueWave.UI
{
    public class RW_MainMenu : MainMenu
    {
        [SerializeField, Tooltip("The URL to open when the Wishlist button is clicked.")]
        private string wishlistUrl = "steam://openurl/https://store.steampowered.com/app/2895630/Rogue_Wave";
        [SerializeField, Tooltip("The URL to open when the Follow button is clicked.")]
        private string followUrl = "steam://openurl/https://store.steampowered.com/app/2895630/Rogue_Wave";
        [SerializeField, Tooltip("The URL to open when the user clicks the 'Join Discord' button.")]
        private string joinDiscordUrl = "https://discord.gg/Mp6XAz9T6w";

        protected override void OnEnable()
        {
            base.OnEnable();

            MusicManager.Instance.PlayMenuMusic();

#if DISCORD_ENABLED
            // TODO: should only do this if the game stats have changed since the last time they were sent
            GameStatsManager.Instance.SendDataToWebhook("Main Menu `OnEnable`");
#endif
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
