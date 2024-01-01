using NaughtyAttributes;
using NeoFPS;
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Playground
{
    internal class EnemyAudioController : MonoBehaviour
    {
        [SerializeField, Tooltip("The Enemy Definition to use for default values if any of the values below are not set."), Expandable]
        [Required("A configuration must be provided. This forms the base definition of the enemy. Higher level enemies will be generated from this base definition.")]
        EnemyDefinition config = null;

        [Header("Audio Sources")]
        [SerializeField, Tooltip("The audio source for this enemies drone sound.")]
        AudioSource droneSource = null;

        BasicEnemyController m_EnemyController = null;

        private void Awake()
        {
            m_EnemyController = GetComponent<BasicEnemyController>();
        }

        private void OnEnable()
        {
            m_EnemyController.onDeath.AddListener(OnDeath);
        }

        private void Start()
        {
            StartDrone();
        }

        private void OnDisable()
        {
            m_EnemyController.onDeath.RemoveListener(OnDeath);
            StopDrone();
        }

        protected void OnDeath()
        {
            droneSource.Stop();

            if (config.deathClips.Length > 0)
            {
                PlayOneShot(config.deathClips[UnityEngine.Random.Range(0, config.deathClips.Length)], transform.position);
            } else
            {
                PlayOneShot(config.GetDeathClip(), transform.position);
            }
        }

        private void StartDrone()
        {
            if (config.droneClip != null)
            {
                droneSource.clip = config.droneClip;
            }
            else
            {
                droneSource.clip = config.droneClip;
            }
            droneSource.loop = true;
            droneSource.Play();
        }

        private void StopDrone()
        {
            droneSource.Stop();
        }

        static void PlayOneShot(AudioClip clip, Vector3 position)
        {
            // OPTIMIZATION Play only a limited number of death sounds within a certain time frame. Perhaps adding chorus or similar on subsequent calls
            NeoFpsAudioManager.PlayEffectAudioAtPosition(clip, position);
        }

#if UNITY_EDITOR

        #region Inspector
        [Button]
        private void SaveCopyOfConfig()
        {
            string defaultPath = AssetDatabase.GetAssetPath(config);
            string directoryPath = Path.GetDirectoryName(defaultPath);

            string path = EditorUtility.SaveFilePanel(
                "Save Enemy Definition",
                directoryPath,
                $"{transform.root.name} Enemy Definition",
                "asset"
            );

            if (path.Length != 0)
            {
                string relativePath = "Assets" + path.Substring(Application.dataPath.Length);

                EnemyDefinition newConfig = ScriptableObject.CreateInstance<EnemyDefinition>();

                FieldInfo[] fields = newConfig.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo field in fields)
                {
                    if (field.IsPublic && !Attribute.IsDefined(field, typeof(System.NonSerializedAttribute)) ||
                        Attribute.IsDefined(field, typeof(SerializeField)))
                    {
                        field.SetValue(newConfig, field.GetValue(config));
                    }
                }

                AssetDatabase.CreateAsset(newConfig, relativePath); 
                config = newConfig;
                AssetDatabase.SaveAssets();
            }
        }
        #region Validatoin
        #endregion

        #endregion
#endif
    }
}
