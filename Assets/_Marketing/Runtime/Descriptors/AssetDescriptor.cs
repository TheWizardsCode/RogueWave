using NaughtyAttributes;
using Steamworks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using WizardsCode.RogueWave;

namespace WizardsCode.Marketing
{
    [CreateAssetMenu(fileName = "New Asset Descriptor", menuName = "Wizards Code/Marketing/Generic Asset Descriptor")]
    public class AssetDescriptor : ScriptableObject
    {
        //[Header("Metadata")]
        [SerializeField, Tooltip("A name to be used in filenames and other references."), BoxGroup("Metadata"), FormerlySerializedAs("assetName")]
        string m_AssetName;
        [SerializeField, Tooltip("The description of the asset, used as a reminder for why this is needed."), TextArea(3,8), BoxGroup("Metadata"), FormerlySerializedAs("description")]
        string m_Description;

        [HorizontalLine(color: EColor.Gray)]

        // Hero Images
        [SerializeField, Tooltip("Capture a hero image on a fixed frame?"), FormerlySerializedAs("m_CaptureHeroImage"), BoxGroup("Hero Image")]
        bool m_CaptureHeroImageOnFixedFrame = false;
        [SerializeField, Tooltip("The hero frame is the one that is most pleasing within the sequence. This frame, and one either side, will always be captured as a still, regardless of the kind of asset being generated."), BoxGroup("Hero Image"), ShowIf("m_CaptureHeroImageOnFixedFrame")]
        int m_HeroFrame = 30;
        [SerializeField, Tooltip("How many frames to capture before and after the fixed frame hero."), BoxGroup("Hero Image"), ShowIf("m_CaptureHeroImageOnFixedFrame")]
        int m_FramesEitherSideOfFixedFrameHero = 3;
        [SerializeField, Tooltip("Capture a hero image on a Game Event?"), BoxGroup("Hero Image")]
        bool m_CaptureScreenshotOnGameEvent = false;
        [SerializeField, Tooltip("The Game Event to trigger the capture of a hero image."), BoxGroup("Hero Image"), ShowIf("m_CaptureScreenshotOnGameEvent")]
        GameEvent m_HeroImageGameEvent;
        [SerializeField, Tooltip("The number of screenshots to take when the game event is triggered."), BoxGroup("Hero Image"), ShowIf("m_CaptureScreenshotOnGameEvent")]
        int m_ScreenshotCount = 3;
        [SerializeField, Tooltip("Minimum number of frames between screenshots."), BoxGroup("Hero Image"), ShowIf("m_CaptureScreenshotOnGameEvent")]
        int m_MinimumFramesBetweenScreenshots = 30;

        [SerializeField, Tooltip("Capture an image sequence?"), BoxGroup("Image Sequence")]
        bool m_CaptureImageSequence = false;
        [SerializeField, Tooltip("The time, in game time, when the image sequence recording starts. You should leave enough frames for the scene to \"warm up\"."), BoxGroup("Image Sequence"), ShowIf("m_CaptureImageSequence")]
        float m_ImageSequenceStartTime = 0;
        [SerializeField, Tooltip("The time, in game time, when the image sequence recording stops. If this is 0 then the recording will only stop when the application stops."), BoxGroup("Image Sequence"), ShowIf("m_CaptureImageSequence")]
        float m_ImageSequenceEndTime = 30;

        [SerializeField, Tooltip("Capture a video?"), BoxGroup("Video")]
        bool m_CaptureVideo = true;
        [SerializeField, Tooltip("The time, in game time, when the video recording starts. You should leave enough frames for the scene to \"warm up\"."), BoxGroup("Video"), ShowIf("m_CaptureVideo")]
        float m_VideoStartTime = 0;
        [SerializeField, Tooltip("The time, in game time, when the video recording stops. If this is 0 then the recording will only stop when the application stops."), BoxGroup("Video"), ShowIf("m_CaptureVideo")]
        float m_VideoEndTime = 30;

        // Resolution
        [SerializeField, Tooltip("The resolution of all the assets in pixels."), ShowIf(EConditionOperator.Or, "m_CaptureHeroImageOnFixedFrame", "m_CaptureScreenshotOnGameEvent", "m_CaptureImageSequence", "m_CaptureVideo"), BoxGroup("Resolution")]
        Vector2Int m_Resolution = new Vector2Int(1920, 1080);
        [SerializeField, Tooltip("The target frame rate for the recorders used to capture these assets. A common frame rate must be used across all assets, so set this to the highest rate needed."), ShowIf(EConditionOperator.Or, "m_CaptureHeroImageOnFixedFrame", "m_CaptureScreenshotOnGameEvent", "m_CaptureImageSequence", "m_CaptureVideo"), BoxGroup("Resolution")]
        float m_FrameRate = 30;

