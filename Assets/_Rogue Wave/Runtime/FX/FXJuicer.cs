using NeoFPS;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RogueWave
{
    /// <summary>
    /// FXJuicer is responsible for adding juice to a gameobject when it is spawned, injured, killed or other events.
    /// 
    /// Place it anywhere on an object and ensure that the Juices sections of the config are setup. The juices will be added at the location of this components transform.
    /// </summary>
    [Obsolete("Use More Mountains Feel instead.")]
    public class FXJuicer : PooledObject
    {
        [Header("Setup")]
        [SerializeField, Tooltip("The audio source for this enemy.")]
        AudioSource audioSource = null;

        [Header("Alive")]
        [SerializeField, Tooltip("The drone sound to play for this enemy.")]
        internal AudioClip _droneClip = null;
        [SerializeField, Tooltip("The volume of the drone sound."), Range(0,1)]
        internal float droneVolume = 1.0f;

        [Header("Death")]
        [SerializeField, Tooltip("The Game object which has the juice to add when the enemy is killed, for example any particles, sounds or explosions.")]
        internal PooledObject deathJuicePrefab;
        [SerializeField, Tooltip("The offset from the enemy's position to spawn the juice.")]
        internal Vector3 juiceOffset = Vector3.zero;
        [SerializeField, Tooltip("The sound to play when the enemy is killed.")]
        internal AudioClip[] deathClips;
    }
}