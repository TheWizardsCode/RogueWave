using UnityEngine;

namespace WizardsCode.StoryTeller
{
    public class StoryManagedUIElement : MonoBehaviour
    {
        [SerializeField, Tooltip("If this is set to true then this UI element will be disabled during the execution of a tutorial step.")]
        internal bool disableDuringTutorial = false;
    }
}
