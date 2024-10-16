using System;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// A GameEvent is an arbitrary event that can be listened to with `GameEventListener` which will respond by
    /// taking a defined action. 
    /// 
    /// The type for this generic event 
    /// </summary>
    public abstract class ParameterizedGameEvent<T> : ScriptableObject, IParameterizedGameEvent<T>
    {
        [SerializeField, TextArea, Tooltip("A description of this event. This has no gameplay value but is useful in the editor.")]
        private string description;
        [SerializeField, TextArea, Tooltip("A desription of the parameters to be provided when the event is raised.")]
        string parameters;

        private List<IParameterizedGameEventListener<T>> listeners = new List<IParameterizedGameEventListener<T>>();

        public virtual void Raise(T parameter)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised(this, parameter);
            }
        }

        public void AddListener(IParameterizedGameEventListener<T> listener)
        {
            listeners.Add(listener);
        }

        public void RemoveListener(IParameterizedGameEventListener<T> listener)
        {
            listeners.Remove(listener);
        }
    }
}
