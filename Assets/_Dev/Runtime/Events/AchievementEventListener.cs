using RogueWave.GameStats;
using UnityEngine;
using UnityEngine.Events;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// Listen for and respond to an Achievement event that is raised whenever
    /// an achievement is unlocked.
    /// </summary>
    /// <seealso cref="Achievement"/>"/>
    public class AchievementEventListener : MonoBehaviour, IAchievementEventListener
    {
        [SerializeField, Tooltip("The AchievementEvent to listen for events from. This listener will invoke the Response event whenever this Achievement Event is raised.")]
        public AchievementUnlockedEvent Event;
        [SerializeField, Tooltip("The Unity event to invoke whenever the Game Event is raised.")]
        public UnityEvent<Achievement> Response;

        public void OnEnable()
        {
            Event.AddListener(this);
        }

        public void OnDisable()
        {
            Event.RemoveListener(this);
        }

        public virtual void OnEventRaised(Achievement achievement)
        {
            Response.Invoke(achievement);
        }
    }
}
