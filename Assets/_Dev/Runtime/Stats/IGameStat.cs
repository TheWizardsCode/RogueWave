namespace WizardsCode.RogueWave
{
    public interface IGameStat<T>
    {
        string key { get; }
        string displayName { get; }
        string description { get; }

        T value { get; }
        T SetValue(T value);
        T Add(T change);
        T Subtract(T change);

        int ScoreContribution { get; }

        string ValueAsString { get; }
    }
}
