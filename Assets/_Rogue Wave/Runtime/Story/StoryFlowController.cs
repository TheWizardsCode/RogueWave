using ModelShark;
using UnityEngine;
using UnityEngine.UI;

namespace RogueWave.Story
{
    public class StoryFlowController : MonoBehaviour
    {
        private void Start()
        {
            Button button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(() => AdvanceStory());
        }

        void AdvanceStory()
        {
            TooltipManager.Instance.HideAll();
            FindObjectOfType<StoryManager>().FinishCurrentBeat();
        }
    }
}
