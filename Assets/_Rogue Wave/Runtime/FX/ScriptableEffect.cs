using NaughtyAttributes;
using NeoFPS;
using RogueWave;
using System;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// Scripable Effects are effects are Scriptable Objects that define a set of VFX and SFX.
    /// Instances of this class can be assigned to controller components, such as the WeaponJuice component.
    /// </summary>
    //[CreateAssetMenu(fileName = "New Scripted Effect", menuName = "Rogue Wave/FX/Scripted Effect")]
    [Obsolete("User MMFeel instead.")]
    public class ScriptableEffect : ScriptableObject
    {
        enum StopAction
        {
            None,
            Deactivate,
            Destroy
        }

        enum EffectType {
            SinglePoint,
            Line,
            Area
        }

        //[Header("Meta Data")]
        [SerializeField, Tooltip("The type of effect. This governs how the effect is managed at spawn. What you select here will adjust the configuration options available below."), BoxGroup("Meta Data")]
        EffectType _effectType = EffectType.SinglePoint;
        [SerializeField, Tooltip("The action to take when the effect is stopped."), BoxGroup("Meta Data")]
        StopAction _stopAction = StopAction.Deactivate;

        //[Header("Effects")]
        [SerializeField, Tooltip("The object that will be spawned when this effect is initially started."), BoxGroup("Effects Prefabs")]
        FXJuicer _effectObject;
        [SerializeField, Tooltip("The object that will be spawned when this effect hits another object."), BoxGroup("Effects Prefabs")]
        PooledObject _hitObject;

        [InfoBox("The following properties are more commonly set at runtime to configure the effect, see tooltips for more information. " +
            "They are exposed here in case you need them at design time.")]
        //[Header("Positioning")]
        [SerializeField, Tooltip("The start position of the effect. This can be configured at runtime when the effect is spawned or by using `StartPosition = pos`."), Foldout("Positioning")]
        Vector3 _startPosition;
        [SerializeField, Tooltip("The end position of the effect. This can be configured at runtime when the effect is spawned or by  using `EndPosition = pos`."), Foldout("Positioning"), ShowIf("_effectType", EffectType.Line)]
        Vector3 _endPosition;

        FXJuicer _activeEffect;

        /// <summary>
        /// Pull an instance of this effect from the pool on behalf of an Enemy controller and play it at the specified position.
        /// Play the effect. This will activate the effect object and begin any management routines needed.
        /// </summary>
        public void Spawn (Vector3 startPosition, Quaternion rotation, BasicEnemyController owner)
        {
#if UNITY_EDITOR
            if (_effectObject == null)
            {
                Debug.LogError("No effect object has been assigned to the ScriptableEffect. This is required to play the effect.");
                return;
            }
#endif
            _startPosition = startPosition;
            _activeEffect = PoolManager.GetPooledObject<FXJuicer>(_effectObject, startPosition, rotation);
            _activeEffect.OwnerController = owner;
        }

        /// <summary>
        /// Pull an instance of this effect from the pool on behalf of an Enemy controller and play it at the specified position.
        /// Play the effect. This will activate the effect object and begin any management routines needed.
        /// </summary>
        public void Spawn(Vector3 startPosition, Vector3 endPosition, Quaternion rotation, BasicEnemyController owner)
        {
#if UNITY_EDITOR
            if (_effectObject == null)
            {
                Debug.LogError("No effect object has been assigned to the ScriptableEffect. This is required to play the effect.");
                return;
            }

            if (_effectType != EffectType.Line)
            {
                Debug.LogError("The effect type is not set to Line, but you are trying to spawn a line effect. This will not work.");
                return;
            }
#endif
            _startPosition = startPosition;
            _endPosition = endPosition;
            _activeEffect = PoolManager.GetPooledObject<FXJuicer>(_effectObject, startPosition, rotation);
            _activeEffect.OwnerController = owner;
        }

        public void Stop()
        {
            //_effectObject.SetActive(false);
        }
    }
}
