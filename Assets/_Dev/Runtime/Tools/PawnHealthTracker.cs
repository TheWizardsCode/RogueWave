using NeoFPS;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// Tracks and displays a representation of the players health on the Nanobot Pawn.
    /// </summary>
    public class PawnHealthTracker : PlayerCharacterHudBase
    {
        private IHealthManager m_HealthManager = null;
        private Material material = null;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from old character
            if (m_HealthManager != null)
                m_HealthManager.onHealthChanged -= OnHealthChanged;
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            if (m_HealthManager != null)
                m_HealthManager.onHealthChanged -= OnHealthChanged;

            if (character as Component != null)
            {
                m_HealthManager = character.GetComponent<IHealthManager>();
            }
            else
            {
                m_HealthManager = null;
                return;
            }

            material = GetComponentInChildren<Renderer>().sharedMaterial;

            m_HealthManager.onHealthChanged += OnHealthChanged;
            OnHealthChanged(0f, m_HealthManager.health, false, null);
            gameObject.SetActive(true);
        }

        protected virtual void OnHealthChanged(float from, float to, bool critical, IDamageSource source)
        {
            float healthRatio = to / m_HealthManager.healthMax;
            material.SetFloat("_Health", healthRatio);
        }
    }
}