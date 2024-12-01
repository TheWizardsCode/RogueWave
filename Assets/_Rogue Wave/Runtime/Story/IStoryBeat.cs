using RogueWave.GameStats;
using System.Collections;
using WizardsCode.RogueWave;

namespace RogueWave.Story
{
    public interface IStoryBeat
    {
        public StoryManager StoryManager { get; set; }
        public GameEvent RequiredEvent { get; }
        public Achievement RequiredAchievement { get; }
        public bool HasSceneTrigger { get; }
        public string SceneName { get; }
        public bool IsComplete { get; set; }

        public IEnumerator Execute();

        /// <summary>
        /// Is this step ready to execute?
        /// This will be true when all the conditions for the step to execute have been met.
        /// </summary>
        public bool ReadyToExecute { get; }

        public void Reset();
    }
}
