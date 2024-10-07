using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace WizardsCode.Marketing
{
    public class AssetDescriptor : ScriptableObject
    {
        [Header("Metadata")]
        [SerializeField, Tooltip("A name to be used in filenames and other references."), FormerlySerializedAs("assetName")]
        string m_AssetName;
        [SerializeField, Tooltip("The description of the asset, used as a reminder for why this is needed."), TextArea(10,25), FormerlySerializedAs("description")]
        string m_Description;

        [Header("Asset Settings")]
        [SerializeField, Tooltip("The resolution of the asset in pixels.")]
        Vector2Int m_Resolution = new Vector2Int(1920, 1080);
        [SerializeField, Tooltip("The target frame rate for the asset. If the asset is only a helo image then this is the frame rate of the rendering of frames around the hero frame.")]
        float m_FrameRate = 60;
        [SerializeField, Tooltip("The hero frame is the one that is most pleasing within the sequence. This frame, and one either side, will always be captured as a still, regardless of the kind of asset being generated.")]
        int m_HeroFrame = 30;
        [SerializeField, Tooltip("The first frame to record, you should leave enough frames for the scene to \"warm up\".")]
        int m_StartFrame = 1;
        [SerializeField, Tooltip("The last frame to record.")]
        int m_EndFrame = 60;

        internal string AssetName => m_AssetName;
        internal string Description => m_Description;
        internal Vector2Int Resolution => m_Resolution;
        internal float FrameRate => m_FrameRate;
        internal int HeroFrame => m_HeroFrame;
        internal int StartFrame => m_StartFrame;
        internal int EndFrame => m_EndFrame;
        
        public bool IsRecording { get; set; }

        public virtual IEnumerator GenerateAsset()
        {
            yield return GenerateHeroFrame();
        }

        public IEnumerator GenerateHeroFrame()
        {
            IsRecording = true;

            RecorderUtils recorder = new RecorderUtils(FrameRate);

            while (Time.frameCount < HeroFrame - 2)
            {
                Debug.Log("Waiting for hero frame: " + Time.frameCount);
                yield return null;
            }

            recorder.RecordThreeFrames(Resolution);
            yield return recorder;

            while (Time.frameCount < HeroFrame + 2)
            {
                Debug.Log("Recording hero frame: " + Time.frameCount);
                yield return null;
            }

            recorder.StopRecording();

            // if this is a subclass of AssetDescriptor then we need to wait for the subclass to finish recording, but if it is an AssetDescriptor then we can stop now.
            if (GetType() != typeof(AssetDescriptor))
            {
                while (IsRecording)
                {
                    yield return null;
                }
            }
            IsRecording = false;
        }
    }
}