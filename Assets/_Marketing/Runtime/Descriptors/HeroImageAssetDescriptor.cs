#if UNITY_EDITOR
using NaughtyAttributes;
using System;
using System.Collections;
using UnityEditor.Recorder.Input;
using UnityEditor.Recorder;
using UnityEngine;
using Time = UnityEngine.Time;

namespace WizardsCode.Marketing
{
    [CreateAssetMenu(fileName = "New Hero Image Asset Descriptor", menuName = "Wizards Code/Marketing/Hero Image Asset Descriptor")]
    public class HeroImageAssetDescriptor : AssetDescriptor
    {
        [SerializeField, Tooltip("The hero frame is the one that is most pleasing within the sequence. This frame, and one either side, will always be captured as a still, regardless of the kind of asset being generated."), BoxGroup("Hero Image")]
        int m_HeroFrame = 30;
        [SerializeField, Tooltip("How many frames to capture before and after the fixed frame hero."), BoxGroup("Hero Image")]
        int m_FramesEitherSideOfFixedFrameHero = 3;
        [SerializeField, Tooltip("The target frame rate of the image sequence."), BoxGroup("Video")]
        float m_FrameRate = 30;

        internal int HeroFrame => m_HeroFrame;
        internal int FramesEitherSideOfHero => m_FramesEitherSideOfFixedFrameHero;
        internal float FrameRate => m_FrameRate;
        
        public string HeroPath
        {
            get
            {
                return $"Assets/Recordings/{AssetName}/{SceneName}/Still/";
            }
        }

        public string HeroFilename
        {
            get
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                return $"{SceneName}_{AssetName}_{FrameRate}FPS_{date}_{heroCount.ToString("D4")}_Hero_<Frame>";
            }
        }

        public override float RecordingDuration => (m_FramesEitherSideOfFixedFrameHero * 2) + 1 / m_FrameRate;

        public override IEnumerator GenerateAsset(Action callback = null)
        {
            heroCount++;

            int targetFrame = HeroFrame + Time.frameCount;

            RecordImageSequence();

            while (controller.IsRecording())
            {
                yield return null;
            }
            StopRecording();

            callback?.Invoke();
            StopRecording();
        }

        private void RecordImageSequence()
        {
            RecorderControllerSettings controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            controller = new RecorderController(controllerSettings);

            ImageRecorderSettings settings = ScriptableObject.CreateInstance<ImageRecorderSettings>();
            settings.name = "Hero Image Recorder for " + AssetName;
            settings.Enabled = true;

            settings.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
            settings.CaptureAlpha = true;

            settings.imageInputSettings = new GameViewInputSettings
            {
                OutputWidth = Resolution.x,
                OutputHeight = Resolution.y
            };

            controllerSettings.FrameRate = FrameRate;
            controllerSettings.SetRecordModeToFrameInterval(HeroFrame - FramesEitherSideOfHero, HeroFrame + FramesEitherSideOfHero);
            controllerSettings.AddRecorderSettings(settings);

            settings.OutputFile = $"{HeroPath}{HeroFilename}";

            RecorderOptions.VerboseMode = true;
            controller.PrepareRecording();
            controller.StartRecording();
        }
    }
}
#endif