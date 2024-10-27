using UnityEngine;
using NeoFPS.SinglePlayer;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using NeoFPS;
using UnityEngine.Events;
using NeoSaveGames.SceneManagement;
using System.Collections;
using NeoFPS.Constants;
using NaughtyAttributes;
using RogueWave.UI;
using RogueWave.GameStats;
using System.Collections.Generic;
using System.Text;
using Random = UnityEngine.Random;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using NeoFPS.Samples;
using WizardsCode.RogueWave;
using System.Linq;
using static WizardsCode.RogueWave.ExtractionFXController;

namespace RogueWave
{
    public class RogueWaveGameMode : FpsSoloGameCustomisable, ISpawnZoneSelector, ILoadoutBuilder
    {
        [Header("Scene Setup")]
        [SerializeField, Tooltip("The pre-spawn popup prefab to use.")]
        private LevelMenu m_PreSpawnUI = null;
        [SerializeField, Tooltip("Should the player be spawned on start of the game?")]
        private bool m_SpawnPlayerOnStart = true;

        [Header("Victory")]
        [SerializeField, Tooltip("The amount of time to wait after victory before heading to the hub")]
        float m_VictoryDuration = 5f;
        [SerializeField, Tooltip("The prefab to use for the extraction FX."), Required]
        ExtractionFXController extractionFx;

        [Header("Character")]
        [SerializeField, NeoPrefabField(required = true), Tooltip("The player prefab to instantiate if none exists.")]
        private FpsSoloPlayerController m_PlayerPrefab = null;
        [SerializeField, NeoPrefabField(required = true), Tooltip("The character prefab to use.")]
        private FpsSoloCharacter m_CharacterPrefab = null;
        [SerializeField, Tooltip("The initial health of the player character.")]
        private float initialHealth = 40;
        [SerializeField, Tooltip("The recipes that will be available to the player at the start of each run, regardless of resources and death.")]
        [FormerlySerializedAs("m_StartingRecipes")]
        private AbstractRecipe[] _startingRecipesPermanent;
        [SerializeField, Tooltip("The recipes that will be available to the player on the start of their first run, but lost on death.")]
        private AbstractRecipe[] _startingRecipesRun;

        [Header("Level Management")]
        [SerializeField, Tooltip("If true the level will be generated when the game mode starts.")]
        bool m_generateLevelOnStart = false;
        [SerializeField, Tooltip("The campaign definitions which defines the levels to play in order, which in turn defines the enemies, geometry and more for each level."), Expandable, FormerlySerializedAs("campaign")]
        CampaignDefinition m_Campaign;

        // Game Stats
        [SerializeField, Expandable, Foldout("Game Stats"), Tooltip("A short textual log of the game results.")]
        private StringGameStat m_GameLog;
        [SerializeField, Expandable, Foldout("Game Stats"), Tooltip("The number of runs started, regardless of the outcome.")]
        private IntGameStat m_RunCount;
        [SerializeField, Expandable, Foldout("Game Stats"), Tooltip("The count of succesful runs in the game.")]
        private IntGameStat m_VictoryCount;
        [SerializeField, Expandable, Foldout("Game Stats"), Tooltip("The count of portal exits in the game. A portal exist is when a player finds a portal and manages to exit through it.")]
        private IntGameStat m_PortalExitsCount;
        [SerializeField, Expandable, Foldout("Game Stats"), Tooltip("The count of deaths in the game.")]
        private IntGameStat m_DeathCount;
        [SerializeField, Expandable, Foldout("Game Stats"), Tooltip("The time player stat for recording how long a player has been inside runs.")]
        private IntGameStat m_TimePlayedStat;

