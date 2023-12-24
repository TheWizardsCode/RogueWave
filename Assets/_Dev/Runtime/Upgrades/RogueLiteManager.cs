using NeoFPS;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Playground
{
    [CreateAssetMenu(fileName = "FpsManager_RogueLite", menuName = "Playground/Rogue-Lite Manager")]
    public class RogueLiteManager : NeoFpsManager<RogueLiteManager>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void LoadRogueLiteManager()
        {
            UpdateAvailableProfiles();
            GetInstance("FpsManager_RogueLite");
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Playground/Explore To Profiles Folder", priority = 0)]
        static void ExploreToProfilesFolder()
        {
            string folder = string.Format("{0}\\{1}\\", Application.persistentDataPath, k_Subfolder);
            Application.OpenURL(folder);
        }
#endif

        [SerializeField, Tooltip("")]
        private string m_HubScene = "Playground_HubScene";

        const string k_Extension = "profileData";
        const string k_Subfolder = "Profiles";

        private RuntimeBehaviour m_ProxyBehaviour = null;

        public static FileInfo[] availableProfiles
        {
            get;
            private set;
        } = { };

        public static string currentProfile
        {
            get;
            private set;
        } = string.Empty;

        public static string hubScene
        {
            get
            {
                if (instance != null)
                    return instance.m_HubScene;
                else
                    return string.Empty;
            }
        }

        private static RogueLitePersistentData m_PersistentData = null;
        public static RogueLitePersistentData persistentData
        {
            get
            {
                if (m_PersistentData == null)
                    ResetPersistentData();
                return m_PersistentData;
            }
        }

        private static List<FpsInventoryItemBase> m_RunLoadoutData = new List<FpsInventoryItemBase>();
        /// <summary>
        /// The items that will be available to the player in their loadout when they start a level in a run.
        /// This will be reset on death.
        /// </summary>
        public static List<FpsInventoryItemBase> RunLoadoutData
        {
            get
            {
                return m_RunLoadoutData;
            }
        }

        private static List<IRecipe> m_RunRecipeData = new List<IRecipe>();
        /// <summary>
        /// The recipes that will be available to the player in their NanobotManager when they start a level in a run.
        /// This will be reset on death.
        /// </summary>
        public static List<IRecipe> RunRecipeData
        {
            get
            {
                return m_RunRecipeData;
            }
        }

        public static void ClearRunData()
        {
            RunRecipeData.Clear();
            RunLoadoutData.Clear();
        }

        public override bool IsValid()
        {
            return true;
        }

        protected override void Initialise()
        {
            m_ProxyBehaviour = GetBehaviourProxy<RuntimeBehaviour>();
        }

        class RuntimeBehaviour : MonoBehaviour
        {
            void Start()
            {
                StartCoroutine(SaveProfileData());
            }

            IEnumerator SaveProfileData()
            {
                var wait = new WaitForSecondsRealtime(3f);
                while (true)
                {
                    yield return wait;

                    if (m_PersistentData != null && m_PersistentData.isDirty)
                        SaveProfile();
                }
            }
        }

        static RogueLitePersistentData CreatePersistentDataFromJson(string json)
        {
            if (!string.IsNullOrEmpty(json))
                m_PersistentData = JsonUtility.FromJson<RogueLitePersistentData>(json);
            else
                m_PersistentData = new RogueLitePersistentData();

            return persistentData;
        }

        public static RogueLitePersistentData ResetPersistentData()
        {
            m_PersistentData = new RogueLitePersistentData();
            return persistentData;
        }

        public static void AssignPersistentData(RogueLitePersistentData custom)
        {
            if (custom != null)
                m_PersistentData = custom;
            else
                m_PersistentData = new RogueLitePersistentData();
        }

        static void UpdateAvailableProfiles()
        {
            // Get or create the profiles folder
            string folder = string.Format("{0}\\{1}\\", Application.persistentDataPath, k_Subfolder);
            DirectoryInfo directory = Directory.Exists(folder) ? new DirectoryInfo(folder) : Directory.CreateDirectory(folder);

            // Get and sort an array of profile files with the correct extension
            if (directory != null)
            {
                FileInfo[] result = directory.GetFiles("*." + k_Extension);
                Array.Sort(result, (FileInfo f1, FileInfo f2) => { return f2.CreationTime.CompareTo(f1.CreationTime); });
                availableProfiles = result;
            }
            else
                availableProfiles = new FileInfo[0];
        }

        public static void CreateNewProfile(string profileName)
        {
            currentProfile = profileName;
            ResetPersistentData();
            persistentData.isDirty = true;
        }

        public static string GetProfileName(int index)
        {
            return Path.GetFileNameWithoutExtension(availableProfiles[index].Name);
        }

        public static void LoadProfile(int index)
        {
            // Load the file if available and create new instance from json
            using (var stream = availableProfiles[index].OpenText())
            {
                string json = stream.ReadToEnd();
                CreatePersistentDataFromJson(json);
            }

            // Get the profile name
            currentProfile = GetProfileName(index);
        }

        public static void SaveProfile()
        {
            // Only save if there have been changes
            if (persistentData == null || !persistentData.isDirty || currentProfile == string.Empty)
                return;

            // Check the folder exists
            string folder = string.Format("{0}\\{1}\\", Application.persistentDataPath, k_Subfolder);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // Write the file from instance json
            using (var stream = File.CreateText(string.Format("{0}{1}.{2}", folder, currentProfile, k_Extension)))
            {
                string json = JsonUtility.ToJson(m_PersistentData, true);
                stream.Write(json);
            }

            // Wipe the instance dirty flag
            persistentData.isDirty = false;

            // Update available saves
            UpdateAvailableProfiles();
        }
    }
}