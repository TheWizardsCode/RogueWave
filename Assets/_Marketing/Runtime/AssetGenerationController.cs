using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.Marketing
{
    public class AssetGenerationController : MonoBehaviour
    {
        [SerializeField, Tooltip("The asset descriptor for the asset to be generated.")]
        private AssetDescriptor assetDescriptor;

        private IEnumerator Start()
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

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}