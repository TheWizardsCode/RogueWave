using UnityEngine;

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
    public abstract class AbstractWaitForDirection : AbstractDirection
    {
        /// <summary>
        /// Test to see if the condition we are waiting for has been met. If it has return false, otherwise return true.
        /// </summary>
        public abstract bool IsWaiting { get; }
    }
}
