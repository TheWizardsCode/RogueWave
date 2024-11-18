using WizardsCode.RogueWave;

namespace RogueWave.Tutorial
{
    public interface ITutorialStep
    {
        public void Execute();

        public TutorialManager TutorialManager { get; set; }
        public bool TriggerBySceneLoad { get; }
        public GameEvent TriggeringEvent { get; }
    }
}
