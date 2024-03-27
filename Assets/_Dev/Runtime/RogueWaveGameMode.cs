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
using Steamworks;
using WizardsCode.GameStats;
using Codice.Client.BaseCommands;
using System;
using System.Collections.Generic;

namespace RogueWave
{
    public class RogueWaveGameMode : FpsSoloGameCustomisable, ISpawnZoneSelector, ILoadoutBuilder
    {
        [Header("Victory")]
        [SerializeField, Tooltip("The amount of time to wait after victory before heading to the hub")]
        float m_VictoryDuration = 5f;

        [Header("Character")]
        [SerializeField, NeoPrefabField(required = true), Tooltip("The player prefab to instantiate if none exists.")]
        private FpsSoloPlayerController m_PlayerPrefab = null;
        [SerializeField, NeoPrefabField(required = true), Tooltip("The character prefab to use.")]
        private FpsSoloCharacter m_CharacterPrefab = null;
        private float initialHealth = 30;
        [SerializeField, Tooltip("The recipes that will be available to the player at the start of each run, regardless of resources.")]
        private AbstractRecipe[] _startingRecipes;

        [Header("Level Management")]
        [SerializeField, Tooltip("The level definitions which define the enemies, geometry and more for each level."), Expandable]
        WfcDefinition[] levels;

        // Game Stats
        [SerializeField, Expandable, Foldout("Game Stats"), Tooltip("The count of succesful runs in the game.")]
        private GameStat m_VictoryCount;
        [SerializeField, Expandable, Foldout("Game Stats"), Tooltip("The count of deaths in the game.")]
        private GameStat m_DeathCount;
        [SerializeField, Expandable, Foldout("Game Stats"), Tooltip("The time player stat for recording how long a player has been inside runs.")]
        private GameStat m_TimePlayedStat;

        [Header("Events")]
        [SerializeField, Tooltip("The event to trigger when the level generator creates a spawner.")]
        public UnityEvent<Spawner> onSpawnerCreated;

        [SerializeField, Tooltip("Turn on debug mode for this Game Mode"), Foldout("Debug")]
        private bool _isDebug = false;

        LevelProgressBar levelProgressBar;

        private int bossSpawnersRemaining = 0;
        private float timeInLevel;

