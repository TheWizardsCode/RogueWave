namespace WizardsCode.RogueWave
{
    public interface IGameStat<T>
    {
        string key { get; }
        string displayName { get; }
        string description { get; }

        T Value { get; set; }
        T Add(T change);
        T Subtract(T change);

        int ScoreContribution { get; }
    }
}
