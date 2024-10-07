using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Marketing
{
    [CreateAssetMenu(fileName = "New Gif Asset Descriptor", menuName = "Wizards Code/Marketing/Gif Asset Descriptor")]
    public class GifAssetDescriptor : AssetDescriptor
    {
        [Header("Gif Settings")]
         [SerializeField, Tooltip("The quality of the gif. From 1 to 100.")]
        uint m_GifQuality = 90;
        [SerializeField, Tooltip("Whether the GIF is a loop (true) or a single shot (false).")]
        bool m_IsLooping = true;
        
        protected uint GifQuality => m_GifQuality;
        protected bool IsLooping => m_IsLooping;

        public override IEnumerator GenerateAsset()
        {
            IsRecording = true;

            RecorderUtils recorder = new RecorderUtils(FrameRate);
            recorder.RecordGIF(StartFrame, EndFrame, GifQuality, IsLooping, Resolution);

            while (Time.frameCount <= EndFrame)
            {
                yield return null;
            }

            recorder.StopRecording();

            IsRecording = false;
        }

    }
}
