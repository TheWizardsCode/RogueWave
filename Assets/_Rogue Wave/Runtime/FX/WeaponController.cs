using NaughtyAttributes;
using NeoFPS;
using RogueWave;
using System;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// The WeaponController is responsible for managing the state of a weapon as it seeks to destroy a target. 
    /// </summary>
    public class WeaponController : MonoBehaviour, IDamageSource
    {
        enum StopAction
        {
            None,
            //Deactivate,
            //Destroy
        }

        enum AmmunitionType
        {
            HitScan,
            //SinglePoint,
            //Line,
            //Area
        }

        public enum FiringState
        {
            Initializing,
            Idle,
            Targeting,
            Firing,
            Cooldown
        }

        enum FxProviders
        {
            Feel,
            KryptoFxRealisticEffectsPack3
        }

        //[Header("Meta Data")]
        [SerializeField, Tooltip("The type of ammunition defines how the weapone FX are displayed and how damage is done."), BoxGroup("Meta Data")]
        AmmunitionType ammunitionType = AmmunitionType.HitScan;
        [SerializeField, Tooltip("The action to take when the effect is stopped."), BoxGroup("Meta Data")]
        StopAction _stopAction = StopAction.None; [SerializeField, Tooltip("The range of the weapon. If not within this range then it will not fire."), BoxGroup("Targeting")]
        float _Range = 50f;

        //[Header("Behaviours")]
        [SerializeField, Tooltip("The MM Feel Player that is used to control the weapons idle effects."), BoxGroup("Behaviours")]
        WeaponBehaviour _idleBehaviour;
        [SerializeField, Tooltip("The MM Feel Player that is used to control the weapons locking on effects."), BoxGroup("Behaviours")]
        TargetingBehaviour _targetingBehaviour;
        [SerializeField, Tooltip("The MM Feel Player that is used to control the weapons firing effects."), BoxGroup("Behaviours")]
        WeaponBehaviour _firingBehaviour;
        [SerializeField, Tooltip("The MM Feel Player that is used to control the weapons cooldown effects."), BoxGroup("Behaviours")]
        WeaponBehaviour _cooldownBehaviour;

        //[Header("Weapon Targeting")]
        [SerializeField, Tooltip("The event to listent to for when the weapon aborts targeting."), BoxGroup("Weapon Targeting")]
        private GameEvent onTargetingAborted;
        [SerializeField, Tooltip("The event to listen to for when the weapon is ready to fire."), BoxGroup("Weapon Targeting")]
        private GameEvent onTargetAcquired;

        //[Header("Weapon Firing")]
        [SerializeField, Tooltip("The speed of the projectile."), BoxGroup("Weapon Firing"), HideIf("_ammunitionType", AmmunitionType.HitScan)]
        private float _projectileSpeed = 10f;
        [SerializeField, Tooltip("How long the weapon must spend in an idle state between attempted lockons or firings."), BoxGroup("Weapon Firing")]
        private float _cooldown = 1f;
        [SerializeField, Tooltip("The event to listen to for when the weapon inflicts damage. This is used to trigger the damage on the target."), BoxGroup("Weapon Firing")]
        private GameEvent onDamageInflicted;

        //[Header("Damage")]
        [SerializeField, Tooltip("The amount of damage this weapon will do to the player per second."), BoxGroup("Damage")]
        private float damageAmount = 5f;
        [SerializeField, Tooltip("A description of the damage to use in logs, etc."), BoxGroup("Damage")]
        private string _damageDescription = "Laser";
        [SerializeField, Tooltip("The teams this weapon will damage."), BoxGroup("Damage")]
        private DamageFilter _outDamageFilter = DamageFilter.AllDamageAllTeams;
        [SerializeField, Tooltip("The layers this weapon will be blocked by, and potentially damage."), BoxGroup("Damage")]
        private LayerMask _layerMask;

        private float _sqrRange;
        private RaycastHit _targetingHit;

        public DamageFilter outDamageFilter { get => _outDamageFilter; set => _outDamageFilter = value; }

        BasicEnemyController _enemyController;
        public IController controller { 
            get { return null; }
        }

        private float _earliestTimeOfNextStateChange;

        public float range => _Range;

        private Vector3 _targetedPosition;
        public Vector3 targetPosition => _targetedPosition;

        public float projectileSpeed => _projectileSpeed;
        public LayerMask layerMask => _layerMask;

        public Transform damageSourceTransform => transform;

        public string description => _damageDescription;

        private FiringState _state = FiringState.Initializing;
        public FiringState State
        {
            get => _state;
            set
            {
                if (_state == value)
                {
                    return;
                }

                switch (_state)
                {
                    case FiringState.Initializing:
                        break;
                    case FiringState.Idle:
                        _idleBehaviour?.StopBehaviour();
break;
                    case FiringState.Targeting:
                        _targetingBehaviour?.StopBehaviour();
                        break;
                    case FiringState.Firing:
                        _firingBehaviour?.StopBehaviour();
                        break;
                    case FiringState.Cooldown:
                        _cooldownBehaviour?.StopBehaviour();
                        break;
                    default:
                        Debug.LogError($"Unsupported state: {_state}");
                        break;
                }

                _state = value;
                //Debug.Log("Changing WeaponController state to " + value);
                switch (value)
                {
                    case FiringState.Initializing:
                        break;
                    case FiringState.Idle:
                        _earliestTimeOfNextStateChange = Time.time + _cooldown;

                        _idleBehaviour?.StartBehaviour(null);

                        break;
                    case FiringState.Targeting:
                        _targetingBehaviour?.StartBehaviour(_enemyController.Target);
                        
                        break;
                    case FiringState.Firing:
                        _earliestTimeOfNextStateChange = Time.time;
                        
                        _firingBehaviour?.StartBehaviour(_enemyController.Target);

                        break;
                    case FiringState.Cooldown:
                        _earliestTimeOfNextStateChange = Time.time + _cooldown;

                        _cooldownBehaviour?.StartBehaviour(null);

                        break;
                    default:
                        Debug.LogError($"Unsupported state: {_state}");
                        break;
                }
            }
        }

        private void OnEnable()
        {
            if (onTargetAcquired != null)
            {
                onTargetAcquired.RegisterListener(OnTargetAcquired);
            }
            if (onTargetingAborted != null)
            {
                onTargetingAborted.RegisterListener(OnTargetingAborted);
            }
            if (onDamageInflicted != null)
            {
                onDamageInflicted.RegisterListener(OnDamageInflicted);
            }
        }

        private void OnDisable()
        {
            if (onTargetAcquired != null)
            {
                onTargetAcquired.UnregisterListener(OnTargetAcquired);
            }
            if (onTargetingAborted != null)
            {
                onTargetingAborted.UnregisterListener(OnTargetingAborted);
            }
            if (onDamageInflicted != null)
            {
                onDamageInflicted.UnregisterListener(OnDamageInflicted);
            }
        }

        public void OnTargetAcquired()
        {
            State = FiringState.Firing;
            _targetingHit = (RaycastHit)_targetingBehaviour.targetHit; // we know it is not null because we are being notified of the target acquired event
        }

        private void OnTargetingAborted()
        {
            State = FiringState.Idle;
        }

        private void OnDamageInflicted()
        {
            IDamageHandler damageHandler = _targetingHit.collider.GetComponent<IDamageHandler>();
            if (damageHandler != null)
            {
                damageHandler.AddDamage(damageAmount); ;
            }

            State = FiringState.Cooldown;
        }

        private void Awake()
        {
            State = FiringState.Idle;
        }

        private void Start()
        {
            _enemyController = GetComponentInParent<BasicEnemyController>();
            _sqrRange = _Range * _Range;
        }

        private void LateUpdate()
        {
            switch (ammunitionType)
            {
                case AmmunitionType.HitScan:
                    UpdateWeaponState();
                    break;
                default:
                    Debug.LogError($"Unsupported ammunition type: {ammunitionType}");
                    break;
            }
        }

        /// <summary>
        /// Handles the update code for a projectile weapon.
        /// </summary>
        private void UpdateWeaponState()
        {
            if (_enemyController.Target == null) return;

            float sqrDistance = (_enemyController.Target.position - transform.position).sqrMagnitude;
            if (sqrDistance > _sqrRange)
            {
                State = FiringState.Idle;
                return;
            }

            if (_enemyController.requireLineOfSight && _enemyController.CanSeeTarget == false)
            {
                State = FiringState.Idle;
                return;
            }

            float targetingRange = _Range * 1.5f;
            switch (State)
            {
                case FiringState.Idle:
                    if (_earliestTimeOfNextStateChange <= Time.time && sqrDistance < _sqrRange)
                    {
                        State = FiringState.Targeting;
                    }

                    return;
                case FiringState.Targeting:
                    break;
                case FiringState.Firing:
                    break;
                case FiringState.Cooldown:
                    if (_earliestTimeOfNextStateChange <= Time.time)
                    {
                        State = FiringState.Idle;
                    }
                    break;
                default:
                    Debug.LogError($"Unsupported state: {State}");
                    break;
            }
        }
    }
}
