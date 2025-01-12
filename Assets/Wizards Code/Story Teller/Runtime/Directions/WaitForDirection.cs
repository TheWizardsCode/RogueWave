namespace WizardsCode.StoryTeller
{
    /// <summary>
     /// Wait for a particular game state. Supported states are:
     ///
     /// ReachedTarget - waits for the actor to have reached their move target
     /// SceneLoaded, SCENE_NAME - waits for the named scene to be fully loaded before continuing
     /// [a float] - waits for a duration (in seconds)
     ///
     /// </summary>
     /// <param name="args">ACTOR | DURATION</param>
    public class WaitForDirection : AbstractDirection
    {
        public override string DirectionName { get { return "Wait For Direction"; } }

        public override void Execute(string[] parameters)
        {
            if (!ValidateArgumentCount(parameters, 1, 2))
            {
                return;
            }

            string param1 = parameters[0].Trim();
            bool isFloat = float.TryParse(param1, out float time);
            if (isFloat)
            {
                StoryManager.Instance.AddWaitForState(new WaitForState(time));
            }
            else if (param1.ToLower() == "reachedtarget")
            {
                StoryManager.Instance.AddWaitForState(new WaitForState(StoryManager.Instance.FindActor(param1)));
            }
            else if (param1.ToLower() == "sceneloaded")
            {
                StoryManager.Instance.AddWaitForState(new WaitForState(parameters[1]));
            } else
            {
                LogError($"Direction to WaitFor game event with the arguments {string.Join(", ", parameters)} does not contain a recognizable event to wait for.", parameters);
            }
        }
    }
}
