using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
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
        [Header("Senses")]
        [SerializeField, Tooltip("If true, the enemy will only move towards or attack the player if they have line of sight. If false they will always seek out the player.")]
        internal bool requireLineOfSight = true;
        [SerializeField, Tooltip("The maximum distance the character can see"), ShowIf("requireLineOfSight")]
        internal float viewDistance = 30f;
        [SerializeField, Tooltip("The layers the character can see"), ShowIf("isMobile")]
        internal LayerMask sensorMask = 0;

        [Header("Movement")]
        [SerializeField, Tooltip("Is this enemy mobile?")]
        internal bool isMobile = true;
        [SerializeField, Tooltip("How fast the enemy moves."), ShowIf("isMobile")]
        internal float speed = 5f;
        [SerializeField, Tooltip("How fast the enemy rotates."), ShowIf("isMobile")]
        internal float rotationSpeed = 1f;
        [SerializeField, Tooltip("The minimum height the enemy will move to."), ShowIf("isMobile")]
        internal float minimumHeight = 0.5f;
        [SerializeField, Tooltip("The maximum height the enemy will move to."), ShowIf("isMobile")]
        internal float maximumHeight = 75f;
        [SerializeField, Tooltip("How close to the player will this enemy try to get?"), ShowIf("isMobile")]
        internal float optimalDistanceFromPlayer = 0.2f;
        [SerializeField, Tooltip("The maximum distance the enemy will wander from their spawn point. A value of zero means do not wander. The enemy will move further away than this when they are chasing the player but will return to within this range if they go back to a wandering state."), ShowIf("isMobile")]
        internal float maxWanderRange = 30f;

        [Header("Navigation")]
        [SerializeField, Tooltip("The distance the enemy will try to avoid obstacles by."), ShowIf("isMobile")]
        internal float obstacleAvoidanceDistance = 2f;

        [Header("Seek Behaviour")]
        [SerializeField, Tooltip("How long the enemy will seek out the player for after losing sight of them."), ShowIf("isMobile")]
        internal float seekDuration = 7;

        [Header("Juice")]
        [SerializeField, Tooltip("The Game object which has the juice to add when the enemy is killed, for example any particles, sounds or explosions.")]
        [FormerlySerializedAs("deathParticlePrefab")]
        internal ParticleSystem juicePrefab;
        [SerializeField, Tooltip("The offset from the enemy's position to spawn the juice.")]
        internal Vector3 juiceOffset = Vector3.zero;
        [SerializeField, Tooltip("Set to true to generate a damaging and/or knock back explosion when the enemy is killed.")]
        internal bool shouldExplodeOnDeath = false;
        [SerializeField, ShowIf("shouldExplodeOnDeath"), Tooltip("The radius of the explosion when the enemy dies.")]
        internal float deathExplosionRadius = 5f;
        [SerializeField, ShowIf("shouldExplodeOnDeath"), Tooltip("The amount of damage the enemy does when it explodes on death.")]
        internal float explosionDamageOnDeath = 20;
        [SerializeField, ShowIf("shouldExplodeOnDeath"), Tooltip("The force of the explosion when the enemy dies.")]
        internal float explosionForceOnDeath = 15;

        [Header("Rewards")]
        [SerializeField, Tooltip("The chance of dropping a reward when killed.")]
        internal float resourcesDropChance = 0.5f;
        [SerializeField, Tooltip("The resources this enemy drops when killed.")]
        internal ResourcesPickup resourcesPrefab;

        internal bool shouldWander
        {
            get { return maxWanderRange > 0; }
        }
    }
}