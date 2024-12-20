﻿using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace RogueWave
{
    /// <summary>
    /// The Scanner Controller provides a scanner to enemies that enables them to detect the player up to a given range and precision, even without a line of sight.
    /// This allows them to report approximate location to the AI Director and other enemies.
    /// When the Scanner can see the player it will be able to notify of the exact last known location of the player.
    /// </summary>
    public class ScannerController : MonoBehaviour
    {
        [SerializeField, Tooltip("If the scanner can't see the player this is the accuracy of the scan. 0 is perfect accuracy, anything above 0 is a random radius offset from the players position.")]
        float m_ScanPrecision = 50f;
        [SerializeField, Tooltip("How often the scanner will scan for the player. 0 is every frame, anything above 0 is the number of seconds between scan pulses.")]
        float m_ScanFrequency = 10f;

        float m_TimeOfNextScan = 0f;
        private BasicEnemyController m_controller;
        private AIDirector m_director;
        
        private void Awake()
        {
            m_controller = GetComponentInParent<BasicEnemyController>();
            m_director = AIDirector.Instance;
            if (m_director == null)
            {
                Debug.LogWarning("ScannerController: No AIDirector found in scene. Scanner functionality removed.");
                Destroy(this);
            }
        }

        protected void LateUpdate()
        {
            if (m_controller.Target == null || m_TimeOfNextScan > Time.timeSinceLevelLoad)
            {
                return;
            }

            if (m_controller.SquadCanSeeTarget)
            {
                m_director.ReportPlayerLocation(m_controller.Target.position);
            }
            else
            {
                m_TimeOfNextScan = Time.timeSinceLevelLoad + m_ScanFrequency;

                m_controller.goalDestination = m_controller.GetDestination(m_controller.Target.position) + (UnityEngine.Random.insideUnitSphere * m_ScanPrecision);
                m_director.ReportPlayerLocation(m_controller.goalDestination);
            }
        }
    }
}
