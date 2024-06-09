using log4net.Util;
using NaughtyAttributes;
using NeoFPS;
using NeoFPS.SinglePlayer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace RogueWave
{
    public class Fireball : PassiveWeapon

    {
        [SerializeField, Tooltip("The speed of the fireball.")]
        private float speed = 10f;

        private GameObject visuals;
        private EnemyDamageTriggerZone triggerZone;
        Collider collider;

        private NanobotPawnController m_synthos;
        private NanobotPawnController synthos
        {
            get
            {
                if (m_synthos == null)
                {
                    m_synthos = GetComponentInParent<NanobotPawnController>();
                }
                return m_synthos;
            }
        }

        protected override void Start()
        {
            base.Start();
            visuals = GetComponentInChildren<ParticleSystem>().gameObject;
            triggerZone = GetComponentInChildren<EnemyDamageTriggerZone>();
            collider = model.GetComponent<Collider>();
        }

        public override void Fire()
        {
            triggerZone.SetDamage(damage);
            triggerZone.SetHitAudioClips(hitAudioClip);
            StartCoroutine(FireballRoutine());
        }

        private IEnumerator FireballRoutine()
        {
            currentState = State.Firing;
            model.transform.SetParent(null);
            collider.enabled = true;

            BasicEnemyController target = synthos.GetNearestEnemy();
            float distance = 0f;
            while (distance < range)
            {
                Vector3 direction;
                if (target != null)
                {
                    direction = target.transform.position - model.transform.position;
                }
                else
                {
                    direction = FpsSoloCharacter.localPlayerCharacter.transform.forward;
                }
                model.transform.position += direction * speed * Time.deltaTime;
                AdjustHeight();
                distance += speed * Time.deltaTime;

                yield return null;
            }

            visuals.SetActive(false);
            collider.enabled = false;
            model.transform.SetParent(transform);
            model.transform.localPosition = positionOffset;

            // Wait until there is only 1 second of the colldown left and re-enabel the projectile at source position.
            if (m_NextFireTime > Time.timeSinceLevelLoad - 2)
            {
                yield return new WaitForSeconds(m_NextFireTime - Time.timeSinceLevelLoad - 2);
            }

            visuals.SetActive(true);
            currentState = State.Ready;
        }

        private void AdjustHeight()
        {
            float obstacleAvoidanceDistance = 0.9f;

            Ray ray = new Ray(model.transform.position, -model.transform.up);
            if (Physics.Raycast(ray, out RaycastHit hit, obstacleAvoidanceDistance))
            {
                model.transform.position += Vector3.up * 10 * Time.deltaTime;
            }
        }
    }
}
