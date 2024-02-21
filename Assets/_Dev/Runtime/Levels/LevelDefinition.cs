using System;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// Defines a single level in the game. If the player survives a level they increase in experience.
    /// Each level consists of one or more waves of enemies to spawn.
    /// </summary>
    /// <seealso cref="WaveDefinition"/>
    [CreateAssetMenu(fileName = "LevelDefinition", menuName = "Rogue Wave/Level Definition", order = 200)]
    public class LevelDefinition : ScriptableObject
    {
        [Header("Size and Layout")]
        [SerializeField, Tooltip("The seed to use for the level generation. If this is set to <= 0 then a random seed will be used.")]
        internal int seed = 0;
        [SerializeField, Tooltip("The size of the level in square meters.")]
        internal Vector2 size = new Vector2(500f, 500f);

        [Header("Tile Types")]
        [SerializeField, Tooltip("The tile to use for boundary walls. Walls will attempt to autoconnect to adjacent tiles.")]
        internal TileDefinition wallTileDefinition;
        [SerializeField, Tooltip("The tile definitions to use for this level. If a tile is defined in the tile constraints but does not appear in this list it will not be used. This allows level definitions to be reused in different ways.")]
        internal TileDefinition[] tileDefinitions;
        [SerializeField, Tooltip("The tile to use for spawners.")]
        internal TileDefinition spawnerTileDefinition;
        [SerializeField, Tooltip("The tile to use for empty tiles. In general it shouldn't be used in the level at all. It is here as a fallback in case the level is not well defined.")]
        internal TileDefinition emptyTileDefinition;

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
        [SerializeField, Tooltip("The tiles to place in the level before any other generation is done. This is useful for things such as placing the player start position and objective tiles..")]
        public TileDefinition[] prePlacedTiles;


        public WaveDefinition[] Waves => waves;

        public float WaveWait => waveWait;

        public bool GenerateNewWaves => generateNewWaves;

        internal Vector2 lotSize = new Vector2(25f, 25f);
    }
}