        [Header("Events")]
        [SerializeField, Tooltip("Game event to fire when the player spawns into the level.")]
        GameEvent playerSpawnedEvent;
        [SerializeField, Tooltip("Game event to fire when the player dies.")]
        GameEvent playerDiedEvent;
        [SerializeField, Tooltip("Game event to fire when the player escapes a level (based on time).")]
        GameEvent playerEscapedEvent;
        [SerializeField, Tooltip("Game event to fire when the player exits via a portal.")]
        GameEvent playerExitedViaPortalEvent;
        [SerializeField, Tooltip("The event to trigger when the level generator creates a spawner.")]
        public UnityEvent<Spawner> onSpawnerCreated;
        [SerializeField, Tooltip("The event to trigger when an enemy is spawned into the game.")]
        public UnityEvent<BasicEnemyController> onEnemySpawned;

        public override bool spawnOnStart
        {
            get { return m_SpawnPlayerOnStart; }
            set { m_SpawnPlayerOnStart = value; }
        }

        public virtual bool showPrespawnUI
        {
            get { return m_PreSpawnUI != null; }
        }

        public AbstractRecipe[] StartingPermanentRecipes
        {
            get { return _startingRecipesPermanent; }
            set
            {
                _startingRecipesPermanent = _startingRecipesPermanent.Concat(value).ToArray();
            }
        }

        public AbstractRecipe[] StartingRunRecipes
        {
            get { return _startingRecipesRun; }
            set {
                _startingRecipesRun = value;
            }
        }

        public CampaignDefinition Campaign => m_Campaign;

        private AIDirector m_aiDirector;
        private AIDirector aiDirector
        {
            get
            {
                if (m_aiDirector == null)
                {
                    m_aiDirector = FindAnyObjectByType<AIDirector>();
                }
                return m_aiDirector;
            }
        }

        LevelProgressBar levelProgressBar;

        int m_BossSpawnersRemaining = 0;
        protected int bossSpawnersRemaining
        {
            get { return m_BossSpawnersRemaining; }
            private set
            {
                m_BossSpawnersRemaining = value;
            }
        }
        private float timeInLevel;

        LevelGenerator _levelGenerator;
        private HudGameStatusController statusHud;

        internal LevelGenerator levelGenerator
        {
            get
            {
                if (_levelGenerator == null)
                    _levelGenerator = GetComponentInChildren<LevelGenerator>();
                return _levelGenerator;
            }
            private set
            {
                _levelGenerator = value;
            }
        }

        public WfcDefinition currentLevelDefinition
        {
            get { 
                if (m_Campaign.levels.Length <= RogueLiteManager.persistentData.currentGameLevel)
                    return m_Campaign.levels[m_Campaign.levels.Length - 1]; 
                else
                    return m_Campaign.levels[RogueLiteManager.persistentData.currentGameLevel];
            }
        }

        #region Unity Life-cycle
        protected override void Awake()
        {
            statusHud = FindObjectOfType<HudGameStatusController>();
            levelGenerator = GetComponentInChildren<LevelGenerator>();

            levelProgressBar = FindObjectOfType<LevelProgressBar>(true);

            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            GameLog.ClearLog();
        }

        private void Update()
        {
            if (FpsSoloCharacter.localPlayerCharacter == null) 
            {
                return;
            }

            timeInLevel += Time.deltaTime;

            if (timeInLevel >= currentLevelDefinition.Duration && m_VictoryCoroutine == null)
            {
                LogGameState("Level Cleared - Timed Extraction");
                m_VictoryCoroutine = StartCoroutine(DelayedLevelTimerAchievedCoroutine(m_VictoryDuration));
            }

            levelProgressBar.Value = timeInLevel;

            updateHUD();
        }
        #endregion

        #region Game Events

        private Coroutine m_VictoryCoroutine = null;
        private float m_VictoryTimer = 0f;

        public static event UnityAction onLevelComplete;
        public static event UnityAction onPortalEntered;

