
using UnityEngine;

namespace WizardsCode.StoryTeller
{
    /// <summary>
    /// Wait for a specified actor to reach their current move target.
    /// 
    /// Parameters:
    /// 
    /// ACTOR_NAME - the name of the actor to wait for.
    /// 
    /// </summary>
    public class WaitForReachTargetDirection : AbstractWaitForDirection
    {
        public override string DirectionName { get { return "Wait For Duration"; } }

        public IActorController actor;

        public override bool IsWaiting
        {
            get
            {
                return actor.IsMoving;
            }
        }

        public override void Execute(string[] parameters)
        {
            if (!ValidateArgumentCount(parameters, 1))
            {
                return;
            }

            actor = StoryManager.Instance.FindActor(parameters[0]);
            StoryManager.Instance.AddWaitForState(this);
        }
    }
}
