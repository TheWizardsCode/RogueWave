using NaughtyAttributes;
using RogueWave.GameStats;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// A StatEvent is an event that is raised whenever a GameStat should be changed.
    /// </summary>
    [CreateAssetMenu(fileName = "New Int Stat Event", menuName = "Rogue Wave/Events/Stat Event (Int)")]
    public class IntStatEvent : ParameterizedGameEvent<int>
    {
        [SerializeField, Tooltip("The stat being altered by this event."), Required]
        internal IntGameStat stat;
    }

}