        internal void OnSpawnerDestroyed(Spawner spawner)
        {
            if (FpsSoloCharacter.localPlayerCharacter != null && FpsSoloCharacter.localPlayerCharacter.isAlive == false)
            {
                return;
            }

            spawner.onSpawnerDestroyed.RemoveListener(OnSpawnerDestroyed);
            spawners.Remove(spawner);

            if (spawner.isBossSpawner)
            {
                bossSpawnersRemaining--;
            }

            LogGameState("Level Cleared - Spawners destroyed");

            if (bossSpawnersRemaining == 0 && m_VictoryCoroutine == null)
            {
                m_VictoryCoroutine = StartCoroutine(DelayedLevelClearedCoroutine(m_VictoryDuration));
            }
        }

        protected override IEnumerator DelayedDeathReactionCoroutine(float delay)
        {
            StartExtractionFX(ExtractionType.Death);

            // Prevent additional resources being collected after death
            if (FpsSoloCharacter.localPlayerCharacter != null)
            {
                MagnetController magnet = FpsSoloCharacter.localPlayerCharacter.GetComponent<MagnetController>();
                if (magnet != null)
                {
                    magnet.range = 0;
                    magnet.speed = 0;
                }
            }

            yield return base.DelayedDeathReactionCoroutine(delay);
        }

        private void StartExtractionFX(ExtractionType extractionType)
        {
            RW_HudHider.HideHUD();
            levelGenerator.HideLevelGeometry();
            aiDirector.Kill(1);

            extractionFx.extractionType = extractionType;
            extractionFx.gameObject.SetActive(true);
            AudioManager.MuteAllExceptNanobots(2);
        }

        protected override void DelayedDeathAction()
        {
            playerDiedEvent?.Raise();

            MusicManager.Instance.PlayDeathMusic();

            LogGameState("Death");

            m_GameLog.Add("D, ");

            SaveGameData("Death");
            GameLog.ClearLog();

            RogueLiteManager.ResetRunData();

            if (SceneManager.GetActiveScene().name != RogueLiteManager.combatScene) // must be the intro scene
            {
                NeoSceneManager.LoadScene(RogueLiteManager.mainMenuScene);
            }
            else
            {
                NeoSceneManager.LoadScene(RogueLiteManager.reconstructionScene);
            }
        }

        void DelayedVictoryAction(bool usedPortal)
        {
            m_GameLog.Add("C, ");

            playerExitedViaPortalEvent?.Raise();

            SaveGameData("usedPortal");
            GameLog.ClearLog();

            if (usedPortal)
            {
                NeoSceneManager.LoadScene(RogueLiteManager.portalScene);
            }
            else
            {
                NeoSceneManager.LoadScene(RogueLiteManager.hubScene);
            }
        }

        /// <summary>
        /// Level timer achieved is when the timeInLevel reaches the target, and the player has not died, but the player has not yet exited the level via the portal.
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        /// <seealso cref="DelayedLevelClearedCoroutine(float)"/>
        /// <seealso cref="DelayedLevelCompleteCoroutine(float)"/>"/>
        private IEnumerator DelayedLevelTimerAchievedCoroutine(float delay)
        {
            StartExtractionFX(ExtractionType.PlayerEscaped);
            yield return new WaitForSeconds(extractionFx.duration / 4);

            playerEscapedEvent?.Raise();
            yield return new WaitForSeconds(extractionFx.duration / 4);

            onLevelComplete?.Invoke();
            SaveGameData("Level Timer Achieved");

            // Temporary magnet buff to pull in victory rewards
            MagnetController magnet = null;
            float originalRange = 0;
            float originalSpeed = 0;
            if (FpsSoloCharacter.localPlayerCharacter != null)
            {
                magnet = FpsSoloCharacter.localPlayerCharacter.GetComponent<MagnetController>();
                if (magnet != null)
                {
                    originalRange = magnet.range;
                    originalSpeed = magnet.speed;
                    magnet.range = 100;
                    magnet.speed = 25;
                }
            }

            yield return new WaitForSeconds(delay);

            // Reset magnet
            if (magnet != null)
            {
                magnet.range = originalRange;
                magnet.speed = originalSpeed;
            }

            RogueLiteManager.persistentData.currentGameLevel++;

            if (m_VictoryCount != null)
            {
                m_VictoryCount.Add(1);
            }

            if (inGame)
                DelayedVictoryAction(false);
        }

