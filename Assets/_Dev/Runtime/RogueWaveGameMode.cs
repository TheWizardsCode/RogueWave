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

namespace RogueWave
{
    public class RogueWaveGameMode : FpsSoloGameCustomisable, ISpawnZoneSelector, ILoadoutBuilder
    {
        [Header("Victory")]
        [SerializeField, Tooltip("The amount of time to wait after victory before heading to the hub")]
        float m_VictoryDuration = 5f;

        [Header("Character")]
        [SerializeField, Tooltip("If true, the player will spawn at a random spawn point. If false, the player will spawn at the first spawn point in the scene.")]
        internal bool randomizePlayerSpawn = true;
        [SerializeField, NeoPrefabField(required = true), Tooltip("The player prefab to instantiate if none exists.")]
        private FpsSoloPlayerController m_PlayerPrefab = null;
        [SerializeField, NeoPrefabField(required = true), Tooltip("The character prefab to use.")]
        private FpsSoloCharacter m_CharacterPrefab = null;
        private float initialHealth = 30;
        [SerializeField, Tooltip("The recipes that will be available to the player at the start of each run, regardless of resources.")]
        private AbstractRecipe[] _startingRecipes;

        [Header("Level Management")]
        [SerializeField, Tooltip("The level definitions which define the enemies, geometry and more for each level.")]
        LevelDefinition[] levels;

        [SerializeField, Tooltip("Turn on debug mode for this Game Mode"), Foldout("Debug")]
        private bool _isDebug = false;

        LevelProgressBar levelProgressBar;

        private int spawnersRemaining = 0;
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
                if (_levelGenerator != null && _levelGenerator != value)
                {
                    _levelGenerator.onSpawnerCreated.RemoveListener(OnSpawnerCreated);
                }
                _levelGenerator = value;
            }
        }

        public LevelDefinition currentLevelDefinition
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
            levelGenerator.onSpawnerCreated.AddListener(OnSpawnerCreated);

            levelProgressBar = FindObjectOfType<LevelProgressBar>(true);
            if (levelProgressBar == null)
            {
                Debug.LogError("No LevelProgressBar found in the scene. Please add one to the scene.");
            }

            base.Awake();
        }

        protected override void OnDestroy()
        {
            levelGenerator.onSpawnerCreated.RemoveListener(OnSpawnerCreated);

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
            spawnersRemaining++;
            spawner.onDestroyed.AddListener(OnSpawnerDestroyed);
        }

        internal void OnSpawnerDestroyed(Spawner spawner)
        {
            spawnersRemaining--;

            if (!_isDebug && spawnersRemaining == 0 && m_VictoryCoroutine == null)
            {
                m_VictoryCoroutine = StartCoroutine(DelayedVictoryCoroutine(m_VictoryDuration));
            }
        }

        protected override void DelayedDeathAction()
        {
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
            if (magnet != null) {
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

            if (inGame)
                DelayedVictoryAction();
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
        }

        private void OnCharacterIsAliveChanged(ICharacter character, bool alive)
        {
            if (alive == false)
            {
                RogueLiteManager.ResetRunData();
                character.onIsAliveChanged -= OnCharacterIsAliveChanged;
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
                levelGenerator.Generate(this);
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

        #endregion
    }
}