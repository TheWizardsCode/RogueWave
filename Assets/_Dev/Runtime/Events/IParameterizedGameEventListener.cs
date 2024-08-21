
namespace WizardsCode.RogueWave
{
    public interface IParameterizedGameEventListener<T>
    {
        void OnEventRaised(ParameterizedGameEvent<T> e, T parameters);
    }
}