        //[Header("Scene Setup")]
        [SerializeField, Tooltip("The camera position for the asset capture."), Foldout("Scene Setup")]
        Vector3 m_CameraPosition = Vector3.zero;
        [SerializeField, Tooltip("The camera rotation for the asset capture."), Foldout("Scene Setup")]
        Vector3 m_CameraRotation;
        [SerializeField, Tooltip("The logo enabled state for the asset capture."), Foldout("Scene Setup")]
        bool m_LogoEnabled = true;
        [SerializeField, Tooltip("The logo position for the asset capture."), Foldout("Scene Setup"), ShowIf("m_LogoEnabled")]
        Vector3 m_LogoPosition;
        [SerializeField, Tooltip("The logo rotation for the asset capture."), Foldout("Scene Setup"), ShowIf("m_LogoEnabled")]
        Vector3 m_LogoRotation;
        [SerializeField, Tooltip("The fog enabled state for the asset capture."), Foldout("Scene Setup")]
        private bool m_FogEnabled = true;
        [SerializeField, Tooltip("The posisition of the background during this asset capture."), Foldout("Scene Setup")]
        Vector3 m_BackgroundPosition;
        [SerializeField, Tooltip("The rotation of the background during this asset capture."), Foldout("Scene Setup")]
        Vector3 m_BackgroundRotation;
        [SerializeField, HideInInspector]
        internal int heroCount = 0;
        [SerializeField, HideInInspector]
        internal int screenshotCount = 0;
        [SerializeField, HideInInspector]
        internal int imageSequenceCount = 0;
        [SerializeField, HideInInspector]
        internal int videoCount = 0;

        internal string AssetName => m_AssetName;
        internal string Description => m_Description;
        internal Vector2Int Resolution => m_Resolution;
        internal float FrameRate => m_FrameRate;

        internal bool CaptureHeroImageOnFixedFrame => m_CaptureHeroImageOnFixedFrame;
        internal int HeroFrame => m_HeroFrame;
        internal int FramesEitherSideOfHero => m_FramesEitherSideOfFixedFrameHero;
        
        internal bool CaptureScreenshotOnGameEvent => m_CaptureScreenshotOnGameEvent;
        internal GameEvent ScreenshotGameEvent => m_HeroImageGameEvent;
        internal int ScreenshotCount => m_ScreenshotCount;
        
        internal bool CaptureImageSequence => m_CaptureImageSequence;
        internal float ImageSequenceStartTime => m_ImageSequenceStartTime;
        internal float ImageSequenceEndTime => m_ImageSequenceEndTime;
        internal bool CaptureVideo => m_CaptureVideo;
        internal float VideoStartTime => m_VideoStartTime;
        internal float VideoEndTime => m_VideoEndTime;

        private RecorderUtils videoRecorder;
        private Action videoStoppedCallback;
        private float videoSessionStartTime = 0;

        private RecorderUtils imageSequenceRecorder;
        private Action imageSequenceStoppedCallback;
        private float imageSequenceSessionStartTime = 0;
        private int nextPermittedScreenshotFrame = 0;

