using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.StoryTeller
{
    /// <summary>
    /// Hide the story UI. This is used when you know there is no more story to communicate to the player for some time,
    /// The Story Manage will automatically show the UI again when it is time to communicate more story.
    /// 
    /// While the UI is hidden the story manager will still be running, so you can still progess the story for 
    /// actor directions, audio etc.
    /// 
    /// Example Use:
    /// 
    /// >>> HideStoryUI:
    /// 
    /// </summary>
    public class HideStoryUIDirection : AbstractDirection
    {
        public override string DirectionName => "Hide Story UI";

        public override void Execute(string[] parameters)
        {
            if (!ValidateArgumentCount(parameters, 0, 0))
            {
                return;
            }

            StoryManager.Instance.HideUI();
        }
    }
}
