using System;
using System.Collections;
using System.Collections.Generic;
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
        float frameRate = 60;

        RecorderController controller;

        public RecorderUtils(float frameRate)
        {
            this.frameRate = frameRate;
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

        void RecordVideo()
        {
            RecorderControllerSettings settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();

            settings.FrameRate = frameRate;
           
            controller = new RecorderController(settings);
        }

        public void RecordThreeFrames(Vector2Int resolution)
        {
            controller = new RecorderController(ScriptableObject.CreateInstance<RecorderControllerSettings>());

            var imageRecorderSettings = ScriptableObject.CreateInstance<ImageRecorderSettings>();
            imageRecorderSettings.name = "Image Recorder";
            imageRecorderSettings.Enabled = true;

            string sceneName = SceneManager.GetActiveScene().name;
            string dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            imageRecorderSettings.OutputFile = $"Assets/Recordings/Stills/{sceneName}_{dateTime}_Hero<Frame>";

            imageRecorderSettings.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
            imageRecorderSettings.imageInputSettings = new GameViewInputSettings
            {
                OutputWidth = resolution.x,
                OutputHeight = resolution.y
            };

            imageRecorderSettings.RecordMode = RecordMode.FrameInterval;
            imageRecorderSettings.FrameRate = frameRate;
            imageRecorderSettings.StartFrame = 0;
            imageRecorderSettings.EndFrame = 3;

            controller.Settings.AddRecorderSettings(imageRecorderSettings);

            controller.PrepareRecording();
            controller.StartRecording();
        }

        public void RecordGIF(int startFrame, int endFrame, uint quality, bool isLooping, Vector2Int resolution)
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

            string sceneName = SceneManager.GetActiveScene().name;
            string dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            movieRecorderSettings.OutputFile = $"Assets/Recordings/Gifs/{sceneName}_{dateTime}";

            movieRecorderSettings.ImageInputSettings = new GameViewInputSettings
            {
                OutputWidth = resolution.x,
                OutputHeight = resolution.y
            };

            movieRecorderSettings.RecordMode = RecordMode.FrameInterval;
            movieRecorderSettings.StartFrame = startFrame;
            movieRecorderSettings.EndFrame = endFrame;

            controller.Settings.AddRecorderSettings(movieRecorderSettings);

            controller.PrepareRecording();
            controller.StartRecording();
        }

        internal void StopRecording()
        {
            controller.StopRecording();
        }
    }
}