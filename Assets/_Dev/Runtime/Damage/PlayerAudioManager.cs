using NeoFPS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RogueWave
{
    public class PlayerAudioManager : MonoBehaviour
    {
        [Header("Shield Audio")]
        [SerializeField, Tooltip("The audio to play when the shield is charged up.")]
        AudioClip[] chargeUp;
        [SerializeField, Tooltip("The audio to play when the shield looses some charge.")]
        AudioClip[] chargeDown;
        
        private IShieldManager shieldManager;
        private float earliestTimeOfNextShieldSound = 0;

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
            if (earliestTimeOfNextShieldSound > Time.timeSinceLevelLoad)
            {
                return;
            }

            if (from > to)
            {
                if (chargeDown != null)
                {
                    AudioClip clip = chargeDown[Random.Range(0, chargeDown.Length)];
                    NeoFpsAudioManager.PlayEffectAudioAtPosition(clip, transform.position);
                    earliestTimeOfNextShieldSound = Time.timeSinceLevelLoad + clip.length + 0.2f;
                }
            }
            else if (from < to)
            {
                if (chargeUp != null)
                {
                    AudioClip clip = chargeUp[Random.Range(0, chargeUp.Length)];
                    NeoFpsAudioManager.PlayEffectAudioAtPosition(clip, transform.position);
                    earliestTimeOfNextShieldSound = Time.timeSinceLevelLoad + clip.length + 0.2f;
                }
            }
        }
    }
}