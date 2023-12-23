using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeoFPS.SinglePlayer;
using NeoFPS;
using System;

namespace Playground
{
    [RequireComponent (typeof (CanvasGroup))]
	public class HudVictoryPopup : MonoBehaviour
    {
        private CanvasGroup m_CanvasGroup = null;

        void Awake()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
            PlaygroundDecember23GameMode.onVictory += OnVictory;
            m_CanvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private void OnVictory()
        {
            m_CanvasGroup.alpha = 1f;
            gameObject.SetActive(true);
        }
    }
}