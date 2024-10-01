using UnityEngine;
using UnityEngine.Events;

namespace WizardsCode.RogueWave
{
    public class ParameterizedGameEventListener<T> : MonoBehaviour, IParameterizedGameEventListener<T>
    {
        [SerializeField, Tooltip("The game event to listen for. This listener will invoke the Response event whenever this Game Event is raised.")]
        public ParameterizedGameEvent<T> Event;
        [SerializeField, Tooltip("Unity events to invoke when this event is raised.")]
        public UnityEvent<IParameterizedGameEvent<T>, T> unityEvent;
        
        private void OnEnable()
        {
            if (Event != null)
            {
                Event.AddListener(this);
            }
        }

        private void OnDisable()
        {
            if (Event != null)
            {
                Event.RemoveListener(this);
            }
        }

        public void OnEventRaised(IParameterizedGameEvent<T> e, T parameters)
        {
            unityEvent?.Invoke(e, parameters);
        }
    }
}