        /// <summary>
        /// Level cleared is when all spawners are defeated, and the player has not died, but the player has not yet exited the level via the portal.
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        /// <seealso cref="DelayedLevelTimerAchievedCoroutine(float)"/>
        /// <seealso cref="DelayedLevelCompleteCoroutine(float)"/>
        private IEnumerator DelayedLevelClearedCoroutine(float delay)
        {
            StartExtractionFX(ExtractionType.SpawnerDestroyed);
            yield return new WaitForSeconds(extractionFx.duration / 4);

            MusicManager.Instance.PlayMenuMusic();

            onLevelComplete?.Invoke();
            SaveGameData("Level Cleared");

            // Temporary magnet buff to pull in victory rewards
            MagnetController magnet = null;
            float originalRange = 0;
            float originalSpeed = 0;
            if (FpsSoloCharacter.localPlayerCharacter != null)
            {
                magnet = FpsSoloCharacter.localPlayerCharacter.GetComponent<MagnetController>();
                if (magnet != null)
                {
                    originalRange = magnet.range;
                    originalSpeed = magnet.speed;
                    magnet.range = 100;
                    magnet.speed = 25;
                }
            }

            yield return null;

            // Delay timer
            yield return new WaitForSeconds(delay);

            // Reset magnet
            if (magnet != null)
            {
                magnet.range = originalRange;
                magnet.speed = originalSpeed;
            }

            RogueLiteManager.persistentData.currentGameLevel++;

            if (m_VictoryCount != null)
            {
                m_VictoryCount.Add(1);
            }

            if (inGame)
                DelayedVictoryAction(false);
        }

        /// <summary>
        /// Level completed is when the player has managed to exit the level via a portal.
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        /// <seealso cref="DelayedLevelTimerAchievedCoroutine(float)"/>
        /// <seealso cref="DelayedLevelClearedCoroutine(float)"/>
        private IEnumerator DelayedLevelCompleteCoroutine(float delay)
        {
            StartExtractionFX(ExtractionType.PortalUsed);
            yield return new WaitForSeconds(extractionFx.duration / 4);

            LogGameState("Portal used");

            FloatValueModifier modifier = FpsSoloCharacter.localPlayerCharacter.GetComponent<MovementUpgradeManager>().GetFloatModifier("moveSpeed");
            modifier.multiplier = 0;

            yield return null;

            onPortalEntered?.Invoke();

            yield return null;

            m_VictoryTimer = delay;
            while (m_VictoryTimer > 0f)
            {
                m_VictoryTimer -= Time.deltaTime;
                yield return null;
            }

            RogueLiteManager.persistentData.currentGameLevel++;

            if (m_VictoryCount != null)
            {
                m_VictoryCount.Add(1);
            }

            if (m_PortalExitsCount != null)
            {
                m_PortalExitsCount.Add(1);
            }

            if (inGame)
                DelayedVictoryAction(true);
        }

        /// <summary>
        /// Save the game data. The event name is used to identify the event that triggered the save
        /// </summary>
        /// <param name="eventName"></param>
        private void SaveGameData(string eventName)
        {
            float timePlayed = Time.time - startTime;
            if (m_TimePlayedStat != null)
            {
                m_TimePlayedStat.Add(Mathf.RoundToInt(timePlayed));
            }

#if DISCORD_ENABLED
            GameStatsManager.Instance.SendDataToWebhook(eventName);
#endif
        }

#endregion

        #region ISpawnZoneSelector IMPLEMENTATION

        [Header("Spawning")]

