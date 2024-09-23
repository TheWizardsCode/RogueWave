using NeoFPS;
using UnityEngine;

namespace RogueWave
{
    public class RW_HudAdvancedCrosshair : HudAdvancedCrosshair
    {
        private void OnEnable()
        {
            DamageEvents.onDamageHandlerHit += OnDamageHandlerHit;
        }

        private void OnDisable()
        {
            DamageEvents.onDamageHandlerHit -= OnDamageHandlerHit;
        }

        private void OnDamageHandlerHit(IDamageHandler handler, IDamageSource source, Vector3 hitPoint, DamageResult result, float damage)
        {
            if (result == DamageResult.Blocked)
            {
                return;
            }

            // Debug.Log($"{handler.name} was hit by {source} use crosshair {this.GetCurrentCrosshair()}");
            if (handler.GetComponentInParent<BuildingController>() != null)
            {
                return;
            } else
            {
                this.GetCurrentCrosshair().ShowHitMarker(result == DamageResult.Critical);
            }
        }
    }
}
