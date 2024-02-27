using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    public class RWPooledExplosion : PooledExplosion
    {
        protected static List<IHealthManager> s_HealthManagers = new List<IHealthManager>(8);

        public override void Explode(float maxDamage, float maxForce, IDamageSource source = null, Transform ignoreRoot = null)
        {
            s_HealthManagers.Clear();
            base.Explode(maxDamage, maxForce, source, ignoreRoot);
        }

        protected override void CheckCollider(Collider c, Vector3 explosionCenter)
        {
            IHealthManager healthManager = c.GetComponentInParent<IHealthManager>();
            if (healthManager != null && s_HealthManagers.Contains(healthManager) == false)
            {
                s_HealthManagers.Add(healthManager);
                base.CheckCollider(c, explosionCenter);
            }
        }
    }
}