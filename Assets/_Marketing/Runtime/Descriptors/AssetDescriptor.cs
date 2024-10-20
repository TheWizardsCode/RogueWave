using NaughtyAttributes;
using System;
using System.Collections;
using UnityEditor.Recorder;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace WizardsCode.Marketing
{
    [CreateAssetMenu(fileName = "New Asset Descriptor", menuName = "Wizards Code/Marketing/Generic Asset Descriptor")]
    public abstract class AssetDescriptor : ScriptableObject
    {
        //[Header("Metadata")]
        [SerializeField, Tooltip("A name to be used in filenames and other references."), BoxGroup("Metadata"), FormerlySerializedAs("assetName")]
        string m_AssetName;
        [SerializeField, Tooltip("The description of the asset, used as a reminder for why this is needed."), TextArea(3,8), BoxGroup("Metadata"), FormerlySerializedAs("description")]
        string m_Description;
        [SerializeField, Tooltip("The resolution of all the assets in pixels."), BoxGroup("Resolution")]
        Vector2Int m_Resolution = new Vector2Int(1920, 1080);

        [HorizontalLine(color: EColor.Gray)]
        
        //[Header("Scene Setup")]
        [SerializeField, Tooltip("The camera position for the asset capture."), BoxGroup("Camera"), Foldout("Scene Setup")]
        Vector3 m_CameraPosition = Vector3.zero;
        [SerializeField, Tooltip("The camera rotation for the asset capture."), BoxGroup("Camera"), Foldout("Scene Setup")]
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

        protected RecorderUtils recorder;
        protected RecorderController controller;
        protected Action recordingStoppedCallback;
        protected float sessionStartTime = 0;
        
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

        public virtual bool IsRecording
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

        protected void StopRecording()
        {
            if (controller != null)
            {
                controller.StopRecording();
                controller = null;
            }
        }

        private void OnDisable()
        {
            if (controller != null && controller.IsRecording())
            {
                controller.StopRecording();
            }

            controller = null;
        }

        public abstract IEnumerator GenerateAsset(Action callback = null);

        /// <summary>
        /// The total time the recorder needs to be active.
        /// </summary>
        public abstract float RecordingDuration { get; }

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