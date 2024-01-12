using NeoFPS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Playground
{
    public class PlayerAudioManager : MonoBehaviour
    {
        [Header("Shield Audio")]
        [SerializeField, Tooltip("The audio to play when the shield is charged up.")]
        AudioClip[] chargeUp;
        [SerializeField, Tooltip("The audio to play when the shield looses some charge.")]
        AudioClip[] chargeDown;
        
        private IShieldManager shieldManager;
        private float earliestTimeOfLastShieldDown = 0f;
        private float earliestTimeOfNextShieldUp = 0f;

        private void Awake()
        {
            shieldManager = GetComponent<IShieldManager>();
        }

        private void OnEnable()
        {
            shieldManager.onShieldValueChanged += OnShieldValueChanged;
        }

        private void OnDisable()
        {
            shieldManager.onShieldValueChanged -= OnShieldValueChanged;
        }

        private void OnShieldValueChanged(IShieldManager shield, float from, float to)
        {
            if (from > to && earliestTimeOfLastShieldDown < Time.time)
            {
                if (chargeDown != null)
                {
                    AudioClip clip = chargeDown[Random.Range(0, chargeDown.Length)];
                    NeoFpsAudioManager.PlayEffectAudioAtPosition(clip, transform.position);
                    earliestTimeOfLastShieldDown = Time.timeSinceLevelLoad + clip.length + 0.2f;
                }
            }
            else if (from < to && earliestTimeOfNextShieldUp < Time.time)
            {
                if (chargeUp != null)
                {
                    AudioClip clip = chargeUp[Random.Range(0, chargeUp.Length)];
                    NeoFpsAudioManager.PlayEffectAudioAtPosition(clip, transform.position);
                    earliestTimeOfNextShieldUp = Time.timeSinceLevelLoad + clip.length + 0.2f;
                }
            }
        }
    }
}