        [SerializeField, Tooltip("The spawn zones (groups of spawn points) available on this map.")]
        private SpawnZoneSelectorData m_SpawnZones = new SpawnZoneSelectorData();

        private static readonly NeoSerializationKey k_SpawnZoneIndexKey = new NeoSerializationKey("spawnIndex");

        public Sprite mapSprite
        {
            get { return m_SpawnZones.mapSprite; }
        }

        public int numSpawnZones
        {
            get { return m_SpawnZones.spawnZones.Length; }
        }

        public int currentSpawnZoneIndex
        {
            get { return m_SpawnZones.currentIndex; }
            set { m_SpawnZones.currentIndex = value; }
        }

        public ISpawnZoneInfo GetSpawnZoneInfo(int index)
        {
            return m_SpawnZones.spawnZones[index];
        }

        protected override void OnStart()
        {
            if (m_generateLevelOnStart)
            {
                levelGenerator.Generate(currentLevelDefinition, m_Campaign.seed);
            }

            base.OnStart();

            if (m_SpawnZones.currentIndex == -1 && m_SpawnZones.spawnZones.Length > 0)
                m_SpawnZones.currentIndex = 0;
        }

        #endregion

        #region ILoadoutBuilder IMPLEMENTATION

        [Header("Loadout Builder")]

        [SerializeField, HideInInspector]
        private LoadoutBuilderData m_LoadoutBuilder = new LoadoutBuilderData();
        private float startTime;
        private List<Spawner> spawners = new List<Spawner>();

        internal int bossSpawnerCount => bossSpawnersRemaining;

        public int numLoadoutBuilderSlots
        {
            get { return m_LoadoutBuilder.slots.Length; }
        }

        public ILoadoutBuilderSlot GetLoadoutBuilderSlotInfo(int index)
        {
            return m_LoadoutBuilder.slots[index];
        }

        public FpsInventoryLoadout GetLoadout()
        {
            return m_LoadoutBuilder.GetLoadout();
        }

        protected override bool GetFriendlyFire()
        {
            return false;
        }

        protected override void OnCharacterSpawned(ICharacter character)
        {
            m_RunCount.Add(1);

            // Configure the Level Progress Bar
            levelProgressBar.gameObject.SetActive(true);
            levelProgressBar.MaxValue = currentLevelDefinition.Duration;
            levelProgressBar.MinValue = 0;
            levelProgressBar.Value = 0;
            levelProgressBar.levelDefinition = currentLevelDefinition;
            
            timeInLevel = 0;

            // Configure the character health management
            character.onIsAliveChanged += OnCharacterIsAliveChanged;
            BasicHealthManager healthManager = character.GetComponent<BasicHealthManager>();
            healthManager.healthMax = initialHealth;

            IRecipe startingWeapon;
            if (RogueLiteManager.persistentData.WeaponBuildOrder.Count > 0 && RecipeManager.TryGetRecipe(RogueLiteManager.persistentData.WeaponBuildOrder[0], out startingWeapon))
            {
                WeaponRecipe weaponRecipe = startingWeapon as WeaponRecipe;
                if (weaponRecipe != null)
                {
                    RogueLiteManager.runData.AddToLoadout(weaponRecipe.pickup.GetItemPrefab());
                }
            }

            // This test for the combat scene meant the player loadout was not being applied in test and demo scenes, but why was it here in the first place since the player only spawns into scenes where they are going into combat. Can we remove this? 10/8/24
            // if (SceneManager.GetActiveScene().name == RogueLiteManager.combatScene)
            // {
                FpsInventoryLoadout loadout = ConfigureLoadout();
                if (loadout != null)
                    character.GetComponent<IInventory>()?.ApplyLoadout(loadout);
            // }

            // Add nanobot recipes
            NanobotManager manager = character.GetComponent<NanobotManager>();
            for (int i = 0; i < RogueLiteManager.runData.Count; i++)
            {
                manager.AddToRunRecipes(RogueLiteManager.runData.GetRecipeAt(i));
            }

            // since a recipe may have adjusted the max health, we need to reset the health to the new max
            healthManager.health = healthManager.healthMax;

            startTime = Time.time;

            m_GameLog.Add($"{m_Campaign.name}-{RogueLiteManager.persistentData.currentGameLevel}-");

            LogGameState("Character Spawned");

            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.PlayCombatMusic();
            }

