using System.Collections;
using UnityEngine;

namespace WizardsCode.StoryTeller
{
    /// <summary>
    /// An interface for an Actor in the story.
    /// </summary>
    public interface IActorController
    {
        string DisplayName { get; }
        bool IsMoving { get; }

        void MoveTo(Transform transform);
        IEnumerator Prompt(IActorController actor);
        void Prompt(ActorCue m_startTalkingCue); // See original implementation in https://github.com/TheWizardsCode/Character-Dev/blob/140f9f3a144c630f3b71cb974e9eb530e47238b0/Assets/Wizards%20Code/Character%20AI/Character/Scripts/Integrations/MxM/MxMActorController.cs#L38
        void StopMoving();
    }
}