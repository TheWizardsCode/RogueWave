using NaughtyAttributes;
using NeoFPS;
using System.Collections;
using UnityEditor;
using UnityEngine;

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
            NeoFpsInputManager.captureMouseCursor = true;

            foreach (AssetDescriptor assetDescriptor in assetDescriptors)
            {
                assetDescriptor.LoadSceneSetup();

                if (assetDescriptor is ScreenshotOnEventAssetDescriptor screenshotOnEventAssetDescriptor)
                {
                    screenshotOnEventAssetDescriptor.ScreenshotGameEvent.RegisterListener(() =>
                    {
                        StartCoroutine(screenshotOnEventAssetDescriptor.GenerateAsset());
                    });
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