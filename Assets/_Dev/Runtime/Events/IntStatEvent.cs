using NaughtyAttributes;
using RogueWave.GameStats;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// A StatEvent is an event that is raised whenever a GameStat should be changed. Typically
    /// these are listened to by the `GameStatsManager` which will adjust the stat accordingly.
    /// However since it is a type of `GameEvent` any object can listen by adding a 
    /// `GameEventListener` which can respond by invoking a `UnityEvent`.
    /// </summary>
    [CreateAssetMenu(fileName = "New Stat Event", menuName = "Rogue Wave/Events/Stat Event")]
    public class IntStatEvent : ParameterizedGameEvent<int> {
        [SerializeField, Tooltip("The stat being altered by this event."), Required]
        internal GameStat stat;
    }

}
