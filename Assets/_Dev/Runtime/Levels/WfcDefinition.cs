using NaughtyAttributes;
using NeoFPS;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace RogueWave
{
    /// <summary>
    /// Defines how the Wave Function Collapse is to be applied. This is used to desribe both the level and tile layouts of the game.
    /// </summary>
    /// <seealso cref="WaveDefinition"/>
    [CreateAssetMenu(fileName = "New WFC Definition", menuName = "Rogue Wave/WFC Definition", order = 200)]
    public class WfcDefinition : ScriptableObject
    {
        [Header("Size and Layout")]
        [SerializeField, Tooltip("The seed to use for the level generation. If this is set to <= 0 then a random seed will be used.")]
        internal int seed = 0;
        [SerializeField, Tooltip("The size of the lot to generate the level in. This is used to determine the size of the level geometry. Each tile should fill this space.")]
        internal Vector2 lotSize = new Vector2(50f, 50f);
        [SerializeField, Tooltip("The size of the level in tiles."), FormerlySerializedAs("size")]
        internal Vector2Int mapSize = new Vector2Int(20, 20);
        [SerializeField, Tooltip("If true then the entire level will be enclosed by a wall.")]
        internal bool encloseLevel = true;

        [Header("Tile Types")]
        [SerializeField, Tooltip("The tile to use for tiles that do not have a valid tile based on their surroundings. In general it shouldn't be used in the level at all. It is here as a fallback in case the level is not well defined."), Expandable]
        [FormerlySerializedAs("emptyTileDefinition")]
        internal TileDefinition defaultTileDefinition;
        [SerializeField, Tooltip("The tile to use for boundary walls. Walls will attempt to autoconnect to adjacent tiles."), ShowIf("encloseLevel"), Expandable]
        internal TileDefinition wallTileDefinition;

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
        [SerializeField, Tooltip("The tiles to place in the level before any other generation is done. This is useful for things such as placing the player start position and objective tiles.."), Expandable]
        public TileDefinition[] prePlacedTiles;

        [Header("Audio")]
        [SerializeField, Tooltip("The audio to play when the level is ready. This will start playing once the level has been loaded/generated. Usually ths will be during the loudout screen. This might include a Nanobot announcement about the level, for example.")]
        internal AudioClip[] levelReadyAudioClips = new AudioClip[0];
        [SerializeField, Tooltip("The audio to play when the level is complete. This might include a Nanobot announcement about the level, for example.")]
        internal AudioClip[] levelCompleteAudioClips = new AudioClip[0];
        [SerializeField, Tooltip("The audio to play when the level is failed. This might include a Nanobot announcement about the level, for example.")]
        internal AudioClip[] deathAudioClips = new AudioClip[0];

        public string DisplayName => name;

        /// <summary>
        /// Get the Challenge Rating for this level. The higher this value the more difficult the level will be to complete.
        /// </summary>
        public int challengeRating
        {
            get
            {
                int cr = 0;
                foreach (var wave in waves)
                {
                    cr += wave.CR;
                }

                return cr;
            }
        }

        public string Description
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(DisplayName);
                sb.AppendLine($"Challenge Rating of {challengeRating}");

                HashSet<string> uniqueEnemies = new HashSet<string>();
                foreach (WaveDefinition wave in waves)
                {
                    foreach (var enemy in wave.Enemies)
                    {
                        uniqueEnemies.Add(enemy.pooledEnemyPrefab.name);
                    }
                }
                sb.AppendLine($"<size=60%>({string.Join(", ", uniqueEnemies)})</size>");
                sb.AppendLine();

                StringBuilder contains = new StringBuilder();
                foreach (TileDefinition tile in prePlacedTiles)
                {
                    if (tile.icon == null)
                    {
                        continue;
                    }
                    contains.AppendLine($"\t{tile.name}");
                }

                if (contains.Length > 0)
                {
                    sb.AppendLine("Contains:");
                    sb.Append(contains.ToString());
                }

                return sb.ToString();
            }
        }

        public WaveDefinition[] Waves => waves;

        public float WaveWait => waveWait;

        public bool GenerateNewWaves => generateNewWaves;

        internal float Duration
        {
            get
            {
                float duration = 0;
                foreach (var wave in waves)
                {
                    duration += wave.WaveDuration;
                }
                return duration;
            }
        }

        /// <summary>
        /// Get a random enemy from the first wave defined in this level.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal PooledObject GetRandomEnemy()
        {
            if (waves.Length == 0)
            {
                Debug.LogWarning($"No waves defined in {this}. Either turn of enemy generation or add a wave definition.");
                return null;
            }
            return waves[0].GetNextEnemy(); ;
        }
    }
}
