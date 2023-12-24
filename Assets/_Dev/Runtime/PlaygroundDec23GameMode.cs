using UnityEngine;
using NeoFPS.SinglePlayer;
using System;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using Playground;
using NeoFPS;
using UnityEngine.Events;
using NeoSaveGames.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Playground
{
    public class PlaygroundDecember23GameMode : FpsSoloGameCustomisable, ISpawnZoneSelector, ILoadoutBuilder
    {
        [Header("Character")]
        [SerializeField, NeoPrefabField(required = true), Tooltip("The player prefab to instantiate if none exists.")]
        private FpsSoloPlayerController m_PlayerPrefab = null;
        [SerializeField, NeoPrefabField(required = true), Tooltip("The character prefab to use.")]
        private FpsSoloCharacter m_CharacterPrefab = null;

        [Header("Level Generation")]
        [SerializeField, Tooltip("Should a level be auto generated on start?")]
        bool generateLevelOnSpawn = true;

        [Header("Victory")]
        [SerializeField, Tooltip("The amount of time to wait after victory before heading to the hub")]
        float m_VictoryDuration = 5f;

        LevelGenerator levelGenerator;
        private int spawnersRemaining = int.MaxValue;

        #region Unity Life-cycle
        protected override void Awake()
        {
            levelGenerator = GetComponentInChildren<LevelGenerator>();
            base.Awake();
        }

        private void Update()
        {
            if (spawnersRemaining == 0 && m_VictoryCoroutine == null)
            {
                m_VictoryCoroutine = StartCoroutine(DelayedVictoryCoroutine(m_VictoryDuration));
            }
        }
        #endregion

        #region Game Events

        private Coroutine m_VictoryCoroutine = null;
        private float m_VictoryTimer = 0f;

        public static event UnityAction onVictory;

        internal void SpawnerDestroyed()
        {
            spawnersRemaining--;
        }

        protected override void DelayedDeathAction()
        {
            RogueLiteManager.RunLoadoutData.Clear();

            NeoSceneManager.LoadScene(RogueLiteManager.hubScene);
        }

        void DelayedVictoryAction()
        {
            NeoSceneManager.LoadScene(RogueLiteManager.hubScene);
        }

        private IEnumerator DelayedVictoryCoroutine(float delay)
        {
            onVictory?.Invoke();

            yield return null;

            // Delay timer
            m_VictoryTimer = delay;
            while (m_VictoryTimer > 0f && !SkipDelayedDeathReaction())
            {
                m_VictoryTimer -= Time.deltaTime;
                yield return null;
            }

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

        [SerializeField, Tooltip("The loadouts that are available to use.")]
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

        public void AddToLoadout(FpsInventoryItemBase item)
        {
            if (!RogueLiteManager.RunLoadoutData.Contains(item)) {
                RogueLiteManager.RunLoadoutData.Add(item);
            }
        }

        protected override void OnCharacterSpawned(ICharacter character)
        {
            // Apply inventory loadout
            var loadout = m_LoadoutBuilder.GetLoadout();
            if (loadout != null)
                character.GetComponent<IInventory>()?.ApplyLoadout(loadout);
        }

        #endregion

        protected override bool PreSpawnStep()
        {
            if (generateLevelOnSpawn)
            {
                spawnersRemaining = levelGenerator.Generate(this);
            }

            for (int i = 0; i < RogueLiteManager.RunLoadoutData.Count; i++)
            {
                m_LoadoutBuilder.slots[0].AddOption(RogueLiteManager.RunLoadoutData[0] as FpsInventoryItemBase);
            }

            return base.PreSpawnStep();
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
    }

    public static class LoadoutBuilderSlotExtensions
    {
        public static void AddOption(this LoadoutBuilderSlot slot, FpsInventoryItemBase option)
        {
            var field = typeof(LoadoutBuilderSlot).GetField("m_Options", BindingFlags.NonPublic | BindingFlags.Instance);
            var options = (FpsInventoryItemBase[])field.GetValue(slot);
            var optionsList = new List<FpsInventoryItemBase>(options) { option };
            field.SetValue(slot, optionsList.ToArray());
        }
    }

}