            // We need to be able to execute commands against the player character. This is done by a SceneSetupCommands object in the scene. Currently this is not implemented.
            //if (m_ExecuteSceneSetupCommandsOnSpawn)
            //{
            //    SceneSetupCommands sceneSetupCommands = FindObjectOfType<SceneSetupCommands>();
            //    if (sceneSetupCommands != null)
            //    {
            //        sceneSetupCommands.ExecuteScript();
            //    }
            //}

            playerSpawnedEvent?.Raise();
        }

        private void LogGameState(string eventName)
        {
            StringBuilder log = new StringBuilder($"{eventName}, ");

            foreach (IRecipe recipe in RogueLiteManager.runData.GetRecipes())
            {
                log.Append($"Recipe: {recipe.DisplayName}");
                if (RogueLiteManager.persistentData.RecipeIds.Contains(recipe.UniqueID))
                {
                    log.Append(" (Permanent), ");
                } else
                {
                    log.Append(" (Temporary), ");
                }
            }

            if (FpsSoloCharacter.localPlayerCharacter != null)
            {
                IInventoryItem[] loadout = FpsSoloCharacter.localPlayerCharacter.GetComponent<IInventory>().GetItems();
                foreach (var item in loadout)
                {
                    log.Append($"Inventory Item: {item.name}, ");
                }
            }

            foreach (string id in RogueLiteManager.persistentData.WeaponBuildOrder)
            {
                if (RecipeManager.TryGetRecipe(id, out IRecipe recipe))
                {
                    log.AppendLine($"Weapon Build Order: {recipe.DisplayName}, ");
                }
            }

            // TODO: Remove hard coding of resource stat key
            log.Append($"Resources: {GameStatsManager.Instance.GetIntStat("RESOURCES").value}, ");
            log.Append($"Nanobot Level: {RogueLiteManager.persistentData.currentNanobotLevel}, ");
            log.Append($"Game Level: {RogueLiteManager.persistentData.currentGameLevel}, ");
            log.Append($"Run Number: {RogueLiteManager.persistentData.runNumber}, ");

            if (FpsSoloCharacter.localPlayerCharacter != null)
            {
                log.Append($"Health: {FpsSoloCharacter.localPlayerCharacter.GetComponent<BasicHealthManager>().healthMax}, ");
            }
            
            GameLog.Info(log.ToString());
        }

        private void OnCharacterIsAliveChanged(ICharacter character, bool alive)
        {
            if (alive == false)
            {
                RogueLiteManager.ResetRunData();
                character.onIsAliveChanged -= OnCharacterIsAliveChanged;

                m_DeathCount.Add(1);

                RogueLiteManager.persistentData.isDirty = true;
            }
        }

        #endregion

        private void ConfigureRecipeForRun(string recipeId)
        {
            if (RecipeManager.TryGetRecipe(recipeId, out IRecipe recipe) == false)
            {
                Debug.LogError($"Attempt to configure a recipe with ID {recipeId} but no such recipe can be found. Ignoring this recipe.");
                return;
            }

            ConfigureRecipeForRun(recipe);
        }

        private void ConfigureRecipeForRun(IRecipe recipe)
        {
            if (recipe.IsStackable || RogueLiteManager.runData.Contains(recipe) == false)
            {
                RogueLiteManager.runData.Add(recipe);
            }
        }

