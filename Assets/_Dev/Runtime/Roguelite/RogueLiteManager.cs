using NaughtyAttributes;
using NeoFPS;
using RogueWave.GameStats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RogueWave
{
    [CreateAssetMenu(fileName = "FpsManager_RogueLite", menuName = "Rogue Wave/Rogue-Lite Manager", order = 900)]
    public class RogueLiteManager : NeoFpsManager<RogueLiteManager>
    {
        [Header("Scenes")]
        [SerializeField, Tooltip("Name of the Main Menu Scene to load. This is where the player starts the game."), Scene]
        private string m_mainMenuScene = "RogueWave_MainMenu";
        [SerializeField, Tooltip("Name of the Hub Scene to load between levels. This is where the player gets to buy permanent upgrades for their character."), Scene]
        private string m_reconstructionScene = "RogueWave_ReconstructionScene";
        [SerializeField, Tooltip("Name of the Reconstruction Scene to load upon death. This will show a summary of the players most recent run."), Scene]
        private string m_hubScene = "RogueWave_HubScene";
        [SerializeField, Tooltip("The scene to load when the player enters the portal."), Scene]
        private string m_portalScene = "RogueWave_PortalUsed";

        public static string reconstructionScene
        {
            get { return instance.m_reconstructionScene; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void LoadRogueLiteManager()
        {
            UpdateAvailableProfiles();
            GetInstance("FpsManager_RogueLite");
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Rogue Wave/Profiles/Explore To Profiles Folder", priority = 0)]
        static void ExploreToProfilesFolder()
        {
            string folder = string.Format("{0}\\{1}\\", Application.persistentDataPath, k_Subfolder);
            Application.OpenURL(folder);
        }

        [UnityEditor.MenuItem("Tools/Rogue Wave/Profiles/Delete Profiles", priority = 1)]
        static void DeleteProfiles()
        {
            string folder = string.Format("{0}\\{1}\\", Application.persistentDataPath, k_Subfolder);
            DirectoryInfo directory = new DirectoryInfo(folder);
            if (directory.Exists)
            {
                directory.Delete(true);
                UpdateAvailableProfiles();
            }
        }
#endif


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

        public static string mainMenuScene
        {
            get
            {
                if (instance != null)
                    return instance.m_mainMenuScene;
                else
                    return string.Empty;
            }
        }

        public static string hubScene
        {
            get
            {
                if (instance != null)
                    return instance.m_hubScene;
                else
                    return string.Empty;
            }
        }

        public static string combatScene
        {
            get
            {
                return "RogueWave_CombatLevel";
            }
        }

        public static string portalScene
        {
            get
            {
                if (instance != null)
                    return instance.m_portalScene;
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

        private static RogueLiteRunData m_RunData = null;
        public static RogueLiteRunData runData
        {
            get
            {
                if (m_RunData == null)
                    ResetRunData();
                return m_RunData;
            }
        }

        public static bool hasProfile { 
            get
            {
                return availableProfiles != null && availableProfiles.Length != 0;
            } 
        }

        protected override void OnDestroy()
        {
            SaveProfile();

            base.OnDestroy();
        }

        public static void ResetRunData()
        {
            m_RunData = new RogueLiteRunData();
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

        internal static void UpdateAvailableProfiles()
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
            ResetRunData();
            GameStatsManager.ResetStats();
            persistentData.isDirty = true;
        }

        public static string GetProfileName(int index)
        {
            if (availableProfiles == null || availableProfiles.Length == 0)
                return string.Empty;
            else if (index < 0 || index >= availableProfiles.Length)
                return string.Empty;
            else
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
//#if UNITY_EDITOR
//            if (currentProfile == string.Empty)
//            {
//                currentProfile = "Test";

//                FileInfo newProfile = new FileInfo(string.Format("{0}\\{1}.{2}", Application.persistentDataPath, currentProfile, k_Extension));

//                if (availableProfiles == null)
//                {
//                    availableProfiles = new FileInfo[] { newProfile };
//                }
//                else
//                {
//                    List<FileInfo> temp = new List<FileInfo>(availableProfiles);
//                    temp.Add(newProfile);
//                    availableProfiles = temp.ToArray();
//                }
//            }
//#endif
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