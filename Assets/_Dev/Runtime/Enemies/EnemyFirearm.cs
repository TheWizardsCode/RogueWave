using NeoFPS;
using NeoFPS.ModularFirearms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
    public class EnemyFirearm : MonoBehaviour
    {
        [SerializeField, Tooltip("If true the weapon will aim at the player. Make this false for guided projectiles.")]
        private bool _AimAtPlayer = true;

        BasicEnemyController controller;

        private ModularFirearm m_Firearm = null;
        private bool m_TriggerDown = false;

        private void Start()
        {
            m_Firearm = GetComponent<ModularFirearm>();
            controller = GetComponentInParent<BasicEnemyController>();
        }

        private void Update()
        {

            if (!m_TriggerDown)
            {
                if (controller.CanSeeTarget && controller.shouldAttack)
                {
                    m_TriggerDown = true;
                    m_Firearm.trigger.Press();
                }
            }
            else
            {
                if (!controller.CanSeeTarget || !controller.shouldAttack)
                {
                    m_TriggerDown = false;
                    m_Firearm.trigger.Release();
                }
            }

            if (_AimAtPlayer && controller.Target != null)
            {
                transform.LookAt(controller.Target.position + Vector3.up * 1f);
            }
        }
    }
}