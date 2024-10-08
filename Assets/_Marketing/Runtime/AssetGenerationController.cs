using System.Collections;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.Marketing
{
    public class AssetGenerationController : MonoBehaviour
    {
        [SerializeField, Tooltip("The asset descriptor for the asset to be generated.")]
        private AssetDescriptor[] assetDescriptors;

        private IEnumerator Start()
        {
            foreach (AssetDescriptor assetDescriptor in assetDescriptors)
            {
                StartCoroutine(assetDescriptor.GenerateHeroFrame());
                if (assetDescriptor.GetType() != typeof(AssetDescriptor))
                {
                    StartCoroutine(assetDescriptor.GenerateAsset());
                }

                while (assetDescriptor.IsRecording)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(1);
            }

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}