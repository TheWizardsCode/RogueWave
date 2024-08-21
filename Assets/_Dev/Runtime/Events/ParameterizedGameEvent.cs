using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// A GameEvent is an arbitrary event that can be listened to with `GameEventListener` which will respond by
    /// taking a defined action.
    /// </summary>
    public abstract class ParameterizedGameEvent<T> : ScriptableObject
    {
        [SerializeField, TextArea, Tooltip("A description of this event. This has no gameplay value but is useful in the editor.")]
        private string description;

        private List<IParameterizedGameEventListener<T>> listeners = new List<IParameterizedGameEventListener<T>>();

        public virtual void Raise(T parameters)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised(this, parameters);
            }
        }

        public void RegisterListener(IParameterizedGameEventListener<T> listener)
        {
            listeners.Add(listener);
        }

        public void UnregisterListener(IParameterizedGameEventListener<T> listener)
        {
            listeners.Remove(listener);
        }
    }
}
