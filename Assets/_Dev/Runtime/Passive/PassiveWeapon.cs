using NaughtyAttributes;
using NeoFPS.ModularFirearms;
using NeoFPS.WieldableTools;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace RogueWave
{
    /// <summary>
    /// A passive weapon is one which will fire automatically based on a timer.
    /// </summary>
    public class PassiveWeapon : MonoBehaviour
    {
        [Header("Damage")]
        [SerializeField, Tooltip("The maximum area of the damage.")]
        [FormerlySerializedAs("radius")]
        internal float range = 20f;
        [SerializeField, Tooltip("The damage applied to each enemy within the area of effect, each time the weapon fires.")]
        internal float damage = 50f;
        [SerializeField, Layer, Tooltip("The layers that the weapon will damage.")]
        internal int layers;

        [Header("Visuals")]
        [SerializeField, Tooltip("The model to display when the weapon is active.")]
        internal GameObject model;

        [SerializeField, Tooltip("Cooldown between trigger pulls, in game seconds.")]
        internal float m_Cooldown = 5f;

        internal float m_NextFireTime = 0;
        ModularFirearm m_Firearm;

        private void Awake()
        {
            m_Firearm = GetComponent<ModularFirearm>();
        }

        public virtual bool isValid
        {
            get { return m_NextFireTime < Time.timeSinceLevelLoad; }
        }

        protected void Update()
        {
            if (isValid)
            {
                Fire();
            }
        }

        public virtual void Fire()
        {
            m_NextFireTime = Time.timeSinceLevelLoad + m_Cooldown;

            m_Firearm.trigger.Press();
        }
    }
}