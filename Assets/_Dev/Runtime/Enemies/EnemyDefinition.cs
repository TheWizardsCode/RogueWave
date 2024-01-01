using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
    /// <summary>
    /// EnemyDefinition is a ScriptableObject that defines the behaviour of an enemy.
    /// It is used as a basis for configuring the BasicEnemyController for new enemies.
    /// </summary>
    [CreateAssetMenu(fileName = "Enemy Definition", menuName = "Playground/Enemy Definition", order = 300)]
    public class EnemyDefinition : ScriptableObject
    {
        [SerializeField, Tooltip("The drone sound to play for this enemy."), Foldout("Clips")]
        internal AudioClip _droneClip = null;

        [SerializeField, Tooltip("The sound to play when the enemy is killed."), Foldout("Clips")]
        internal AudioClip[] deathClips;

        /// <summary>
        /// The drone is the sound that plays when the enemy is alive.
        /// It is a looping sound.
        /// </summary>
        public AudioClip droneClip
        {
            get { return _droneClip; }
        }

        public AudioClip GetDeathClip()
        {
            if (deathClips.Length > 0)
            {
                return deathClips[Random.Range(0, deathClips.Length)];
            }
            else
            {
                return null;
            }
        }
    }
}