using log4net.Util;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Playground
{
    /// <summary>
    /// EnemyBehaviourDefinition is a ScriptableObject that defines the behaviour of an enemy.
    /// It is used as a basis for configuring the BasicEnemyController for new enemies.
    /// </summary>
    [CreateAssetMenu(fileName = "Enemy Behaviour Definition", menuName = "Playground/Enemy Behaviour Definition", order = 300)]
    public class EnemyBehaviourDefinition : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField, Tooltip("How fast the enemy moves.")]
        internal float speed = 5f;
        [SerializeField, Tooltip("How fast the enemy rotates.")]
        internal float rotationSpeed = 1f;
        [SerializeField, Tooltip("The minimum height the enemy will move to.")]
        internal float minimumHeight = 0.5f;
        [SerializeField, Tooltip("The maximum height the enemy will move to.")]
        internal float maximumHeight = 75f;
        [SerializeField, Tooltip("How close to the player will this enemy try to get?")]
        internal float optimalDistanceFromPlayer = 0.2f;

        [Header("Seek Behaviour")]
        [SerializeField, Tooltip("How long the enemy will seek out the player for after losing sight of them."), Foldout("Behaviour")]
        internal float seekDuration = 7;
        [SerializeField, Tooltip("The maximum distance the enemy will wander from their spawn point. The enemy will move further away than this when they are chasing the player but will return to within this range if they go back to a wandering state."), Foldout("Behaviour")]
        internal float maxWanderRange = 30f;
    }
}