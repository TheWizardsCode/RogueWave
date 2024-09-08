using RogueWave;
using RogueWave.GameStats;
using System.Runtime.InteropServices;
using System;
using WizardsCode.CommandTerminal;

namespace WizardsCode.RogueWave.CommandTerminal
{
    /// <summary>
    /// Commands for simulating key presses in the game.
    /// </summary>
    public class KeyCommands
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const int KEYEVENTF_KEYDOWN = 0x0000;
        private const int KEYEVENTF_KEYUP = 0x0002;

#if BUILD_ARCH_WINDOWSX64
        [RegisterCommand(Help = "Simulate pressing the Pause key.", MinArgCount = 0, MaxArgCount = 0)]
        public static void PressEsc(CommandArg[] args)
        {
            byte keyCode = 0x1B; // Escape key
            keybd_event(keyCode, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            keybd_event(keyCode, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
#endif
    }
}
