using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    public interface IParameterizedGameEvent<T>
    {
        void Raise(T parameters);

        void RegisterListener(IParameterizedGameEventListener<T> listener);

        void UnregisterListener(IParameterizedGameEventListener<T> listener);
    }
}
