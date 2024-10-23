using NaughtyAttributes;
using NeoFPS;
using RogueWave;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// The WeaponController is responsible for managing the state of a weapon as it seeks to destroy a target. 
    /// </summary>
    public class BasicWeaponController : MonoBehaviour, IDamageSource
    {
        enum AmmunitionType
        {
            HitScan,
            //SinglePoint,
            //Line,
            //Area
        }

        enum WeaponState
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

        // Meta Data
        [SerializeField, Tooltip("The enemy controller that is using this weapon."), Required, BoxGroup("Meta Data")]
        BasicEnemyController _enemyController;
        [SerializeField, Tooltip("The type of ammunition defines how the weapone FX are displayed and how damage is done."), BoxGroup("Meta Data")]
        AmmunitionType ammunitionType = AmmunitionType.HitScan;
        [SerializeField, Tooltip("The range of the weapon. If not within this range then it will not fire."), BoxGroup("Targeting")]
        float _Range = 50f;

        // Behaviours
        [SerializeField, Tooltip("The behaviour that is used to control the weapons idle effects."), BoxGroup("Behaviours")]
        BasicWeaponBehaviour _idleBehaviour;
        [SerializeField, Tooltip("The behaviour that is used to control the weapons locking on effects."), BoxGroup("Behaviours")]
        TargetingBehaviour _targetingBehaviour;
        [SerializeField, Tooltip("The behaviour that is used to control the weapons firing effects."), BoxGroup("Behaviours")]
        BasicWeaponBehaviour _firingBehaviour;
        [SerializeField, Tooltip("The behaviour that is used to control the weapons reload effects."), BoxGroup("Behaviours")]
        BasicWeaponBehaviour _reloadBehaviour;

        // Damage
        [SerializeField, Tooltip("A description of the damage to use in logs, etc."), BoxGroup("Damage")]
        private string _damageDescription = "Laser";
        [SerializeField, Tooltip("The teams this weapon will damage."), BoxGroup("Damage")]
        private DamageFilter _outDamageFilter = DamageFilter.AllDamageAllTeams;
        [SerializeField, Tooltip("The layers this weapon will be blocked by, and potentially damage."), BoxGroup("Damage")]
        private LayerMask _layerMask;

        private float _sqrRange;
        private RaycastHit _targetingHit;

        internal BasicEnemyController enemyController => _enemyController;
        internal BasicWeaponBehaviour weaponFiringBehaviour => _firingBehaviour;

        public DamageFilter outDamageFilter { get => _outDamageFilter; set => _outDamageFilter = value; }

        public IController controller { 
            get { return null; }
        }

        public float range => _Range;

        private Vector3 _targetedPosition;
        public Vector3 targetPosition => _targetedPosition;

        public LayerMask layerMask => _layerMask;

        public Transform damageSourceTransform => transform;

        public string description => _damageDescription;

        private WeaponState _currentState = WeaponState.Initializing;
        private WeaponState State
        {
            get => _currentState;
            set
            {
                if (_currentState == value)
                {
                    return;
                }

                switch (_currentState)
                {
                    case WeaponState.Initializing:
                        break;
                    case WeaponState.Idle:
                        _idleBehaviour?.StopBehaviour();
                        break;
                    case WeaponState.Targeting:
                        _targetingBehaviour?.StopBehaviour();
                        break;
                    case WeaponState.Firing:
                        _firingBehaviour?.StopBehaviour();
                        break;
                    case WeaponState.Cooldown:
                        _reloadBehaviour?.StopBehaviour();
                        break;
                    default:
                        Debug.LogError($"Unsupported state: {_currentState}");
                        break;
                }

                _currentState = value;
                //Debug.Log("Changing WeaponController state to " + value);
                switch (value)
                {
                    case WeaponState.Initializing:
                        break;
                    case WeaponState.Idle:
                        _idleBehaviour?.StartBehaviour(null);

                        break;
                    case WeaponState.Targeting:
                        _targetingBehaviour?.StartBehaviour(_enemyController.Target);
                        
                        break;
                    case WeaponState.Firing:
                        _firingBehaviour?.StartBehaviour(_enemyController.Target);

                        break;
                    case WeaponState.Cooldown:
                        _reloadBehaviour?.StartBehaviour(null);

                        break;
                    default:
                        Debug.LogError($"Unsupported state: {_currentState}");
                        break;
                }
            }
        }

        internal void OnTargetAcquired(RaycastHit targetHit)
        {
            _targetingHit = targetHit;
            State = WeaponState.Firing;
        }

        private void Awake()
        {
            State = WeaponState.Idle;
        }

        private void OnEnable()
        {
            _idleBehaviour?.onBehaviourStopped.AddListener(() => State = WeaponState.Targeting);

            _targetingBehaviour?.onTargetAcquired.AddListener(OnTargetAcquired);
            _targetingBehaviour?.onTargetingAborted.AddListener(() => State = WeaponState.Idle);

            _firingBehaviour?.onBehaviourStopped.AddListener(() => State = WeaponState.Cooldown);

            _reloadBehaviour?.onBehaviourStopped.AddListener(() => State = WeaponState.Idle);

            State = WeaponState.Idle;
        }

        private void OnDisable()
        {
            _idleBehaviour?.onBehaviourStopped.RemoveListener(() => State = WeaponState.Targeting);

            _targetingBehaviour?.onTargetAcquired.RemoveListener(OnTargetAcquired);
            _targetingBehaviour?.onTargetingAborted.RemoveListener(() => State = WeaponState.Idle);

            _firingBehaviour?.onBehaviourStopped.RemoveListener(() => State = WeaponState.Cooldown);

            _reloadBehaviour?.onBehaviourStopped.RemoveListener(() => State = WeaponState.Idle);
        }

        private void Start()
        {
            _enemyController = GetComponentInParent<BasicEnemyController>();

            _sqrRange = _Range * _Range;
        }

        private void LateUpdate()
        {
            ValidateWeaponState();
        }

        /// <summary>
        /// Checks to see that the current state is still valid. If not it will change the state to Idle.
        /// </summary>
        private void ValidateWeaponState()
        {
            if (_enemyController.Target == null) return;

            float sqrDistance = (_enemyController.Target.position - transform.position).sqrMagnitude;
            if (sqrDistance > _sqrRange)
            {
                State = WeaponState.Idle;
                return;
            }

            if (_enemyController.requireLineOfSight && _enemyController.CanSeeTarget == false)
            {
                State = WeaponState.Idle;
                return;
            }
        }
    }
}
