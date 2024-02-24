using NeoFPS.ModularFirearms;
using UnityEngine;

namespace RogueWave
{
    public class EnemyFirearmController : MonoBehaviour
    {
        [SerializeField, Tooltip("If true the weapon will aim at the player. Make this false for guided projectiles.")]
        private bool _AimAtPlayer = true;
        [SerializeField, Tooltip("Weapon cooldown time. This is the time that must elapse between presses of the triger. Note that settings on the weapon may restrict actual firing even further.")]
        private float _cooldown = 2f;
        [SerializeField, Tooltip("Weapon startup time. This is the time that must elapse between creation of the weapon and its first firing.")]
        private float _StartupTime = 3f;

        BasicEnemyController controller;

        private ModularFirearm m_Firearm = null;
        private bool m_TriggerDown = false;
        private float m_CooldownRemaining = 0;

        private void Start()
        {
            m_Firearm = GetComponent<ModularFirearm>();
            controller = GetComponentInParent<BasicEnemyController>();
        }

        private void LateUpdate()
        {
            if (_StartupTime > 0f)
            {
                _StartupTime -= Time.deltaTime;
                return;
            }

            m_CooldownRemaining -= Time.deltaTime;

            if (!m_TriggerDown && m_CooldownRemaining <= 0)
            {
                if (controller.shouldAttack)
                {
                    m_TriggerDown = true;
                    m_Firearm.trigger.Press();
                    m_CooldownRemaining = _cooldown;
                }
            }
            else if (m_TriggerDown)
            {
                m_TriggerDown = false;
                m_Firearm.trigger.Release();
                Debug.Log($"{this} release trigger. Target {controller.Target} is at position {controller.Target.position} (RB at {controller.Target.GetComponent<Rigidbody>().position})");
            }

            if (_AimAtPlayer && controller.Target != null)
            {
                transform.LookAt(controller.Target.position + Vector3.up * 1f);
            }
        }
    }
}