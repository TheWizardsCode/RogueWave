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

        private IEnumerator Start()
        {
            NeoFpsInputManager.captureMouseCursor = true;

            foreach (AssetDescriptor assetDescriptor in assetDescriptors)
            {
                assetDescriptor.LoadSceneSetup();

                StartCoroutine(assetDescriptor.GenerateHeroFrame());
                if (assetDescriptor.GetType() != typeof(AssetDescriptor))
                {
                    StartCoroutine(assetDescriptor.GenerateRequiredAssets());
                }

                while (assetDescriptor.IsRecording)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(1);
            }

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