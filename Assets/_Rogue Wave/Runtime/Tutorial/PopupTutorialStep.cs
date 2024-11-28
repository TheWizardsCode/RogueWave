using ModelShark;
using NeoFPS;
using RogueWave.Tutorial;
using System;
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
    [CreateAssetMenu(fileName = "Popup Tutorial Step", menuName = "Rogue Wave/Popup Tutorial Step", order = 1)]
    public class PopupTutorialStep : TutorialStep
    {
        [SerializeField, Tooltip("An override for the style of the tooltip to show. If this is null the style set in the TooltipManager will be used.")]
        TooltipStyle m_TooltipStyleOverride;
        [SerializeField, Tooltip("The text to display on the button that will dismiss the tooltip.")]
        string buttonText = "Continue";

        public override void Execute()
        {
            GameObject tooltip = new GameObject("TutorialTooltip " + displayName);
            TooltipTrigger tooltipTrigger = tooltip.AddComponent<TooltipTrigger>();

            if (m_TooltipStyleOverride == null)
            {
                tooltipTrigger.tooltipStyle = TutorialManager.tutorialTooltipStyle;
            } else
            {
                tooltipTrigger.tooltipStyle = m_TooltipStyleOverride;
            }
            tooltipTrigger.tipPosition = TipPosition.CanvasTopMiddle;
            tooltipTrigger.minTextWidth = TutorialManager.tutorialTooltipMinWidth;
            tooltipTrigger.maxTextWidth = TutorialManager.tutorialTooltipMaxWidth;
            tooltipTrigger.staysOpen = false;

            tooltipTrigger.SetText("BodyText", script);
            tooltipTrigger.SetText("ButtonText", buttonText);
            
            tooltipTrigger.Popup(Mathf.Infinity, TutorialManager.gameObject);
        }
    }
}