        private FpsInventoryLoadout ConfigureLoadout()
        {
            for (int i = 0; i < RogueLiteManager.runData.Loadout.Count; i++)
            {
                FpsInventoryItemBase item = RogueLiteManager.runData.Loadout[i];
                FpsSwappableCategory category = FpsSwappableCategory.Firearm;

                FpsInventoryQuickUseSwappableItem quickUse = item as FpsInventoryQuickUseSwappableItem;
                if (quickUse != null)
                {
                    category = quickUse.category;
                }
                else
                {
                    FpsInventoryWieldableSwappable wieldable = item as FpsInventoryWieldableSwappable;
                    if (wieldable != null)
                    {
                        category = wieldable.category;
                    }
                }
                m_LoadoutBuilder.slots[category].AddOption(item);
            }

            return m_LoadoutBuilder.GetLoadout();
        }

        protected override IController GetPlayerControllerPrototype()
        {
            return m_PlayerPrefab;
        }

        protected override ICharacter GetPlayerCharacterPrototype(IController player)
        {
            return m_CharacterPrefab;
        }

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            base.WriteProperties(writer, nsgo, saveMode);

            writer.WriteValue(k_SpawnZoneIndexKey, m_SpawnZones.currentIndex);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            base.ReadProperties(reader, nsgo);

