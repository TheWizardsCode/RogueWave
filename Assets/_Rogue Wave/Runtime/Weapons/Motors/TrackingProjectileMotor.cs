using NaughtyAttributes;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    public class TrackingProjectileMotor : BasicProjectileMotor
    {
        [Header("Tracking")]
        [SerializeField, Tooltip("The maximum angle in degrees that the projectile can turn in one frame.")]
        internal float maxTurnAngle = 1f;
        [SerializeField, Tooltip("The maximum distance that the projectile can turn in one frame.")]
        internal float maxTurnDistance = 1f;

        [ShowNonSerializedField, Tooltip("The target that the projectile is tracking.")]
        private Transform m_Target;
        public Transform Target
        {
            get { 
                return m_Target; 
            }
            internal set
            {
                m_Target = value; 
            }
        }

        internal override void Initialize(float speed, float lifeTime, LayerMask layerMask)
        {
            base.Initialize(speed, lifeTime, layerMask);
            isInitialized = false; // it's not sufficient to initialize the base class, we need to reset the flag to ensure that the target is also set.
        }

        internal void Initialize(float speed, float lifeTime, Transform target, LayerMask layerMask)
        {
            Initialize(speed, lifeTime, layerMask);
            m_Target = target;
            isInitialized = true;
        }

        protected override void Update()
        {
            if (Target && Target.transform.parent.gameObject.activeInHierarchy)
            {
                Vector3 targetDirection = m_Target.position - transform.position;
                Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, maxTurnAngle * Mathf.Deg2Rad, maxTurnDistance);
                transform.rotation = Quaternion.LookRotation(newDirection);
            }

            base.Update();
        }
    }
}
