using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.StoryTeller
{
    /// <summary>
    /// Tells the Story Engine that when a particular scene is loaded the story should jump to or resume from a specific knot.
    /// 
    /// Example Use:
    /// 
    /// >>> ListenForSceneLoad, SCENE_NAME, KNOT_NAME[, ONE_SHOT]
    /// 
    /// SCENE_NAME is the name of the scene to wait for.
    /// KNOT_NAME is the name of the knot to continue from when the scene is loaded.
    /// ONE_SHOT is an optional parameter that if "true" will cause the listener to be removed after the scene is loaded. Any other value will cause the listener to remain active.
    /// 
    /// </summary>
    public class ResumeOnSceneLoadDirection : AbstractDirection
    {
        public override string DirectionName => "Listen For Scene Load";

        public override void Execute(string[] parameters)
        {
            if (!ValidateArgumentCount(parameters, 2, 3))
            {
                return;
            }

            string sceneName = parameters[0].Trim();
            string knotName = parameters[1].Trim();
            bool oneShot = parameters.Length > 2 ? parameters[2].Trim().ToLower() == "true" : false;

            StoryManager.Instance.AddSceneLoadListener(new SceneToKnotMapping(sceneName, knotName, oneShot));
        }
    }
}
