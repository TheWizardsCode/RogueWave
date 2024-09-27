using NeoFPS;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    public class RW_EnemyHitFxBehaviour : ParticleSystemHitFxBehaviour
    {
        public override void Hit(GameObject hitObject, Vector3 position, Vector3 normal, Vector3 ray, float size, bool decal)
        {
            // NOTE this requires the m_ChipsSystem and m_DustSystem to be made protected in the base class

            if (hitObject != null)
            {
                var particleSystemRenderer = m_ChipsSystem.GetComponentInParent<ParticleSystemRenderer>();
                Renderer renderer = hitObject.GetComponent<Renderer>();
                if (particleSystemRenderer != null && renderer != null)
                {
                    particleSystemRenderer.material = renderer.material;
                }
            }

            base.Hit(hitObject, position, normal, ray, size, decal);
        }
    }
}
