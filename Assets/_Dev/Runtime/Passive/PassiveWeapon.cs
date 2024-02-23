using NeoFPS.ModularFirearms;
using NeoFPS.WieldableTools;
using System.Collections;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// A passive weapon is one which will fire automatically based on a timer.
    /// </summary>
    public class PassiveWeapon : MonoBehaviour
    {
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