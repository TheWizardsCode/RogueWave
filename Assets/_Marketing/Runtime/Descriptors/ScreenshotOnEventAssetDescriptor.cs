using NaughtyAttributes;
using System;
using System.Collections;
using UnityEditor.Recorder.Input;
using UnityEditor.Recorder;
using UnityEngine;
using WizardsCode.RogueWave;
using Time = UnityEngine.Time;
using PlasticPipe.PlasticProtocol.Messages;

namespace WizardsCode.Marketing
{
    [CreateAssetMenu(fileName = "New Screenshot On Event Asset Descriptor", menuName = "Wizards Code/Marketing/Screenshot On Event Asset Descriptor")]
    public class ScreenshotOnEventAssetDescriptor : AssetDescriptor
    {
        [SerializeField, Tooltip("The Game Event to trigger the capture of a Screenshot."), BoxGroup("Screenshot")]
        GameEvent[] m_HeroImageGameEvents;
        [SerializeField, Tooltip("The number of screenshots to take when the game event is triggered."), BoxGroup("Screenshot")]
        int m_ScreenshotCount = 3;
        [SerializeField, Tooltip("Minimum number of frames between screenshots."), BoxGroup("Screenshot")]
        int m_MinimumFramesBetweenScreenshots = 30;
        [SerializeField, Tooltip("The target frame rate of the image sequence."), BoxGroup("Screenshot")]
        float m_FrameRate = 30;

        internal GameEvent[] ScreenshotGameEvents => m_HeroImageGameEvents;
        internal int ScreenshotCount => m_ScreenshotCount;
        private int nextPermittedScreenshotFrame = 0; internal float FrameRate => m_FrameRate;

        public string ScreenshotPath
        {
            get
            {
                return $"Assets/Recordings/{AssetName}/{SceneName}/Screenshot/";
            }
        }

        public string ScreenshotFilename
        {
            get
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                return $"{SceneName}_{AssetName}_{date}_{screenshotCount.ToString("D4")}_Screenshot_<Frame>";
            }
        }

        public override float RecordingDuration => (screenshotCount * 2) + 1 / m_FrameRate;

        /// <summary>
        /// Capture a screenshot image of the current frame.
        /// </summary>
        /// <param name="callback">This will be called when the image has been saved</param>
        /// <returns>The coroutine handling the process.</returns>
        public override IEnumerator GenerateAsset(Action callback = null)
        {
            if (nextPermittedScreenshotFrame > Time.frameCount)
            {
                yield break;
            }

            nextPermittedScreenshotFrame = Time.frameCount + m_MinimumFramesBetweenScreenshots + ScreenshotCount;
            screenshotCount++;
            yield return new WaitForEndOfFrame();
            int endFrame = Time.frameCount + ScreenshotCount;

            RecordScreenshot();

            while (Time.frameCount <= endFrame)
            {
                yield return null;
            }

            StopRecording();

            callback?.Invoke();
            StopRecording();
        }

        private void RecordScreenshot()
        {
            RecorderControllerSettings controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            controller = new RecorderController(controllerSettings);

            ImageRecorderSettings settings = ScriptableObject.CreateInstance<ImageRecorderSettings>();
            settings.name = "Screenshot Image Recorder for " + AssetName;
            settings.Enabled = true;

            settings.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
            settings.CaptureAlpha = true;

            settings.imageInputSettings = new GameViewInputSettings
            {
                OutputWidth = Resolution.x,
                OutputHeight = Resolution.y
            };

            if (ScreenshotCount > 1)
            {
                controllerSettings.SetRecordModeToFrameInterval(0, screenshotCount);
            }
            else
            {
                controllerSettings.SetRecordModeToSingleFrame(0);
            }
            controllerSettings.AddRecorderSettings(settings);

            settings.OutputFile = $"{ScreenshotPath}{ScreenshotFilename}";

            RecorderOptions.VerboseMode = true;
            controller.PrepareRecording();
            controller.StartRecording();
        }
    }
}
