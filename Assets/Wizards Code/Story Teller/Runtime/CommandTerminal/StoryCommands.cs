using WizardsCode.CommandTerminal;

namespace WizardsCode.StoryTeller
{
    public class StoryCommands
    {
        [RegisterCommand(Help = "Clears all tutorial progress so that the tutorial will be displayed again.", RuntimeLevel = 0)]
        static void ResetTutorial(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;

            StoryManager.ClearStoryProgress();
            Terminal.Log("Tutorial progress cleared.");
        }
    }
}
