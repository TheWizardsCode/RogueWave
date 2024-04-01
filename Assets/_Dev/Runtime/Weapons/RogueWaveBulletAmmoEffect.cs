using NeoFPS;
using NeoFPS.ModularFirearms;
using NeoFPS.SinglePlayer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RogueWave
{
    public class RogueWaveBulletAmmoEffect : BaseAmmoEffect
    {
        [SerializeField, Tooltip("The damage the bullet does.")]
        internal float damage = 15f;
        [SerializeField, Tooltip("The size of the bullet. Used to size decals.")]
        private float bulletSize = 1f;
        [SerializeField, Tooltip("The force to be imparted onto the hit object. Requires either a [Rigidbody][unity-rigidbody] or an impact handler.")]
        private float impactForce = 15f;

        bool effectApplied = false;
        private NanobotManager nanobotManager;
        List<AmmunitionEffectUpgradeRecipe> availableRecipes;
        List<string> appliedRecipeIDs = new List<string>();

        private static List<IDamageHandler> damageHandlers = new List<IDamageHandler>(4);


        public override void Hit(RaycastHit hit, Vector3 rayDirection, float totalDistance, float speed, IDamageSource damageSource)
        {
            SurfaceManager.ShowBulletHit(hit, rayDirection, bulletSize, hit.rigidbody != null);
            
            hit = ApplyDamage(hit, damageSource);

            ApplyHitForce(hit, rayDirection);
        }

        internal RaycastHit ApplyDamage(RaycastHit hit, IDamageSource damageSource)
        {
            if (damage > 0f)
            {
                hit.collider.GetComponents(damageHandlers);
                for (int i = 0; i < damageHandlers.Count; ++i)
                {
                    if (damageHandlers[i].enabled)
                        damageHandlers[i].AddDamage(damage, hit, damageSource);
                }
                damageHandlers.Clear();
            }

            return hit;
        }

        internal void ApplyHitForce(RaycastHit hit, Vector3 rayDirection)
        {
            if (hit.collider != null && impactForce > 0f)
            {
                IImpactHandler impactHandler = hit.collider.GetComponent<IImpactHandler>();
                if (impactHandler != null)
                    impactHandler.HandlePointImpact(hit.point, rayDirection * impactForce);
                else
                {
                    if (hit.rigidbody != null)
                        hit.rigidbody.AddForceAtPosition(rayDirection * impactForce, hit.point, ForceMode.Impulse);
                }
            }
        }

        private void OnEnable()
        {
            if (!effectApplied)
            {
                nanobotManager = FpsSoloCharacter.localPlayerCharacter.GetComponent<NanobotManager>();

                effectApplied = true;
            }

            availableRecipes = nanobotManager.GetAmmunitionEffectUpgradesFor(GetComponent<SharedPoolAmmo>().ammoType);

            foreach (AmmunitionEffectUpgradeRecipe recipe in availableRecipes)
            {
                if (!appliedRecipeIDs.Contains(recipe.uniqueID))
                {
                    int count = availableRecipes.Count(r => r.uniqueID == recipe.uniqueID);
                    while (count-- > 0)
                    {
                        recipe.Apply(this);
                    }
                    appliedRecipeIDs.Add(recipe.uniqueID);
                }
            }
        }
    }
}