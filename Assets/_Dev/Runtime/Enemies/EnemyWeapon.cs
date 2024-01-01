using NeoFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
    public class EnemyWeapon : MonoBehaviour
    {
        [Header("Weapon")]
        [SerializeField, Tooltip("The range of the weapon. If not within this range then it will not fire.")]
        float _Range = 50f;
        [SerializeField, Tooltip("The amount of time in seconds that the weapon will lock on to the player before firing.")]
        private float _LockOnTime = 1.5f;
        [SerializeField, Tooltip("How frequently this weapon can fire in seconds.")]
        private float _fireRate = 1f;
        [SerializeField, Tooltip("The amount of damage this weapon will do to the player.")]
        private float damageAmount = 5f;

        [Header("Juice")]
        [SerializeField, Tooltip("The line renderer used to show the weapon firing.")]
        LineRenderer _lineRenderer;
        [SerializeField, Tooltip("The amount of time in seconds that the weapon will be visible when firing.")]
        private float _fireDuration = 0.75f;

        enum State
        {
            Idle,
            LockingOn,
            Firing
        }

        private State _state = State.Idle;
        private float _lockOnTimer = 0f;
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
                _state = State.Idle;
                return;
            }

            switch (_state)
            {
                case State.Idle:
                    _state = State.LockingOn;
                    _lockOnTimer = _LockOnTime;
                    return;
                case State.LockingOn:
                    if (Vector3.Distance(transform.position, controller.Target.position) <= _Range)
                    {
                        _lockOnTimer -= Time.deltaTime;
                        if (_lockOnTimer <= 0f)
                        {
                            _state = State.Firing;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        _state = State.Idle;
                        return;
                    }
                    break;
                case State.Firing:
                    if (Vector3.Distance(transform.position, controller.Target.position) > _Range)
                    {
                        _state = State.Idle;
                        return;
                    }
                    else
                    {
                        _lineRenderer.SetPosition(0, transform.position);

                        _fireTimer += Time.deltaTime;

                        if (_fireTimer >= _fireRate && controller.shouldAttack)
                        {
                            _fireTimer = 0f;
                            StartCoroutine(Fire());
                        }
                    }
                    break;
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