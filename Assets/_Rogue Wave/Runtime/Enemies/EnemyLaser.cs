using NaughtyAttributes;
using NeoFPS;
using System;
using UnityEngine;
using WizardsCode.RogueWave;
using Random = UnityEngine.Random;

namespace RogueWave
{
    [Obsolete("Use WeaponEffectController instead")]
    public class EnemyLaser : MonoBehaviour, IDamageSource
    {
        [Header("Weapon")]
        [SerializeField, Tooltip("The range of the weapon. If not within this range then it will not fire.")]
        float _Range = 50f;
        [SerializeField, Tooltip("How frequently this weapon can fire in seconds.")]
        private float _fireRate = 1f;
        [SerializeField, Tooltip("How quickly the weapon will move to the target position.")]
        private float _targetingSpeed = 5f;
        [SerializeField, Tooltip("The amount of damage this weapon will do to the player per second.")]
        private float damageAmount = 5f;
        [SerializeField, Tooltip("The accuracy of the weapon. This is a random offset applied to the target position. The actual offset will be +/- this amount on each axis.")]
        private float accuracy = 0.1f; 
        [SerializeField, Tooltip("A description of the damage to use in logs, etc.")]
        private string damageDescription = "Laser";

        [Header("Visuals")]
        [SerializeField, Tooltip("The effects to use when the weapon is seeking a target.")]
        private ScriptableEffect _WeaponTargetingFX;
        [SerializeField, Tooltip("The effects to use when the weapon is firing.")]
        private ScriptableEffect _WeaponFiringFX;
        [SerializeField, Tooltip("The amount of time in seconds that the weapon will be visible when firing.")]
        private float _fireDuration = 0.75f; 
        
        private DamageFilter _outDamageFilter = DamageFilter.AllDamageAllTeams;

        private float _timeToNextFiring = 0f;

        BasicEnemyController controller;

        private Vector3 accuracyOffset;
        private float timeToNextRetargeting;
        private float _remainingShotTime;
        private Vector3 _targetedPos;

        public enum State
        {
            Idle,
            LockingOn,
            LockedOn,
            Firing
        }

        private State _state = State.Idle;
        public State state
        {
            get => _state;
            set
            {
                if (_state == value)
                {
                    return;
                }

                _state = value;
                switch (value)
                {
                    case State.Idle:
                        HideLaser();
                        break;
                    case State.LockingOn:
                        if (_WeaponTargetingFX != null)
                        {
                            _WeaponTargetingFX.Spawn(transform.position, Quaternion.identity, controller);
                        }
                        _remainingShotTime = _fireDuration;
                        break;
                    case State.LockedOn:
                        if (_WeaponTargetingFX != null)
                        {
                            _WeaponTargetingFX.Stop();
                        }
                        Debug.LogWarning("TODO: Locked on FX");
                        break;
                    case State.Firing:
                        HideLaser();
                        if (_WeaponFiringFX != null)
                        {
                            _WeaponFiringFX.Spawn(transform.position, Quaternion.identity, controller);
                        }
                        _remainingShotTime = _fireDuration;
                        break;
                }
            }
        }

        /// <summary>
        /// Test if the current targeted position will result in a hit on the player.
        /// </summary>
        private IDamageHandler validTarget
        {
            get
            {
                Vector3 direction = (controller.Target.position - transform.position).normalized;
                if (Physics.Raycast(transform.position, direction, out RaycastHit _hit))
                {
                    if (_hit.collider.CompareTag("Player"))
                    {
                        return _hit.transform.GetComponent<IDamageHandler>();
                    }
                }

                return null;
            }
        }

        public DamageFilter outDamageFilter
        {
            get { return _outDamageFilter; }
            set { _outDamageFilter = value; }
        }

        IController IDamageSource.controller => null;

        public Transform damageSourceTransform => transform;

        public string description
        {
            get { return damageDescription; }
        }

        private void Start()
        {
            controller = GetComponentInParent<BasicEnemyController>();
            HideLaser();
        }

        private void LateUpdate()
        {
            if (controller.CanSeeTarget == false) {
                state = State.Idle;
                return;
            }

            float distance = Vector3.Distance(transform.position, controller.Target.position);
            float targetingRange = _Range * 1.5f;
            switch (state)
            {
                case State.Idle:
                    _timeToNextFiring -= Time.deltaTime;

                    if (distance <= targetingRange && _timeToNextFiring < 0)
                    {
                        state = State.LockingOn;
                    }
                    return;
                case State.LockingOn:
                    if (distance > targetingRange)
                    {
                        state = State.Idle;
                        return;
                    }

                    if (validTarget != null)
                    {
                        state = State.LockedOn;
                    }
                    
                    UpdateLaser();
                    break;
                case State.LockedOn:
                    if (distance > _Range) // can only fire if the player is still in range
                    {
                        state = State.Idle;
                        return;
                    }
                    
                    UpdateLaser();

                    if (controller.shouldAttack)
                    {
                        state = State.Firing;
                    } else
                    {
                        state = State.Idle;
                    }
                    break;
                case State.Firing:
                    _remainingShotTime -= Time.deltaTime;

                    UpdateLaser();

                    IDamageHandler damageHandler = validTarget;
                    if (damageHandler != null)
                    {
                        damageHandler.AddDamage(damageAmount * Time.deltaTime); ;
                    }

                    if (_remainingShotTime < 0)
                    {
                        _timeToNextFiring = _fireRate;
                        state = State.Idle;
                    }
                    break;
            }
        }

        void HideLaser()
        {
            Debug.LogWarning("TODO: Hide Laser");

        }

        void UpdateLaser()
        {
            if (controller.requireLineOfSight && !controller.CanSeeTarget)
            {
                HideLaser();
                return;
            }

            _timeToNextFiring -= Time.deltaTime;
            if (_timeToNextFiring > 0)
            {
                HideLaser();
                return;
            }
            
            if (timeToNextRetargeting < 0)
            {
                accuracyOffset = Vector3.zero;

                accuracyOffset.x += Random.Range(-accuracy, accuracy);
                accuracyOffset.z += Random.Range(-accuracy, accuracy);
                timeToNextRetargeting = _fireRate;
            } else
            {
                timeToNextRetargeting -= Time.deltaTime;
            }

            _targetedPos = controller.Target.position;
            _targetedPos += accuracyOffset;

            Debug.LogWarning("TODO: move laser to targeted position");
            //_lineRenderer.SetPosition(0, transform.position);
            //_lineRenderer.SetPosition(1, Vector3.Slerp(_lineRenderer.GetPosition(1), _targetedPos, _targetingSpeed * Time.deltaTime));
            //_lineRenderer.enabled = true;
        }
    }
}