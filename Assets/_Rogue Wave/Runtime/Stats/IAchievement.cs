namespace WizardsCode.RogueWave
{
    /// <summary>
    /// An achievement is something a player unlocks by taking certain actions. 
    /// For optimization purposes you need to override this class to create concrete instances that define the 
    /// right types, but for testing purposes you can simply create instances of this class.
    /// 
    /// Simply creating an achievement instance in the game will automatically set it up for tracking. 
    /// You don't need to configure anything. Great isn't it :-)
    /// </summary>
    /// <typeparam name="T">The type of GameStat to track.</typeparam>
    /// <typeparam name="T1">The type of the value to track against, should match the GameStat type.</typeparam>
    /// <seealso cref="IGameStat"/>
    public interface IAchievement<T, T1>
    {
        string key { get; }
    }
}
