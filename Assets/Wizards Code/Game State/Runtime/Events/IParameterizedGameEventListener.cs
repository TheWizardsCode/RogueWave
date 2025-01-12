
using UnityEngine.Events;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    public interface IParameterizedGameEventListener<T>
    {
        // A typical implementation will look like this:
        
        //public ParameterizedGameEvent<T> Event;
        //public UnityEvent<IParameterizedGameEvent<T>, T> unityEvent;

        //private void OnEnable()
        //{
        //    Event.AddListener(this);
        //}

        //private void OnDisable()
        //{
        //    Event.RemoveListener(this);
        //}

        //public void OnEventRaised(IParameterizedGameEvent<T> e, T parameters)
        //{
        //    unityEvent?.Invoke(e, parameters);
        //}

        void OnEventRaised(IParameterizedGameEvent<T> e, T parameters);
    }
}
