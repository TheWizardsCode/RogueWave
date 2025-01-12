//using ModelShark;
//using RogueWave.Story;
//using System.Collections;
//using UnityEngine;

//namespace WizardsCode.StoryTeller
//{
//    /// <summary>
//    /// This is a specialist Story Beat that will become enables when a specific game state is reached.
//    /// 
//    /// It is ideal for ensuring players see something important when they first encounter a new feature.
//    /// 
//    /// By default this will only show once per player.
//    /// </summary>
//    [CreateAssetMenu(fileName = "Popup Story Beat", menuName = "Rogue Wave/Popup Story Beat", order = 1)]
//    public class PopupStoryBeat : StoryBeat
//    {
//        [SerializeField, Tooltip("An override for the style of the tooltip to show. If this is null the style set in the TooltipManager will be used.")]
//        TooltipStyle m_TooltipStyleOverride;
//        [SerializeField, Tooltip("The text to display on the button that will dismiss the tooltip.")]
//        string buttonText = "Continue";

//        public override IEnumerator Execute()
//        {
//            if (IsComplete)
//            {
//                yield break;
//            }

//            GameObject tooltip = new GameObject("Story Beat Tooltip: " + displayName);
//            TooltipTrigger tooltipTrigger = tooltip.AddComponent<TooltipTrigger>();

//            if (m_TooltipStyleOverride == null)
//            {
//                tooltipTrigger.tooltipStyle = StoryManager.defaultStoryTooltipStyle;
//            } else
//            {
//                tooltipTrigger.tooltipStyle = m_TooltipStyleOverride;
//            }
//            tooltipTrigger.tipPosition = TipPosition.CanvasTopMiddle;
//            tooltipTrigger.minTextWidth = StoryManager.defaultStoryTooltipMinWidth;
//            tooltipTrigger.maxTextWidth = StoryManager.defaultStoryTooltipMaxWidth;
//            tooltipTrigger.staysOpen = true;
//            tooltipTrigger.isBlocking = true;

//            tooltipTrigger.SetText("BodyText", script);
//            tooltipTrigger.SetText("ButtonText", buttonText);
            
//            tooltipTrigger.Popup(Mathf.Infinity, StoryManager.gameObject);

//            yield return base.Execute();
//        }

//        private void OnValidate()
//        {
//            float longestClip = 0;
//            foreach (AudioClip clip in audioClips)
//            {
//                if (clip.length > longestClip)
//                {
//                    longestClip = clip.length;
//                }
//            }

//            if (duration < longestClip + 0.5)
//            {
//                duration = longestClip + 0.5f;
//            }
//        }
//    }
//}