        LevelGenerator _levelGenerator;
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
                if (levels.Length <= RogueLiteManager.persistentData.currentGameLevel)
                    return levels[levels.Length - 1]; 
                else
                    return levels[RogueLiteManager.persistentData.currentGameLevel];
            }
        }

        #region Unity Life-cycle
        protected override void Awake()
        {
            levelGenerator = GetComponentInChildren<LevelGenerator>();

            levelProgressBar = FindObjectOfType<LevelProgressBar>(true);
            if (levelProgressBar == null)
            {
                Debug.LogError("No LevelProgressBar found in the scene. Please add one to the scene.");
            }

            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void Update()
        {
            if (FpsSoloCharacter.localPlayerCharacter == null) 
            {
                return;
            }

            timeInLevel += Time.deltaTime;
            levelProgressBar.Value = timeInLevel;
        }
        #endregion

        #region Game Events

        private Coroutine m_VictoryCoroutine = null;
        private float m_VictoryTimer = 0f;

        public static event UnityAction onVictory;

        internal void OnSpawnerCreated(Spawner spawner)
        {
            bossSpawnersRemaining++;
            spawner.onSpawnerDestroyed.AddListener(OnSpawnerDestroyed);
        }

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

            if (bossSpawnersRemaining == 0 && m_VictoryCoroutine == null)
            {
                m_VictoryCoroutine = StartCoroutine(DelayedVictoryCoroutine(m_VictoryDuration));
            }
        }

        protected override void DelayedDeathAction()
        {
            SendData();

            RogueLiteManager.ResetRunData();

            NeoSceneManager.LoadScene(RogueLiteManager.hubScene);
        }

        void DelayedVictoryAction()
        {
            NeoSceneManager.LoadScene(RogueLiteManager.hubScene);
        }

        private IEnumerator DelayedVictoryCoroutine(float delay)
        {
            onVictory?.Invoke();

            // Temporary magnet buff to pull in victory rewards
            MagnetController magnet = FpsSoloCharacter.localPlayerCharacter.GetComponent<MagnetController>();
            float originalRange = 0;
            float originalSpeed = 0;
            if (magnet != null)
            {
                originalRange = magnet.range;
                originalSpeed = magnet.speed;
                magnet.range = 100;
                magnet.speed = 25;
            }

            yield return null;

            // Delay timer
            m_VictoryTimer = delay;
            while (m_VictoryTimer > 0f && !SkipDelayedDeathReaction())
            {
                m_VictoryTimer -= Time.deltaTime;
                yield return null;
            }

            // Reset magnet
            if (magnet != null)
            {
                magnet.range = originalRange;
                magnet.speed = originalSpeed;
            }

            RogueLiteManager.persistentData.currentGameLevel++;

            if (m_VictoryCount != null)
            {
                m_VictoryCount.Increment();
            }

            SendData();

            if (inGame)
                DelayedVictoryAction();
        }

        private void SendData()
        {
            float timePlayed = Time.time - startTime;
            if (m_TimePlayedStat != null)
            {
                m_TimePlayedStat.Increment(timePlayed);
            }

            GameStatsManager.Instance.SendDataToWebhook();
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

        protected override void OnCharacterSpawned(ICharacter character)
        {
            // Configure the Level Progress Bar
            levelProgressBar.gameObject.SetActive(true);
            levelProgressBar.MaxValue = currentLevelDefinition.Duration;
            levelProgressBar.MinValue = 0;
            levelProgressBar.Value = 0;
            levelProgressBar.levelDefinition = currentLevelDefinition;
            timeInLevel = 0;

            // Configure the character
            character.onIsAliveChanged += OnCharacterIsAliveChanged;

            BasicHealthManager healthManager = character.GetComponent<BasicHealthManager>();
            healthManager.healthMax = initialHealth;

            IRecipe startingWeapon;
            if (RogueLiteManager.persistentData.WeaponBuildOrder.Count > 0 && RecipeManager.TryGetRecipeFor(RogueLiteManager.persistentData.WeaponBuildOrder[0], out startingWeapon))
            {
                WeaponPickupRecipe weaponRecipe = startingWeapon as WeaponPickupRecipe;
                if (weaponRecipe != null)
                {
                    RogueLiteManager.runData.AddToLoadout(weaponRecipe.pickup.GetItemPrefab());
                }
            }

            var loadout = ConfigureLoadout();
            if (loadout != null)
                character.GetComponent<IInventory>()?.ApplyLoadout(loadout);

            // Add nanobot recipes
            NanobotManager manager = character.GetComponent<NanobotManager>();
            for (int i = 0; i < RogueLiteManager.runData.Recipes.Count; i++)
            {
                manager.Add(RogueLiteManager.runData.Recipes[i]);
            }

            // since a recipe may have adjusted the max health, we need to reset the health to the new max
            healthManager.health = healthManager.healthMax;

            startTime = Time.time;
        }

        private void OnCharacterIsAliveChanged(ICharacter character, bool alive)
        {
            if (alive == false)
            {
                RogueLiteManager.ResetRunData();
                character.onIsAliveChanged -= OnCharacterIsAliveChanged;


                m_DeathCount.Increment();
            }
        }

        #endregion

        private void ConfigureRecipe(string recipeId)
        {
            if (RecipeManager.TryGetRecipeFor(recipeId, out IRecipe recipe) == false)
            {
                Debug.LogError($"Attempt to configure a recipe with ID {recipeId} but no such recipe can be found. Ignoring this recipe.");
                return;
            }

            ConfigureRecipe(recipe);
        }

        private void ConfigureRecipe(IRecipe recipe)
        {
            if (RogueLiteManager.runData.Recipes.Contains(recipe) == false)
            {
                RogueLiteManager.runData.Recipes.Add(recipe);
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
            levelProgressBar.gameObject.SetActive(false);

            RogueLiteManager.runData.Loadout.Clear();

            RogueLiteManager.persistentData.runNumber++;

            if (RogueLiteManager.persistentData.WeaponBuildOrder.Count == 0)
            {
                for (int i = 0; i < _startingRecipes.Length; i++)
                {
                    if (_startingRecipes[i] is WeaponPickupRecipe)
                    {
                        RogueLiteManager.persistentData.WeaponBuildOrder.Add(_startingRecipes[i].uniqueID);
                    }
                }
            }

            if (currentLevelDefinition.generateLevelOnSpawn)
            {
                levelGenerator.Generate(currentLevelDefinition);
            }

            for (int i = 0; i < _startingRecipes.Length; i++)
            {
                ConfigureRecipe(_startingRecipes[i]);
            }

            for (int i = 0; i < RogueLiteManager.persistentData.RecipeIds.Count; i++)
            {
                ConfigureRecipe(RogueLiteManager.persistentData.RecipeIds[i]);
            }

            return base.PreSpawnStep();
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

        #endregion
    }
}