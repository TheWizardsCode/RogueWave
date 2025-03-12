using Codice.Client.BaseCommands;

namespace WizardsCode.StoryTeller
{
    /// <summary>
    /// SetSavePoint is used to record the name of the knot that should be used as the resume point for the game.
    /// 
    /// It saves the state of the game at the point that this direction is executed.
    /// 
    /// Parameters:
    /// 
    /// KNOT_NAME - the name of the knot at which the story UI should restart.
    /// 
    /// </summary>
    public class SetSavePointDirection : AbstractDirection
    {
        public override string DirectionName => "Set Save Point";

        public override void Execute(string[] parameters)
        {
            if (!ValidateArgumentCount(parameters, 1, 1))
            {
                return;
            }

            StoryManager.Instance.Save(parameters[0]);
        }
    }
}
