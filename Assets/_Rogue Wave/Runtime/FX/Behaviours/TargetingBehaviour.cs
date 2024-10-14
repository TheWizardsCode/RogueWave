using NaughtyAttributes;
using NeoFPS;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// The TargetingBehaviour is used to locate and lock on to a target. 
    /// Broadly speaking it goes through the following states:
    /// Idle: Doing nothing, waiting for an instruction from the weapon to start targeting.
    /// Targeting: The weapon is attempting to acquire a target. It will do this by moving the currently targeted position towards the target's position.
    /// Acquired: The weapon has located a target and is now attempting to lock on to it. This is a process that takes time and can be interrupted if the target moves out of range.
    /// 
    /// Weapon controllers should subscribe to the OnTargetAcquired event to be notified when a target has been acquired. It is the weapon controllers job to then fire the weapon.
    /// Weapon controllers will also want to subscribe to the OnTargetingAborted when the targeting process is aborted
    /// Weapon controllers should also call StartBehaviour to start the targeting process.
    /// </summary>
    /// <see cref="WeaponController"/>"
    public class TargetingBehaviour : LineWeaponBehaviour
    {
        protected enum TargetingOnState
        {
            Idle,
            Targeting,
            LockingOn,
            Acquired
        }

        //[Header("Targeting")]
        [SerializeField, Tooltip("The range of the weapon. If not within this range then it will not fire."), BoxGroup("Targeting")]
        float _Range = 50f;
        [SerializeField, Tooltip("The accuracy of the weapon targeting systems. This is a random offset applied to the target position. The actual offset will be +/- this amount on each axis."), BoxGroup("Targeting")]
        private float accuracy = 1.5f;
        [SerializeField, Tooltip("The frequency, in seconds, that the weapon will adjust it's targeting location."), BoxGroup("Targeting")]
        private float _targetingFrequency = 2f;
        [SerializeField, Tooltip("The maximum time the weapon can attempt to locate a target, in seconds. If this time is reached without locating the targaet then the behaviour enters the idle."), BoxGroup("Targeting")]
        protected float maxTimeToTryToLocateTarget = 5f;
        [SerializeField, Tooltip("How quickly the weapon will get a lock on the target."), BoxGroup("Targeting")]
        private float _targetingSpeed = 10f;
        [SerializeField, Tooltip("A callback that is called when the weapon has aborted the targeting process."), BoxGroup("Targeting")]
        private GameEvent onTargetingAborted;

        //[Header("Acquiring")]
        [SerializeField, Tooltip("The time between strting the lockon process and the end of that process."), BoxGroup("Acquiring")]
        private float _lockOnTime = 1.5f;
        [SerializeField, Tooltip("The event to invoke when the weapon has acquired a target. The RaycaseHit that marks the point of contact can be retried with the parameter `targetHit`."), BoxGroup("Acquiring")]
        private GameEvent onTargetAcquired;

        //[Header("Deactivate")]
        [SerializeField, Tooltip("The time the weapon will remain locked on to the target after the lock on process is complete. This should be long enough to allow the firing process to begin and any targeting/deactivating effects to complete, which is managed by another behaviour."), BoxGroup("Deactivation")]
        private float _deactivateDelay = 0.5f;

        //[Header("Effects")]
        [SerializeField, Tooltip("The LineRenderer material to use when the weapon is targeting."), BoxGroup("Effects")]
        private Material _targetingLineMaterial;
        [SerializeField, Tooltip("The LineRenderer material to use when the weapon is locked on."), BoxGroup("Effects")]
        private Material _lockedOnLineMaterial;
        [SerializeField, Tooltip("The model that will be placed at the current targeting position."), BoxGroup("Effects")]
        private MeshRenderer _targetingMeshRenderer;
        [SerializeField, Tooltip("The targeting model material to use when the weapon is targeting."), BoxGroup("Effects")]
        private Material _targetingMaterial;
        [SerializeField, Tooltip("The LineRenderer material to use when the weapon is locked on."), BoxGroup("Effects")]
        private Material _lockedOnMaterial;

        private TargetingOnState _state = TargetingOnState.Idle;
        private float _startTime;
        private float _timeOfNextTargetingUpdate;
        private Vector3 _accuracyOffset;
        public RaycastHit? targetHit { get; private set; }

        public float range => _Range;
        public float targetingSpeed => _targetingSpeed;
        
        /// <summary>
        /// The position currently being targeted by the weapon. This is not necessarily the same as the target's position as teh weapon may be leading the target or may be inaccurate.
        /// </summary>
        public Vector3 TargetedPosition { get; set; }

        protected TargetingOnState State {
            get { return _state; }
            set
            {
                if (_state == value)
                {
                    return;
                }

                _startTime = Time.time;

                switch (value)
                {
                    case TargetingOnState.Idle:
                        targetHit = null;
                        lineRenderer.gameObject.SetActive(false);
                        onTargetingAborted?.Raise();
                        break;
                    case TargetingOnState.Targeting:
                        lineRenderer.gameObject.SetActive(true);
                        lineRenderer.material = _targetingLineMaterial;
                        _targetingMeshRenderer.material = _targetingMaterial;
                        break;
                    case TargetingOnState.LockingOn:
                        lineRenderer.gameObject.SetActive(true);
                        lineRenderer.material = _lockedOnLineMaterial;
                        _targetingMeshRenderer.material = _lockedOnMaterial;
                        break;
                    case TargetingOnState.Acquired:
                        break;
                    default:
                        Debug.LogError("Unhandled state switch in TargetingBehaviour: " + value);
                        break;
                }

                _state = value;
            }
        }

        /// <summary>
        /// Test if the current targeted position can result in a hit on a valid tarrget.
        /// If there is a valid target then return it, otherwise return null.
        /// </summary>
        /// <returns>Null if no target acquired, otherwise returns the RaycastHit that marks the point of contact.</returns>
        private RaycastHit? AcquiredTarget { 
            get
            {
                // OPTIMIZATION: only do the raycast if the distance between the player and the weapon has changed by more than a tolerance level
                Vector3 direction = TargetedPosition - transform.position;
                float sqrDistance = direction.sqrMagnitude;

                // OPTIMIZATION: cache the range squared calculation
                if (sqrDistance <= range * range)
                {
                    direction.Normalize();
                    if (Physics.Raycast(transform.position, direction, out RaycastHit hit))
                    {
                        if (hit.collider.CompareTag(target.tag))
                        {
                            return hit;
                        }
                    }
                }

                return null;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateEffects(true);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            StopBehaviour();
        }

        /// <summary>
        /// Start attempting to lock on to a target.
        /// </summary>
        /// <param name="target">The target we are trying to lock onto.</param>
        /// <param name="targetAcquiredCallback">A callback to be called when the target is acquired.</param>
        public override void StartBehaviour(Transform target)
        {
            base.StartBehaviour(target);

            State = TargetingOnState.Targeting;

            _startTime = Time.time;
            _timeOfNextTargetingUpdate = Time.time;

            _accuracyOffset = Vector3.zero;
            _accuracyOffset.x += Random.Range(-accuracy, accuracy);
            _accuracyOffset.z += Random.Range(-accuracy, accuracy);

            TargetedPosition = this.target.position + _accuracyOffset;
        }

        protected override void Update()
        {
            if (_state == TargetingOnState.Idle)
            {
                return;
            }

            UpdateTargeting();
            
            switch (_state)
            {
                case TargetingOnState.Idle:
                    break;
                case TargetingOnState.Targeting:
                    targetHit = AcquiredTarget;
                    if (targetHit != null)
                    {
                        State = TargetingOnState.LockingOn;
                    }
                    else if (Time.time - _startTime > maxTimeToTryToLocateTarget)
                    {
                        State = TargetingOnState.Idle;
                    }
                    break;
                case TargetingOnState.LockingOn:
                    targetHit = AcquiredTarget;
                    if (targetHit != null)
                    {
                        if (Time.time - _startTime > _lockOnTime)
                        {
                            State = TargetingOnState.Acquired;
                            onTargetAcquired?.Raise();
                        }
                    } 
                    else
                    {
                        State = TargetingOnState.Idle;
                    }
                    break;  
                case TargetingOnState.Acquired:
                    if (Time.time - _startTime > _deactivateDelay)
                    {
                        State = TargetingOnState.Idle;
                    }
                    break;
                default:
                    Debug.LogError("Unhandled state in TargetingBehaviour: " + _state);
                    break;
            }

            base.Update();
        }

        void UpdateTargeting()
        {
            if (_timeOfNextTargetingUpdate < Time.time)
            {
                _timeOfNextTargetingUpdate += _targetingFrequency;

                _accuracyOffset = Vector3.zero;

                float adjustedAccuracy = accuracy * ((Time.time - _startTime) / maxTimeToTryToLocateTarget);
                _accuracyOffset.x += Random.Range(-adjustedAccuracy, adjustedAccuracy);
                _accuracyOffset.z += Random.Range(-adjustedAccuracy, adjustedAccuracy);
            }

            TargetedPosition = Vector3.Slerp(TargetedPosition, target.position + _accuracyOffset, targetingSpeed * Time.deltaTime);
            //Debug.DrawRay(transform.position, _targetedPosition - transform.position, Color.red);
        }


        /// <summary>
        /// Update the effects to match the current state. Unless you set `force = true`
        /// the update may not happen, depending on optimization settings.
        /// </summary>
        /// <param name="force"></param>
        protected override void UpdateEffects(bool force)
        {
            // OPTIMIZATION: Do we want to update the effects every frame?
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, TargetedPosition);
            }

            _targetingMeshRenderer.transform.position = TargetedPosition;
        }
    }
}
