namespace WizardsCode.StoryTeller
{
    /// <summary>
     /// Stop the story telling and wait for a signal to resume, such as a response to a `ListenFor*` direction.:
     ///
     /// </summary>
     /// <param name="args">ACTOR | DURATION</param>
    public class WaitForResumeSignalDirection : AbstractWaitForDirection
    {
        public override string DirectionName { get { return "Wait For Resume Signal"; } }

        bool isStoryPaused = false;

        public override bool IsWaiting
        {
            get
            {
                return isStoryPaused;
            }
        }

        public override void Execute(string[] parameters)
        {
            if (!ValidateArgumentCount(parameters, 0, 0))
            {
                return;
            }

            isStoryPaused = true;
            StoryManager.Instance.AddWaitForState(this);
        }
    }
}
