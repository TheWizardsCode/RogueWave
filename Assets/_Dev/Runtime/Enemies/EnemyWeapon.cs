using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
    public class EnemyWeapon : MonoBehaviour
    {
        [Header("Weapon")]
        [SerializeField, Tooltip("How frequently this weapon can fire in seconds.")]
        private float _fireRate = 1f;
        [SerializeField, Tooltip("The amount of damage this weapon will do to the player.")]
        private float damageAmount = 5f;

        [Header("Juice")]
        [SerializeField, Tooltip("The line renderer used to show the weapon firing.")]
        LineRenderer _lineRenderer;
        [SerializeField, Tooltip("The amount of time in seconds that the weapon will be visible when firing.")]
        private float _fireDuration = 0.75f;


        private float _fireTimer = 0f;

        BasicEnemyController controller;

        private IDamageHandler _playerDamageHandler;
        IDamageHandler PlayerDamageHandler
        {
            get
            {
                if (controller.Target == null)
                {
                    return null;
                }

                if (_playerDamageHandler == null)
                {
                    _playerDamageHandler = controller.Target.GetComponentInChildren<IDamageHandler>();
                }

                return _playerDamageHandler;
            }
        }

        private void Start()
        {
            controller = GetComponentInParent<BasicEnemyController>();
        }

        private void Update()
        {
            if (controller.CanSeeTarget == false || PlayerDamageHandler == null) {
                return;
            }

            _lineRenderer.SetPosition(0, transform.position);

            _fireTimer += Time.deltaTime;

            if (_fireTimer >= _fireRate && controller.shouldAttack)
            {
                _fireTimer = 0f;
                StartCoroutine(Fire());
            }
        }

        private IEnumerator Fire()
        {
            Vector3 pos = controller.Target.position;
            pos.y += 0.8f; // TODO should use player aim targets
            _lineRenderer.SetPosition(1, pos);
            _lineRenderer.enabled = true;

            PlayerDamageHandler.AddDamage(damageAmount);

            yield return new WaitForSeconds(_fireDuration);

            _lineRenderer.enabled = false;
        }
    }
}