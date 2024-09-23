using NaughtyAttributes;
using NeoFPS;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    public class BasicProjectileMotor : MonoBehaviour
    {
        [InfoBox("This is a simple projectile motor that moves the projectile in a straight line at a constant speed. It will be destroyed after a set amount of time. The parameters should be set in the weapon that fires this projectile.")]

        [SerializeField, Tooltip("The impact effect to display when the weapon hits a target.")]
        internal ParticleSystem impactEffect;

        [ShowNonSerializedField, Tooltip("Inidcates if this motor has been initialized. In the editor this will be false but it should be true when the object is enabled in game.")]
        internal bool isInitialized = false;
        [ShowNonSerializedField, Tooltip("The speed of the motor. This will normally be overridden by the weapon on instantiation, suring the `Initialize` process.")]
        internal float speed = 10f;
        [ShowNonSerializedField, Tooltip("The lifetime of the projectile. This will normally be overridden by the weapon on instantiation, suring the `Initialize` process.")]
        internal float lifeTime = 5f;

        private float endTime;

        PooledObject pooledObject;

        private void Start()
        {
            pooledObject = GetComponent<PooledObject>();
        }

        private void OnEnable()
        {
            endTime = Time.time + lifeTime;
        }

        private void OnDisable()
        {
            Reset();
        }

        private void Reset()
        {
            isInitialized = false;
        }

        internal virtual void Initialize(float speed, float lifeTime)
        {
            isInitialized = true;
            this.speed = speed;
            this.lifeTime = lifeTime;
        }

        protected virtual void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;

            if (Time.time > endTime)
            {
                if (!isInitialized)
                {
                    Debug.LogError($"{this.name} has not been initialized. This is likely because the weapon that fired it has not called Initialize. Using defaults for now.");
                }

                if (pooledObject != null)
                {
                    pooledObject.ReturnToPool();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
