using System;
using System.Collections;
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

        private float sessionStartTime = 0;

        public override IEnumerator GenerateAsset(Action callback = null)
        {
            IsRecording = true;
            sessionStartTime = Time.time;

            RecorderUtils recorder = new RecorderUtils(AssetName, FrameRate);
            recorder.RecordGIF(sessionStartTime + StartTime, sessionStartTime + EndTime, GifQuality, IsLooping, Resolution);

            while (Time.time - sessionStartTime <= EndTime)
            {
                yield return null;
            }

            recorder.StopRecording();

            IsRecording = false;
            callback?.Invoke();
        }

    }
}
