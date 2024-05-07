using NeoFPS;
using ProceduralToolkit;
using UnityEngine;
using static NeoFPS.HealthDelegates;

namespace RogueWave
{
    /// <summary>
    /// Tracks and displays a representation of the players health on the Nanobot Pawn.
    /// </summary>
    public class PawnHealthTracker : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("The colour to use when the player is at full health.")]
        [ColorUsage(true, true)]
        Color fullHealthColour = Color.green;
        [SerializeField, Tooltip("The colour to use when the player is at zero health.")]
        [ColorUsage(true, true)]
        Color noHealthColour = Color.red;

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
            float adjustedHealthRatio = Mathf.Pow(healthRatio, 0.5f); // Adjust this exponent to change the rate of color change

            Color healthColour = Color.Lerp(noHealthColour, fullHealthColour, adjustedHealthRatio);
            material.SetColor("_Color", healthColour);
        }
    }
}