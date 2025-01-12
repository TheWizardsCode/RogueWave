using UnityEngine;
using UnityEngine.UI;

namespace WizardsCode.StoryTeller
{
    public class StoryFlowController : MonoBehaviour
    {
        StoryManager manager;

        private void Start()
        {
            manager = FindAnyObjectByType<StoryManager>();
            if (manager == null)
            {
                Debug.LogError("No StoryManager found in the scene. StoryFlowController will not work.");
                Destroy(this);
                return;
            }

            Button button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(() => AdvanceStory());
        }

        void AdvanceStory()
        {
            manager.FinishCurrentBeat();
        }
    }
}
