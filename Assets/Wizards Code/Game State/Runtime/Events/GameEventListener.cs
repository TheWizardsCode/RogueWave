using UnityEngine;
using UnityEngine.Events;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// List for and respond to arbitrary game events.
    /// 
    /// Add this listener to a GameObject, create a `GameEvent` to listen for and define a Unity Event 
    /// that will be invoked when the event is raised. 
    /// 
    /// <seealso cref="GameEvent"/>
    /// </summary>
    public class GameEventListener : MonoBehaviour, IGameEventListener
    {
        [SerializeField, Tooltip("The game event to listen for. This listener will invoke the Response event whenever this Game Event is raised.")]
        public GameEvent Event;
        [SerializeField, Tooltip("The Unity event to invoke whenever the Game Event is raised.")]
        public UnityEvent Response;

        private void OnEnable()
        {
            Event.RegisterListener(this);
        }

        private void OnDisable()
        {
            Event.UnregisterListener(this);
        }

        public virtual void OnEventRaised()
        {
            Response.Invoke();
        }
    }
}
