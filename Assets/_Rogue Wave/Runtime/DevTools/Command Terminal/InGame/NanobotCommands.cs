#if UNITY_EDITOR || DEVELOPMENT_BUILD
using NeoFPS.SinglePlayer;
using RogueWave;
using WizardsCode.CommandTerminal;

public class NanobotCommands
{
    static NanobotManager m_NanobotManager = null;
    static NanobotManager nanobotManager
    {
        get
        {
            if (m_NanobotManager == null)
            {
                m_NanobotManager = FpsSoloCharacter.localPlayerCharacter.GetComponent<NanobotManager>();
            }
            return m_NanobotManager;
        }
    }


    [RegisterCommand(Help = "Force the Nanobots to level up.")]
    static void LevelUpNanobots(CommandArg[] args)
    {
        if (Terminal.IssuedError) return;

        // using reflection increment the private field stackedLevelUps on m_NanobotManager
        System.Reflection.FieldInfo field = typeof(NanobotManager).GetField("stackedLevelUps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        int stackedLevelUps = (int)field.GetValue(nanobotManager);
        stackedLevelUps++;
        field.SetValue(nanobotManager, stackedLevelUps);
    }
}
#endif