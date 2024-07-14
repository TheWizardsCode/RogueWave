using NeoFPS;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// Thrown Turret Projectile is a projectile for a thrown weapon that when it collides with a surface, it spawns a turret.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ThrownTurretProjectile : ThrownWeaponProjectile
    {
        [SerializeField, NeoPrefabField(required = true), Tooltip("The turret prefab to spawn upon collision with a surface.")]
        private PooledObject m_Turret = null;
        [SerializeField, Tooltip("An FX object to spawn whan the turret is spawned.")]
        private PooledObject m_SpawnFX = null;
        [SerializeField, Tooltip("The time in seconds before the projectile spawns the turret."), Range(0.25f, 5f)]
        private float m_ActiveTime = 5f;

        private PooledObject m_Prototype = null;
        private Rigidbody m_Rigidbody = null;
        private float m_SpawnTime = 0f;

        protected override void Awake()
        {
            base.Awake();
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Prototype = m_Turret.GetComponent<PooledObject>();
        }

        public override void Throw(Vector3 velocity, IDamageSource source)
        {
            velocity /= 2;
            base.Throw(velocity, source);
            m_Rigidbody.velocity = velocity;
            m_Rigidbody.angularVelocity = new Vector3(Random.Range(-2.0f, 2.0f), Random.Range(0.0f, 3f), Random.Range(-2.0f, 2.0f));
            m_SpawnTime = Time.timeSinceLevelLoad + m_ActiveTime;
        }

        private void Update()
        {
            if (m_SpawnTime > Time.timeSinceLevelLoad)
            {
                return;
            }

            PoolManager.GetPooledObject<NanobotPawnController>(m_Prototype, transform.position, Quaternion.identity);
            
            PooledObject fx = PoolManager.GetPooledObject<PooledObject>(m_SpawnFX, transform.position, Quaternion.identity);
            fx.ReturnToPool(10);

            pooledObject.ReturnToPool();
        }

        private static readonly NeoSerializationKey k_TimerKey = new NeoSerializationKey("timer");

        public override void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_TimerKey, m_SpawnTime);
        }

        public override void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_TimerKey, out m_SpawnTime, 0f);
        }
    }
}
