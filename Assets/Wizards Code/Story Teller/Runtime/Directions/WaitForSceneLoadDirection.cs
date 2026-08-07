using UnityEngine.SceneManagement;

namespace WizardsCode.StoryTeller
{
    /// <summary>
    /// Wait for a specified scene to fully load. The story will pause until the scene is loaded.
    /// Upon resuming the story will continue from the current position or the knot specified in the optional RESUME_KNOT parameter.
    /// 
    /// Parameters:
    /// 
    /// SCENE_NAME - the name of the scene to wait for.
    /// [RESUME_KNOT] - the knot to resume from when the scene is loaded, if none is provided the story continues from the current position.
    /// 
    /// </summary>
    public class WaitForSceneLoadDirection : AbstractWaitForDirection
    {
        public override string DirectionName { get { return "Wait For Duration"; } }

        public string sceneName = string.Empty;
        public string knotName = string.Empty;

        public override bool IsWaiting
        {
            get
            {
                if (!SceneManager.GetSceneByName(sceneName).isLoaded) {
                    return true;
                }

                if (!string.IsNullOrEmpty(knotName))
                {
                    StoryManager.Instance.ResumeFromKnot(knotName);
                }

                return false;
            }
        }

        public override void Execute(string[] parameters)
        {
            if (!ValidateArgumentCount(parameters, 1, 2))
            {
                return;
            }

            sceneName = parameters[0].Trim();
            knotName = parameters.Length > 1 ? parameters[1].Trim() : string.Empty;
            StoryManager.Instance.AddWaitForState(this);
        }
    }
}
