using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WizardsCode.Marketing
{
    /// <summary>
    /// Provides convenience methods for working with the Unity Recorder.
    /// </summary>
    public class RecorderUtils
    {
        private RecorderController controller;

        public bool IsRecording
        {
            get
            {
                if (controller == null)
                {
                    return false;
                }
                return controller.IsRecording();
            }
        }

        public void RecordImages(AssetDescriptor descriptor)
        {
            RecorderControllerSettings controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            controller = new RecorderController(controllerSettings);

            ImageRecorderSettings settings = ScriptableObject.CreateInstance<ImageRecorderSettings>();
            settings.name = "Image Recorder";
            settings.Enabled = true;

            settings.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
            settings.CaptureAlpha = true;

            settings.imageInputSettings = new GameViewInputSettings
            {
                OutputWidth = descriptor.Resolution.x,
                OutputHeight = descriptor.Resolution.y
            };

            controllerSettings.FrameRate = descriptor.FrameRate;
            controllerSettings.SetRecordModeToFrameInterval(0, 2 * descriptor.FramesEitherSideOfHero + 1); // start is 0 as we only start this recorder when at the right frame
            controllerSettings.AddRecorderSettings(settings);
            
            settings.OutputFile = $"{descriptor.HeroPath}{descriptor.HeroFilename}<Frame>";

            RecorderOptions.VerboseMode = false;
            controller.PrepareRecording();
            controller.StartRecording();
        }

        private static string GetSceneName()
        {
            string sceneName = "Unknown Scene";
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && scene.name != "Stage")
                {
                    sceneName = scene.name;
                }
            }

            return sceneName;
        }

        /// <summary>
        /// Record a video of the scene.
        /// </summary>
        /// <param name="descriptor">The descriptor for the video to record. This contains all the details needed to configure the GIF recorder.</param>
        public void RecordVideo(AssetDescriptor descriptor)
        {
            RecorderControllerSettings controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            controller = new RecorderController(controllerSettings);

            MovieRecorderSettings settings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            settings.name = "Video Recorder";
            settings.Enabled = true;

            settings.EncoderSettings = new CoreEncoderSettings
            {
                EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High,
                Codec = CoreEncoderSettings.OutputCodec.WEBM
            };
            settings.CaptureAlpha = true;

            settings.ImageInputSettings = new GameViewInputSettings
            {
                OutputWidth = descriptor.Resolution.x,
                OutputHeight = descriptor.Resolution.y,
                FlipFinalOutput = false,
            };

            controllerSettings.SetRecordModeToManual();
            controllerSettings.FrameRate = descriptor.FrameRate;
            controllerSettings.AddRecorderSettings(settings);

            settings.OutputFile = descriptor.VideoPath + descriptor.VideoFilename;

            RecorderOptions.VerboseMode = false;
            controller.PrepareRecording();
            controller.StartRecording();
        }

        /// <summary>
        /// Record a GIF of the scene.
        /// </summary>
        /// <param name="descriptor">The descriptor for the GIF to record. This contains all the details needed to configure the GIF recorder.</param>
        public void RecordGIF(GifAssetDescriptor descriptor)
        {
            RecorderControllerSettings controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            controller = new RecorderController(controllerSettings);

            MovieRecorderSettings settings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            settings.name = "Gif Recorder";
            settings.Enabled = true;

            settings.EncoderSettings = new GifEncoderSettings
            {
                Quality = descriptor.GifQuality,
                Loop = descriptor.IsLooping
            };
            settings.CaptureAlpha = true;

            settings.ImageInputSettings = new GameViewInputSettings
            {
                OutputWidth = descriptor.Resolution.x,
                OutputHeight = descriptor.Resolution.y
            };

            settings.OutputFile = descriptor.GifPath + descriptor.GifFilename;

            controllerSettings.AddRecorderSettings(settings);
            controllerSettings.SetRecordModeToTimeInterval(Time.time + descriptor.StartTime, descriptor.EndTime);
            controllerSettings.FrameRate = descriptor.FrameRate;

            RecorderOptions.VerboseMode = false;
            controller.PrepareRecording();
            controller.StartRecording();
        }

        internal void StopRecording()
        {
            if (controller.IsRecording())
            {
                controller.StopRecording();
            }
            else
            {
                Debug.LogWarning("Attempted to stop recording, but the encoder was not recording.");
            }
        }
    }
}