        int m_ActiveRecordings = 0;
        public bool IsRecording {
            get
            {
                return m_ActiveRecordings > 0;
            } 
            set
            {
                if (value)
                {
                    m_ActiveRecordings++;
                } else
                {
                    m_ActiveRecordings--;
                }
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

        public string ImageSequencePath
        {
            get
            {
                return $"Assets/Recordings/{AssetName}/{SceneName}/Images/";
            }
        }

        public string ImageSequenceFilename
        {
            get
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                return $"{SceneName}_{AssetName}_{FrameRate}FPS_{date}_{imageSequenceCount.ToString("D4")}_<Frame>";
            }
        }

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

        protected string SceneName
        {
            get
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
        }

        private void OnDisable()
        {
            if (videoRecorder != null && IsRecording)
            {
                videoRecorder.StopRecording();
                IsRecording = false;
                videoStoppedCallback?.Invoke();
            }
        }

        public virtual IEnumerator GenerateRequiredAssets(Action callback = null)
        {
            // Start generation coroutines in parallel
            var videoCoroutine = CoroutineHelper.Instance.StartCoroutine(GenerateVideo(callback));
            var heroFrameCoroutine = CoroutineHelper.Instance.StartCoroutine(GenerateHeroAtFixedFrame(callback));
            var imageSequenceCoroutine = CoroutineHelper.Instance.StartCoroutine(GenerateImageSequence(callback));

            // Wait for both coroutines to complete
            yield return videoCoroutine;
            yield return heroFrameCoroutine;
            yield return imageSequenceCoroutine;
        }

        public virtual IEnumerator GenerateVideo(Action callback = null)
        {
            if (!CaptureVideo)
            {
                yield break;
            }

            videoCount++;
            videoStoppedCallback = callback;

            IsRecording = true;
            videoSessionStartTime = Time.time;

            videoRecorder = new RecorderUtils();
            videoRecorder.RecordVideo(this);

            while (VideoEndTime == 0 || Time.time - videoSessionStartTime <= VideoEndTime)
            {
                yield return null;
            }

            videoRecorder.StopRecording();
            IsRecording = false;
            videoStoppedCallback?.Invoke();
        }

        public virtual IEnumerator GenerateImageSequence(Action callback = null)
        {
            if (!CaptureImageSequence)
            {
                yield break;
            }

            imageSequenceCount++;
            imageSequenceStoppedCallback = callback;

            IsRecording = true;
            imageSequenceSessionStartTime = Time.time;

            imageSequenceRecorder = new RecorderUtils();
            imageSequenceRecorder.RecordImageSequence(this);

            while (ImageSequenceEndTime == 0 || Time.time - imageSequenceSessionStartTime <= ImageSequenceEndTime)
            {
                yield return null;
            }

            imageSequenceRecorder.StopRecording();
            IsRecording = false;
            imageSequenceStoppedCallback?.Invoke();
        }

        /// <summary>
        /// Capture a screenshot image of the current frame.
        /// </summary>
        /// <param name="callback">This will be called when the image has been saved</param>
        /// <returns>The coroutine handling the process.</returns>
        public virtual IEnumerator GenerateScreenshot(Action callback = null)
        {
            if (!CaptureScreenshotOnGameEvent || nextPermittedScreenshotFrame > Time.frameCount )
            {
                yield break;
            }
            Debug.Log("Screenshot triggered");

            nextPermittedScreenshotFrame = Time.frameCount + m_MinimumFramesBetweenScreenshots + ScreenshotCount;
            screenshotCount++;
            IsRecording = true;
            yield return new WaitForEndOfFrame();

            RecorderUtils recorder = new RecorderUtils();
            recorder.RecordScreenshot(this);
            yield return recorder;

            int frameCount = ScreenshotCount;
            while (frameCount >= 0)
            {
                yield return null;
                frameCount--;
            }

            recorder.StopRecording();

            callback?.Invoke();
        }

        public virtual IEnumerator GenerateHeroAtFixedFrame(Action callback = null)
        {
            if (!m_CaptureHeroImageOnFixedFrame)
            {
                yield break;
            }

            heroCount++;

            IsRecording = true;
            int targetFrame = HeroFrame + Time.frameCount;

            RecorderUtils recorder = new RecorderUtils();

            while (Time.frameCount <= targetFrame - m_FramesEitherSideOfFixedFrameHero)
            {
                yield return null;
            }

            recorder.RecordHeroImages(this);
            yield return recorder;

            while (Time.frameCount < targetFrame + (2 * m_FramesEitherSideOfFixedFrameHero))
            {
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

            callback?.Invoke();
        }

        [Button()]
        public virtual void LoadSceneSetup()
        {
            Camera.main.transform.position = m_CameraPosition;
            Camera.main.transform.eulerAngles = m_CameraRotation;

            LogoController logoController = FindObjectOfType<LogoController>(true);
            if (logoController != null)
            {
                logoController.gameObject.SetActive(m_LogoEnabled);
                logoController.transform.position = m_LogoPosition;
                logoController.transform.eulerAngles = m_LogoRotation;
            }

            FogController fogController = FindObjectOfType<FogController>(true);
            if (fogController != null)
            {
                fogController.gameObject.SetActive(m_FogEnabled);
            }

            BackgroundController backgroundController = FindObjectOfType<BackgroundController>(true);
            if (backgroundController != null)
            {
                backgroundController.transform.position = m_BackgroundPosition;
                backgroundController.transform.eulerAngles = m_BackgroundRotation;
            }
        }

        [Button()]
        public virtual void SaveSceneSetup()
        {
            if (UnityEditor.EditorUtility.DisplayDialog($"{AssetName} Save Scene Setup", $"Are you sure you want to save the current scene setup to \"{AssetName}\"?", "Yes", "No"))
            {
                m_CameraPosition = Camera.main.transform.position;
                m_CameraRotation = Camera.main.transform.eulerAngles;

                LogoController logoController = FindObjectOfType<LogoController>(true);
                if (logoController != null)
                {
                    m_LogoEnabled = logoController.gameObject.activeSelf;
                    m_LogoPosition = logoController.transform.position;
                    m_LogoRotation = logoController.transform.eulerAngles;
                }

                FogController fogController = FindObjectOfType<FogController>(true);
                if (fogController != null)
                {
                    m_FogEnabled = fogController.gameObject.activeSelf;
                }

                BackgroundController backgroundController = FindObjectOfType<BackgroundController>(true);
                if (backgroundController != null)
                {
                    m_BackgroundPosition = backgroundController.transform.position;
                    m_BackgroundRotation = backgroundController.transform.eulerAngles;
                }

                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssets();
            }
        }
    }
}