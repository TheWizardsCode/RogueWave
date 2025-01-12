using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.StoryTeller
{
    /// <summary>
    /// Creates a listener for the scene load event.If the scene being listend for is loaded then the story will continue from the named knot.
    /// 
    /// Example Use:
    /// 
    /// >>> ListenForSceneLoad, SCENE_NAME, KNOT_NAME[, ONE_SHOT]
    /// 
    /// SCENE_NAME is the name of the scene to listen for.
    /// KNOT_NAME is the name of the knot to continue from when the scene is loaded.
    /// ONE_SHOT is an optional parameter that if "true" will cause the listener to be removed after the scene is loaded. Any other value will cause the listener to remain active.
    /// 
    /// </summary>
    public class ListenForSceneLoadDirection : AbstractDirection
    {
        public override string DirectionName => "Listen For Scene Load";

        public override void Execute(string[] parameters)
        {
            if (!ValidateArgumentCount(parameters, 2, 2))
            {
                return;
            }

            string sceneName = parameters[0].Trim();
            string knotName = parameters[1].Trim();
            bool oneShot = parameters.Length > 2 ? parameters[2].Trim().ToLower() == "true" : false;

            StoryManager.Instance.AddSceneLoadListener(sceneName, knotName, oneShot);
        }
    }
}
