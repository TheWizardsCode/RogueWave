using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// A GameEvent is an arbitrary event that can be listened to with `GameEventListener` which will respond by
    /// taking a defined action.
    /// 
    /// <seealso cref="ParameterizedGameEvent{T}"/>
    /// </summary>
    [CreateAssetMenu(fileName = "New Game Event", menuName ="Rogue Wave/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        [SerializeField, TextArea, Tooltip("A description of this event. This has no gameplay value but is useful in the editor.")]
        private string description;

        private List<IGameEventListener> listeners = new List<IGameEventListener>();

        public virtual void Raise()
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised();
            }
        }

        public void RegisterListener(IGameEventListener listener)
        {
            listeners.Add(listener);
        }

        public void UnregisterListener(IGameEventListener listener)
        {
            listeners.Remove(listener);
        }
    }
}
