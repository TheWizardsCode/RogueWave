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
