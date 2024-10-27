#if UNITY_EDITOR
using NaughtyAttributes;
using System;
using System.Collections;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEditor.Recorder;
using UnityEngine;
using Time = UnityEngine.Time;

namespace WizardsCode.Marketing
{
    [CreateAssetMenu(fileName = "New Video Asset Descriptor", menuName = "Wizards Code/Marketing/Video Asset Descriptor")]
    public class VideoAssetDescriptor : AssetDescriptor
    {
        [SerializeField, Tooltip("The time, in game time, when the video recording starts. You should leave enough frames for the scene to \"warm up\"."), BoxGroup("Video")]
        float m_VideoStartTime = 0;
        [SerializeField, Tooltip("The time, in game time, when the video recording stops. If this is 0 then the recording will only stop when the application stops."), BoxGroup("Video")]
        float m_VideoEndTime = 30;
        [SerializeField, Tooltip("The target frame rate of the video."), BoxGroup("Video")]
        float m_FrameRate = 30;

        internal float VideoStartTime => m_VideoStartTime;
        internal float VideoEndTime => m_VideoEndTime;
        internal float FrameRate => m_FrameRate;

        public override bool IsRecording
        {
            get
            {
                if (recorder == null)
                {
                    return false;
                }

                return sessionStartTime + Time.time <= VideoEndTime;
            }
        }

        public string VideoPath
        {
            get
            {
                return $"Assets/Recordings/{AssetName}/{SceneName}/Video/";
            }
        }

        public string VideoFilename
        {
            get
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                return $"{SceneName}_{AssetName}_{date}_{videoCount.ToString("D4")}";
            }
        }

        public override float RecordingDuration => m_VideoEndTime - m_VideoStartTime;

        public override IEnumerator GenerateAsset(Action callback = null)
        {
            videoCount++;
            recordingStoppedCallback = callback;

            sessionStartTime = Time.time;

            RecordVideo();

            while (controller.IsRecording())
            {
                yield return null;
            }
            StopRecording();

            callback?.Invoke();
            StopRecording();
        }

        private void RecordVideo()
        {
            RecorderControllerSettings controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            controller = new RecorderController(controllerSettings);

            MovieRecorderSettings settings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            settings.name = "Video Recorder for " + AssetName;
            settings.Enabled = true;

            settings.EncoderSettings = new CoreEncoderSettings
            {
                EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High,
                Codec = CoreEncoderSettings.OutputCodec.WEBM
            };
            settings.CaptureAlpha = true;

            settings.ImageInputSettings = new GameViewInputSettings
            {
                OutputWidth = Resolution.x,
                OutputHeight = Resolution.y,
                FlipFinalOutput = false,
            };

            controllerSettings.SetRecordModeToManual();
            controllerSettings.FrameRate = FrameRate;
            controllerSettings.AddRecorderSettings(settings);

            settings.OutputFile = VideoPath + VideoFilename;

            RecorderOptions.VerboseMode = false;
            controller.PrepareRecording();
            controller.StartRecording();
        }
    }
}
#endif