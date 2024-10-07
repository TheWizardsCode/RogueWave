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

        public void RecordImages(Vector2Int resolution, int numberOfFrames)
        {
            controller = new RecorderController(ScriptableObject.CreateInstance<RecorderControllerSettings>());

            var imageRecorderSettings = ScriptableObject.CreateInstance<ImageRecorderSettings>();
            imageRecorderSettings.name = "Image Recorder";
            imageRecorderSettings.Enabled = true;

            string sceneName = GetSceneName();
            string date = DateTime.Now.ToString("yyyyMMdd");
            string time = DateTime.Now.ToString("HHmmss");
            imageRecorderSettings.OutputFile = $"Assets/Recordings/{assetName}/{sceneName}/Still/{assetName}_{sceneName}_{date}_{time}_Hero<Frame>";

            imageRecorderSettings.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
            imageRecorderSettings.imageInputSettings = new GameViewInputSettings
            {
                OutputWidth = resolution.x,
                OutputHeight = resolution.y
            };

            imageRecorderSettings.RecordMode = RecordMode.FrameInterval;
            imageRecorderSettings.FrameRate = frameRate;
            imageRecorderSettings.CapFrameRate = true;
            imageRecorderSettings.StartFrame = 0;
            imageRecorderSettings.EndFrame = 2 * (numberOfFrames - 1) + 1;

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

        public void RecordGIF(float startTime, float endTime, uint quality, bool isLooping, Vector2Int resolution)
        {
            controller = new RecorderController(ScriptableObject.CreateInstance<RecorderControllerSettings>());

            var movieRecorderSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            movieRecorderSettings.name = "Gif Recorder";
            movieRecorderSettings.Enabled = true;

            var gifEncoderSettings = new GifEncoderSettings
            {
                Quality = quality,
                Loop = isLooping
            };
            movieRecorderSettings.EncoderSettings = gifEncoderSettings;

            string sceneName = GetSceneName();
            string date = DateTime.Now.ToString("yyyyMMdd");
            string time = DateTime.Now.ToString("HHmmss");
            movieRecorderSettings.OutputFile = $"Assets/Recordings/{assetName}/{sceneName}/GIF/{sceneName}_{date}_{time}";

            movieRecorderSettings.ImageInputSettings = new GameViewInputSettings
            {
                OutputWidth = resolution.x,
                OutputHeight = resolution.y
            };

            movieRecorderSettings.RecordMode = RecordMode.TimeInterval;
            movieRecorderSettings.FrameRate = frameRate;
            movieRecorderSettings.CapFrameRate = true;
            movieRecorderSettings.StartTime = startTime;
            movieRecorderSettings.EndTime = endTime;

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