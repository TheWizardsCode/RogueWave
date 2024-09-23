using NeoFPS;
using UnityEngine;
using WizardsCode.RogueWave;

namespace RogueWave
{
    public class PassiveProjectileWeapon : PassiveWeapon 
    {
        [SerializeField, Tooltip("The projectile to fire.")]
        internal PooledObject m_ProjectilePrototype;

        public override void Fire()
        {
            BasicProjectileMotor projectile = PoolManager.GetPooledObject<BasicProjectileMotor>(m_ProjectilePrototype, transform.position + positionOffset, nanobotPawn.player.transform.rotation);
            
            Collider target = nanobotPawn.GetNearestObject().Value;
            if (projectile is TrackingProjectileMotor tracker)
            {
                if (target == null)
                {
                    tracker.Initialize(m_Speed, range / m_Speed, null);
                } 
                else
                {
                    tracker.Initialize(m_Speed, range / m_Speed, target.transform);
                }
            } else
            {
                projectile.Initialize(m_Speed, range / m_Speed);
            }

            projectile.GetComponent<ExplodeOnContact>().Initialize(damage);

            base.Fire();
        }

    }
}
