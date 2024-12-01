using ModelShark;
using NeoFPS;
using RogueWave.Story;
using System;
using System.Collections;
using UnityEngine;

namespace WizardsCode.Tutorial
{
    /// <summary>
    /// This is a specialist TutorialStep that pops up a tooltip when the step is executed.
    /// 
    /// It is ideal for ensuring players see something important when they first encounter a new feature.
    /// 
    /// By default this will only show once per player.
    /// </summary>
    [CreateAssetMenu(fileName = "Popup Story Beat", menuName = "Rogue Wave/Popup Story Beat", order = 1)]
    public class PopupStoryBeat : StoryBeat
    {
        [SerializeField, Tooltip("An override for the style of the tooltip to show. If this is null the style set in the TooltipManager will be used.")]
        TooltipStyle m_TooltipStyleOverride;
        [SerializeField, Tooltip("The text to display on the button that will dismiss the tooltip.")]
        string buttonText = "Continue";

        public override IEnumerator Execute()
        {
            if (IsComplete)
            {
                yield break;
            }

            GameObject tooltip = new GameObject("Story Beat Tooltip: " + displayName);
            TooltipTrigger tooltipTrigger = tooltip.AddComponent<TooltipTrigger>();

            if (m_TooltipStyleOverride == null)
            {
                tooltipTrigger.tooltipStyle = StoryManager.tutorialTooltipStyle;
            } else
            {
                tooltipTrigger.tooltipStyle = m_TooltipStyleOverride;
            }
            tooltipTrigger.tipPosition = TipPosition.CanvasTopMiddle;
            tooltipTrigger.minTextWidth = StoryManager.tutorialTooltipMinWidth;
            tooltipTrigger.maxTextWidth = StoryManager.tutorialTooltipMaxWidth;
            tooltipTrigger.staysOpen = false;
            tooltipTrigger.isBlocking = true;

            tooltipTrigger.SetText("BodyText", script);
            tooltipTrigger.SetText("ButtonText", buttonText);
            
            tooltipTrigger.Popup(Mathf.Infinity, StoryManager.gameObject);

            yield return base.Execute();
        }
    }
}
