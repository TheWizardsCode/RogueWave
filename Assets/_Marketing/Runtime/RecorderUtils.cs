using System;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace WizardsCode.Marketing
{
    [Obsolete("The functionality of this call should move into AssetDescriptor")]
    public class RecorderUtils
    {
        private RecorderController controller;
        private RecordingSession session;
        private RecordingSession recordingSession;

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

        /// <summary>
        /// Record screenshots of the scene at the current frame. By default this will record a single screenshot.
        /// If you want more than 1 set the totalNumberOfScreenshots parameter.
        /// </summary>
        public void RecordScreenshot(ScreenshotOnEventAssetDescriptor descriptor)
        {
            RecorderControllerSettings controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            controller = new RecorderController(controllerSettings);

            ImageRecorderSettings settings = ScriptableObject.CreateInstance<ImageRecorderSettings>();
            settings.name = "Screenshot Image Recorder";
            settings.Enabled = true;

            settings.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
            settings.CaptureAlpha = true;

            settings.imageInputSettings = new GameViewInputSettings
            {
                OutputWidth = descriptor.Resolution.x,
                OutputHeight = descriptor.Resolution.y
            };

            if (descriptor.ScreenshotCount > 1)
            {
                controllerSettings.SetRecordModeToFrameInterval(0, descriptor.screenshotCount - 1);
            }
            else
            {
                controllerSettings.SetRecordModeToSingleFrame(0);
            }
            controllerSettings.AddRecorderSettings(settings);

            settings.OutputFile = $"{descriptor.ScreenshotPath}{descriptor.ScreenshotFilename}";

            RecorderOptions.VerboseMode = true;
            controller.PrepareRecording();
            controller.StartRecording();
        }

        /// <summary>
        /// Record a set of hero images of the scene as defined by the AssetDescriptor.
        /// This will usually be an image at a fixed frame, with a limited number of frames either side of the hero frame.
        /// </summary>
        /// <param name="descriptor">The descriptor of the image to capture.</param>
        public void RecordHeroImages(HeroImageAssetDescriptor descriptor)
        {
            RecorderControllerSettings controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            controller = new RecorderController(controllerSettings);

            ImageRecorderSettings settings = ScriptableObject.CreateInstance<ImageRecorderSettings>();
            settings.name = "Hero Image Recorder";
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
            
            settings.OutputFile = $"{descriptor.HeroPath}{descriptor.HeroFilename}";

            RecorderOptions.VerboseMode = true;
            controller.PrepareRecording();
            controller.StartRecording();
        }

        /// <summary>
        /// Record a video of the scene.
        /// </summary>
        /// <param name="descriptor">The descriptor for the video to record. This contains all the details needed to configure the GIF recorder.</param>
        public void RecordVideo(VideoAssetDescriptor descriptor)
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
            settings.name = "Gif Recorder for " + descriptor.AssetName;
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
            controllerSettings.SetRecordModeToTimeInterval(Time.time + descriptor.GifStartTime, descriptor.GifEndTime);
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
                controller = null;
            }
        }
    }
}