
namespace WizardsCode.RogueWave
{
    public interface IParameterizedGameEventListener<T>
    {
        void OnEventRaised(IParameterizedGameEvent<T> e, T parameters);
    }
}
