using NaughtyAttributes;
using RogueWave.GameStats;
using UnityEngine;
using UnityEngine.Events;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// Listen for and respond to integer game state events that are raised whenever
    /// a stat is changed.
    /// 
    /// <seealso cref="GameEvent"/>
    /// </summary>
    [ExecuteAlways]
    public class IntStatEventListener : MonoBehaviour, IParameterizedGameEventListener<int>
    {
        [SerializeField, Tooltip("The game event to listen for. This listener will invoke the Response event whenever this Game Event is raised.")]
        public ParameterizedGameEvent<int> Event;
        [SerializeField, Tooltip("Unity events to invoke when this event is raised.")]
        UnityEvent<int> unityEvent;
        
        private void OnEnable()
        {
            if (Event != null)
            {
                Event.RegisterListener(this);
            }
        }

        private void OnDisable()
        {
            if (Event != null)
            {
                Event.UnregisterListener(this);
            }
        }

        public void OnEventRaised(ParameterizedGameEvent<int> e, int value)
        {
            unityEvent.Invoke(value);
        }
    }
}