            int index;
            if (reader.TryReadValue(k_SpawnZoneIndexKey, out index, m_SpawnZones.currentIndex))
                m_SpawnZones.currentIndex = index;
        }

        #region Pre Spawn
        protected override bool PreSpawnStep()
        {
            RogueLiteManager.persistentData.runNumber++;

            AudioManager.ResetAll(1);

            // If this is the first run then we need to ensure the player has a minimum amount of resources
            // TODO: Remove hard coding of resource stat key
            if (RogueLiteManager.persistentData.runNumber == 1 && GameStatsManager.Instance.GetIntStat("RESOURCES").value < 150) // this will be the players first run
            {
                GameStatsManager.Instance.GetIntStat("RESOURCES").SetValue(150);
            }

            // RunData, between levels, will contain all permanent and temporary recipes. In order to strip duplication of stackables in the permanent data we need to remove any that are already in the run data.
            for (int i = 0; i < RogueLiteManager.persistentData.RecipeIds.Count; i++)
            {
                RecipeManager.TryGetRecipe(RogueLiteManager.persistentData.RecipeIds[i], out IRecipe permanentRecipe);
                if (RogueLiteManager.runData.Contains(permanentRecipe))
                {
                    RogueLiteManager.runData.Remove(permanentRecipe);
                }
            }

            // Ensure Game Mode permanent starting recipes are added to the player
            foreach (IRecipe recipe in _startingRecipesPermanent)
            {
                RogueLiteManager.persistentData.Add(recipe);
            }

            // If the character died then the weapon build order may have weapons that were in the rundata and need to be removed.
            for (int i = RogueLiteManager.persistentData.WeaponBuildOrder.Count - 1; i >= 0; i--)
            {
                if (RecipeManager.TryGetRecipe(RogueLiteManager.persistentData.WeaponBuildOrder[i], out IRecipe weapon))
                {
                    if (!RogueLiteManager.persistentData.Contains(weapon))
                    {
                        RogueLiteManager.persistentData.WeaponBuildOrder.RemoveAt(i);
                    }
                }
            }

            // Gather together all the run recipes, both from previous runs and from the starting recipes for this run's Game Mode
            List<IRecipe> recipes = new List<IRecipe>();
            recipes.AddRange(RogueLiteManager.runData.GetRecipes());
            recipes.AddRange(_startingRecipesRun);

            // Reset the run data to ensure we don't keep adding stackables on subsequent runs, the current run data will be added back below
            RogueLiteManager.runData.Clear();

            // Reset loadout so that it can be reset for this run based on the builder order and any new weapons added since the last run
            RogueLiteManager.runData.Loadout.Clear();

            // Ensure the build order contains all the weapons that are available in this run, ones that are already present are not touched, but ones that are missing are added.
            for (int i = 0; i < recipes.Count; i++)
            {
                if (recipes[i] is WeaponRecipe && !RogueLiteManager.persistentData.WeaponBuildOrder.Contains(recipes[i].UniqueID))
                {
                    RogueLiteManager.persistentData.WeaponBuildOrder.Add(recipes[i].UniqueID);
                }

                // Ensure that the player has all the recipes available to them in this run
                RogueLiteManager.runData.Add(recipes[i]);
            }

            // Ensure the player has all the permanaent recipes available to them in this run and that they are correctly configured for the run
            for (int i = 0; i < _startingRecipesPermanent.Length; i++)
            {
                RogueLiteManager.persistentData.Add(_startingRecipesPermanent[i]);
            }
            for (int i = 0; i < RogueLiteManager.persistentData.RecipeCount; i++)
            {
                ConfigureRecipeForRun(RogueLiteManager.persistentData.GetRecipeIdAt(i));
            }

            // if we are showing a pre-spawn UI then we need to show it now
            if (showPrespawnUI)
            {
                LevelMenu ui = PrefabPopupContainer.ShowPrefabPopup(m_PreSpawnUI);
                ui.Initialise(this, SpawnPlayerCharacter);

                return true;
            }
            else
            {
                return false;
            }
        }

        internal void GenerateLevel()
        {
            if (currentLevelDefinition.generateLevelOnSpawn)
            {
                levelGenerator.Generate(currentLevelDefinition, m_Campaign.seed);
            }

            if (currentLevelDefinition.levelReadyAudioClips != null && currentLevelDefinition.levelReadyAudioClips.Length > 0)
            {
                NeoFpsAudioManager.PlayEffectAudioAtPosition(currentLevelDefinition.levelReadyAudioClips[Random.Range(0, currentLevelDefinition.levelReadyAudioClips.Length)], Camera.main.transform.position);
            }

            RogueLiteManager.persistentData.isDirty = true;
        }

        internal void RegisterPortal(PortalController portal)
        {
            portal.onPortalEntered.AddListener(OnPortalEntered);
        }

        private void OnPortalEntered(PortalController portal, Collider collider)
        {
            if (collider.CompareTag("Player"))
            {
                if (m_VictoryCoroutine == null)
                {
                    m_VictoryCoroutine = StartCoroutine(DelayedLevelCompleteCoroutine(m_VictoryDuration));
                }

                MusicManager.Instance.PlayEscapeMusic();

                RogueLiteManager.persistentData.isDirty = true;
                RogueLiteManager.SaveProfile();
            }
        }

        internal void RegisterSpawner(Spawner spawner)
        {
            if (spawners.Contains(spawner))
            {
                return;
            }

            if (spawner.isBossSpawner)
            {
                bossSpawnersRemaining++;
            }

            spawner.onSpawnerDestroyed.AddListener(OnSpawnerDestroyed);

            spawners.Add(spawner);
            onSpawnerCreated?.Invoke(spawner);
        }

        internal void RegisterEnemy(BasicEnemyController enemy)
        {
            enemy.onDeath.AddListener(OnEnemyDeath);
            onEnemySpawned?.Invoke(enemy);
        }

        private void OnEnemyDeath(BasicEnemyController enemy)
        {
            enemy.onDeath.RemoveListener(OnEnemyDeath);
        }

        private void updateHUD()
        {
            statusHud.UpdateEnemyCount(aiDirector.enemies.Count);
            statusHud.UpdateSpawnerCount(bossSpawnerCount);
        }

        internal WaveDefinition GetEnemyWaveFromBossSpawner()
        {
            WaveDefinition wave = spawners[0].currentWave;
            if (wave == null)
            {
                wave = spawners[0].lastWave;
            }
            return wave;
        }

        #endregion
    }
}