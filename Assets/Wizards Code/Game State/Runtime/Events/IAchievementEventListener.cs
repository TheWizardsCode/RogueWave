using RogueWave.GameStats;

namespace WizardsCode.RogueWave
{
    public interface IAchievementEventListener
    {
        // Typically an implementation will look like this:

        // AchievementUnlockedEvent Event;
        // UnityEvent<Achievement> Response;
        
        // void OnEnable();
        //{
        //    Event.AddListener(this);
        //}

        //void OnDisable();
        //{
        //    Event.RemoveListener(this);
        //}

        void OnEventRaised(Achievement achievement);
        // Typically an implementation will look like this:
        //{
        //    Response.Invoke(achievement);
        //}


    }
}