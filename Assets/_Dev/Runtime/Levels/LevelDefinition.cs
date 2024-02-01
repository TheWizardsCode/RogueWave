using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Playground
{
    /// <summary>
    /// Defines a single level in the game. If the player survives a level they increase in experience.
    /// Each level consists of one or more waves of enemies to spawn.
    /// </summary>
    /// <seealso cref="WaveDefinition"/>
    [CreateAssetMenu(fileName = "LevelDefinition", menuName = "Playground/Level Definition", order = 200)]
    public class LevelDefinition : ScriptableObject
    {
        [Header("Size and Layout")]
        [SerializeField, Tooltip("The size of the level in square meters.")]
        internal Vector2 size = new Vector2(500f, 500f);
        [SerializeField, Tooltip("The space to allocate for each building.")]
        internal Vector2 buildingLotSize = new Vector2(25f, 25f);
        [SerializeField, Range(0.1f, 1), Tooltip("How frequently buildings should be placed. Increase for a more dense level.")]
        internal float buildingDensity = 0.7f;
        
        [Header("Level Visuals")]
        [SerializeField, Tooltip("The material to apply to the ground.")]
        internal Material groundMaterial;
        [SerializeField, Tooltip("The material to apply to the walls.")]
        internal Material wallMaterial;
        [SerializeField, Tooltip("The prefabs to use for buildings without proximity spawners.")]
        internal GameObject[] buildingWithoutSpawnerPrefabs;
        [SerializeField, Tooltip("The prefabs to use for buildings with proximity spawners.")]
        internal GameObject[] buildingWithSpawnerPrefabs;

        [Header("Spawners")]
        [SerializeField, Tooltip("The number of Enemy Spawners to create.")]
        internal int numberOfEnemySpawners = 2;
        [SerializeField, Tooltip("The spawner to use for this level. This will be placed in a random lot that does not have a building in it.")]
        internal Spawner mainSpawnerPrefab;
        [SerializeField, Tooltip("The density of buildings that will contain a proximity spawner. These buildings will generate enemies if the player is nearby.")]
        internal float buildingSpawnerDensity = 0.25f;
        [SerializeField, Tooltip("The prefab to use when generating proximity spawners in buildings.")]
        internal Spawner buildingProximitySpawner;

        [Header("Enemies")]
        [SerializeField, Tooltip("The waves of enemies to spawn in this level.")]
        internal WaveDefinition[] waves;
        [SerializeField, Tooltip("The duration of the wait between each spawn wave in seconds.")]
        internal float waveWait = 5f;
        [SerializeField, Tooltip("If there are no more eaves defined should the spawners generate new ones?")]
        internal bool generateNewWaves = false;
        [SerializeField, Tooltip("The maximum number of enemies that can be alive at any one time. Note that this may be overridden in the spawner settings. If this is set to 0 then there is no limit.")]
        internal int maxAlive = 200;

        [Header("Level Generation")]
        [SerializeField, Tooltip("Should a level geometry be auto generated on start? If false it is expected that the scene will already contain the level geometry.")]
        public bool generateLevelOnSpawn = true;


        public WaveDefinition[] Waves => waves;

        public float WaveWait => waveWait;

        public bool GenerateNewWaves => generateNewWaves;
    }
}
