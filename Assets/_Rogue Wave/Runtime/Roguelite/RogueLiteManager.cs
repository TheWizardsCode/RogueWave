using NaughtyAttributes;
using NeoFPS;
using RogueWave.GameStats;
using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using WizardsCode.RogueWave;
using WizardsCode.StoryTeller;

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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            UpdateAvailableProfiles();
            LoadProfile(0);
        }

        public static string reconstructionScene
        {
            get { return instance.m_reconstructionScene; }
        }

        static string ProfilesFolderPath {
            get { return string.Format("{0}\\{1}\\", Application.persistentDataPath, k_Subfolder); }
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
            Application.OpenURL(ProfilesFolderPath);
        }

        [UnityEditor.MenuItem("Tools/Rogue Wave/Profiles/Delete Profiles", priority = 1)]
        static void DeleteProfiles()
        {
            DirectoryInfo directory = new DirectoryInfo(ProfilesFolderPath);
            if (directory.Exists)
            {
                directory.Delete(true);
                UpdateAvailableProfiles();
            }
        }

        [UnityEditor.MenuItem("Tools/Rogue Wave/Profiles/Reset Tutorial", priority = 0)]
        static void ResetTutorial()
        {
            StoryManager.Instance.ResetStory();
        }
#endif

        const string k_ProfileExtension = "profileData";
        const string k_StatsExtension = "statsData";
        const string k_CampaignExtension = "campaignData";
        const string k_Subfolder = "Profiles";

        private RuntimeBehaviour m_ProxyBehaviour = null;

        public static FileInfo[] availableProfiles
        {
            get;
            private set;
        } = { };

        public static string CurrentProfile
        {
            get;
            private set;
        } = string.Empty;

        public static string MainMenuScene
        {
            get
            {
                if (instance != null)
                    return instance.m_mainMenuScene;
                else
                    return string.Empty;
            }
        }

        public static string HubScene
        {
            get
            {
                if (instance != null)
                    return instance.m_hubScene;
                else
                    return string.Empty;
            }
        }

        public static string CombatScene
        {
            get
            {
                return "RogueWave_CombatLevel";
            }
        }

        public static string PortalScene
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
        public static RogueLitePersistentData PersistentData
        {
            get
            {
                if (m_PersistentData == null)
                    ResetPersistentData();
                return m_PersistentData;
            }
        }

        private static RogueLiteRunData m_RunData = null;
        public static RogueLiteRunData RunData
        {
            get
            {
                if (m_RunData == null)
                    ResetRunData();
                return m_RunData;
            }
        }

        public static bool HasProfile { 
            get
            {
                return availableProfiles != null && availableProfiles.Length != 0;
            } 
        }

        protected override void OnDestroy()
        {
            RogueLiteManager.PersistentData.isDirty = true; // set to true as a security in case we have any bugs not setting it
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
        }

        static RogueLitePersistentData CreatePersistentDataFromJson(string json)
        {
            if (!string.IsNullOrEmpty(json))
                m_PersistentData = JsonUtility.FromJson<RogueLitePersistentData>(json);
            else
                m_PersistentData = new RogueLitePersistentData();

            return PersistentData;
        }

        public static RogueLitePersistentData ResetPersistentData()
        {
            m_PersistentData = new RogueLitePersistentData();
            return PersistentData;
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
            DirectoryInfo directory = Directory.Exists(ProfilesFolderPath) ? new DirectoryInfo(ProfilesFolderPath) : Directory.CreateDirectory(ProfilesFolderPath);

            // Get and sort an array of profile files with the correct extension
            if (directory != null)
            {
                FileInfo[] result = directory.GetFiles("*." + k_ProfileExtension);
                Array.Sort(result, (FileInfo f1, FileInfo f2) => { return f2.CreationTime.CompareTo(f1.CreationTime); });
                availableProfiles = result;
            }
            else
                availableProfiles = new FileInfo[0];

            if (CurrentProfile == string.Empty && availableProfiles.Length > 0)
            {
                LoadProfile(0);
            }
        }

        public static void CreateNewProfile(string profileName)
        {
            CurrentProfile = profileName;
            ResetPersistentData();
            ResetRunData();
            GameStatsManager.Instance.ResetStats();
            PersistentData.isDirty = true;
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

        /// <summary>
        /// Get the absolute file path for the current profile save files.
        /// Note that this does not return a string with the file extension.
        /// Each save file for a given profile will have the same name but with a different extension.
        /// The extension should be added in the code saving the file.
        /// </summary>
        /// <returns>The absolute path to the save files for this profile, without the specificc file extension.</returns>
        public string GetSaveFilenameSansExtension()
        {
            return string.Format("{0}{1}", ProfilesFolderPath, CurrentProfile);
        }

        public static void LoadProfile(int index)
        {
            if (index < 0 || index + 1 >= availableProfiles.Length)
                return;

            RogueLiteManager.PersistentData.isDirty = true; // Set to true as a security in case we fogot to set it somewhere
            
            // Load the file if available and create new instance from json
            using (var stream = availableProfiles[index].OpenText())
            {
                string json = stream.ReadToEnd();
                CreatePersistentDataFromJson(json);
            }

            // Get the profile name
            CurrentProfile = GetProfileName(index);

            // Load the stats from the saved files
            if (instance != null) // checking for null as we may be running this before the instance is created, in which case there is no profile to load yet
            {
                string path = string.Format("{0}.{1}", instance.GetSaveFilenameSansExtension(), k_StatsExtension);
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    StatsWrapperArray wrapperArray = JsonUtility.FromJson<StatsWrapperArray>(json);
                    IntGameStat[] stats = Resources.LoadAll<IntGameStat>("");

                    for (int i = 0; i < wrapperArray.stats.Length; i++)
                    {
                        for (int y = 0; y < stats.Length; y++)
                        {
                            if (wrapperArray.stats[i].key == stats[y].key)
                            {
                                stats[y].Value = wrapperArray.stats[i].value;
                            }
                        }
                    }
                }

                // Load the campaign data
                if (CampaignManager.Instance != null)
                {
                    string campaignPath = string.Format("{0}.{1}", instance.GetSaveFilenameSansExtension(), k_CampaignExtension);
                    CampaignManager.Instance.Init(campaignPath);
                    
                    if (File.Exists(campaignPath))
                    {
                        using (var stream = File.OpenText(campaignPath))
                        {
                            string json = stream.ReadToEnd();
                            JsonUtility.FromJsonOverwrite(json, CampaignManager.Instance);
                        }
                    }
                    CampaignManager.Instance.InitializeStory(((CampaignManager)CampaignManager.Instance).CurrentCampaign.InkStory);
                }
            }
        }

        public static void SaveProfile()
        {
            if (instance == null)
                return;

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
            if (PersistentData == null || !PersistentData.isDirty || CurrentProfile == string.Empty)
                return;

            // Check the folder exists
            if (!Directory.Exists(ProfilesFolderPath))
                Directory.CreateDirectory(ProfilesFolderPath);

            // Write the profile data
            using (var stream = File.CreateText(string.Format("{0}.{1}", instance.GetSaveFilenameSansExtension(), k_ProfileExtension)))
            {
                string json = JsonUtility.ToJson(m_PersistentData, true);
                stream.Write(json);
            }

            // Write the stats data
            using (var stream = File.CreateText(string.Format("{0}.{1}", instance.GetSaveFilenameSansExtension(), k_StatsExtension)))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("{\n\"stats\": [");
                IntGameStat[] stats = Resources.LoadAll<IntGameStat>("");
                for (int i = 0; i < stats.Length; i++)
                {
                    StatsWrapper wrapper = new StatsWrapper(stats[i].key);
                    wrapper.value = stats[i].Value;
                    sb.Append(JsonUtility.ToJson(wrapper, true));
                    if (i < stats.Length - 1)
                    {
                        sb.AppendLine(",");
                    } else
                    {
                        sb.AppendLine();
                    }
                }
                sb.AppendLine("]}");
                stream.Write(sb.ToString());
            }

            // Write the Campaign data
            if (CampaignManager.Instance != null)
            {
                using (var stream = File.CreateText(string.Format("{0}.{1}", instance.GetSaveFilenameSansExtension(), k_CampaignExtension)))
                {
                    string json = JsonUtility.ToJson(CampaignManager.Instance, true);
                    stream.Write(json);
                }
            }

            PersistentData.isDirty = false;

            // Update available saves
            UpdateAvailableProfiles();
        }

        /// <summary>
        /// Get the total Count of a recipe in the player's current recipe permanent + temporary collection.
        /// </summary>
        /// <param name="recipe">The recipe to count.</param>
        /// <returns>Total number of recipes held in current permanent and temporary collections.</returns>
        /// <seealso cref="RogueLiteRunData.GetCount(IRecipe)"/>
        /// <seealso cref="RogueLitePersistentData.GetCount(IRecipe)"/>
        internal static int GetTotalCount(IRecipe recipe)
        {
            int total = RunData.GetCount(recipe);
            total += PersistentData.GetCount(recipe);
            return total;
        }

        [System.Serializable]
        private class StatsWrapper
        {
            public string key;
            public int value;

            public StatsWrapper(string key)
            {
                this.key = key;
            }
        }

        [Serializable]
        private class StatsWrapperArray
        {
            public StatsWrapper[] stats;
        }
    }
}