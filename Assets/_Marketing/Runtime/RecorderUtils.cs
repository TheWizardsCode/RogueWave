using Codice.Client.BaseCommands;
using System;
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
        string assetName = string.Empty;
        float frameRate = 60;

        RecorderController controller;

        public RecorderUtils(string assetName, float frameRate)
        {
            this.frameRate = frameRate;
            this.assetName = assetName;
            if (string.IsNullOrEmpty(assetName))
            {
                assetName = "Misc";
            }
        }

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
            controller = new RecorderController(ScriptableObject.CreateInstance<RecorderControllerSettings>());

            var imageRecorderSettings = ScriptableObject.CreateInstance<ImageRecorderSettings>();
            imageRecorderSettings.name = "Image Recorder";
            imageRecorderSettings.Enabled = true;

            imageRecorderSettings.OutputFile = $"{descriptor.HeroPath}{descriptor.HeroFilename}<Frame>";

            imageRecorderSettings.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
            imageRecorderSettings.imageInputSettings = new GameViewInputSettings
            {
                OutputWidth = descriptor.Resolution.x,
                OutputHeight = descriptor.Resolution.y
            };

            imageRecorderSettings.RecordMode = RecordMode.FrameInterval;
            imageRecorderSettings.FrameRate = frameRate;
            imageRecorderSettings.CapFrameRate = true;
            imageRecorderSettings.StartFrame = 0;
            imageRecorderSettings.EndFrame = 2 * descriptor.FramesEitherSideOfHero + 1;

            controller.Settings.AddRecorderSettings(imageRecorderSettings);

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
        /// Record a GIF of the scene.
        /// </summary>
        /// <param name="startTime">The time to start recording, measured from the frame in which this method is called.</param>
        /// <param name="endTime">The time to stop recording, measured from the frame in which this method is called.</param>
        /// <param name="quality">The quality of the GIF from 1 to 100, 100 being best.</param>
        /// <param name="isLooping">True if the GIF is to loop, otherwise it is a one shot.</param>
        /// <param name="resolution">The resolution in width (x) x height(y).</param>
        /// <returns>The path to the GIF file on disk.</returns>
        public void RecordGIF(GifAssetDescriptor descriptor)
        {
            controller = new RecorderController(ScriptableObject.CreateInstance<RecorderControllerSettings>());

            var movieRecorderSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            movieRecorderSettings.name = "Gif Recorder";
            movieRecorderSettings.Enabled = true;

            var gifEncoderSettings = new GifEncoderSettings
            {
                Quality = descriptor.GifQuality,
                Loop = descriptor.IsLooping
            };
            movieRecorderSettings.EncoderSettings = gifEncoderSettings;

            movieRecorderSettings.OutputFile = descriptor.GifPath + descriptor.GifFilename;

            movieRecorderSettings.ImageInputSettings = new GameViewInputSettings
            {
                OutputWidth = descriptor.Resolution.x,
                OutputHeight = descriptor.Resolution.y
            };

            movieRecorderSettings.RecordMode = RecordMode.TimeInterval;
            movieRecorderSettings.FrameRate = frameRate;
            movieRecorderSettings.CapFrameRate = true;
            movieRecorderSettings.StartTime = Time.time + descriptor.StartTime;
            movieRecorderSettings.EndTime = descriptor.EndTime;

            controller.Settings.AddRecorderSettings(movieRecorderSettings);

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