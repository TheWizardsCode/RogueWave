#if UNITY_EDITOR || DEVELOPMENT_BUILD
using NeoFPS.SinglePlayer;
using WizardsCode.CommandTerminal;

public class PlayerCommands
{
    static FpsSoloCharacter player
    {
        get
        {
            if (FpsSoloCharacter.localPlayerCharacter == null)
            {
                Terminal.Log("No player found in the scene.");
                return null;
            }
            return FpsSoloCharacter.localPlayerCharacter;
        }
    }


    [RegisterCommand(Help = "Kill the player.")]
    static void KillPlayer(CommandArg[] args)
    {
        if (Terminal.IssuedError) return;

        player.Kill();
    }

    [RegisterCommand(Help = "Set the player's health. Default is maxium health.", MinArgCount = 0, MaxArgCount = 1)]
    static void SetHealth(CommandArg[] args)
    {
        if (Terminal.IssuedError) return;

        float health = player.healthManager.healthMax;
        if (args.Length > 0)
        {
            health = args[0].Float;
        }

        player.healthManager.health = health;
    }

    [RegisterCommand(Help = "Set the player's max health. Default is current maximum + 100.", MinArgCount = 0, MaxArgCount = 1)]
    static void SetMaxHealth(CommandArg[] args)
    {
        if (Terminal.IssuedError) return;

        float max = player.healthManager.healthMax + 100;
        if (args.Length > 0)
        {
            max = args[0].Float;
        }

        player.healthManager.healthMax = max;
        SetHealth(args);
    }
}
#endif