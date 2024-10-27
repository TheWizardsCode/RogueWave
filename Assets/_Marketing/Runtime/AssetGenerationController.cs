#if UNITY_EDITOR
using NaughtyAttributes;
using NeoFPS;
using System.Collections;
using UnityEditor;
using UnityEngine;
using WizardsCode.RogueWave;

namespace WizardsCode.Marketing
{
    [DefaultExecutionOrder(-1000)]
    public class AssetGenerationController : MonoBehaviour
    {
        [SerializeField, Tooltip("If true the application will quit when all assets have been generated.")]
        private bool stopOnAssetCompletion = true;
        [SerializeField, Tooltip("The asset descriptor for the asset to be generated."), Expandable]
        private AssetDescriptor[] assetDescriptors;

        private void OnDisable()
        {
            AssetDatabase.Refresh();
        }

        private IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);

            yield return null;
            NeoFpsInputManager.captureMouseCursor = true;

            foreach (AssetDescriptor assetDescriptor in assetDescriptors)
            {
                assetDescriptor.LoadSceneSetup();

                if (assetDescriptor is ScreenshotOnEventAssetDescriptor eventDescriptor)
                {
                    foreach (GameEvent gameEvent in eventDescriptor.ScreenshotGameEvents)
                    {
                        var currentDescriptor = eventDescriptor;
                        gameEvent.RegisterListener(() =>
                        {
                            if (currentDescriptor != null && this != null) // TODO: this is here to stop concurrency errors, but it's not ideal it means we aren't grabbing some shots. Why does this happen at all?
                            {
                                StartCoroutine(currentDescriptor.GenerateAsset());
                            }
                        });
                    }
                }
                else
                {
                    StartCoroutine(assetDescriptor.GenerateAsset());
                }

                yield return new WaitForSeconds(assetDescriptor.RecordingDuration + 1);

                yield return new WaitUntil(() => assetDescriptor.IsRecording == false);
            }

            AssetDatabase.Refresh();

            if (stopOnAssetCompletion)
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            }
        }
    }
}
#endif