using UnityEngine;
using UnityEngine.Events;

namespace RogueWave
{
    /// <summary>
    /// Portal Controller handles all elements of the portal. If the player enters the portal, the level is completed.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PortalController : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField, Tooltip("The event to trigger when something enters the portal.")]
        public PortalEvent onPortalEntered;

        [System.Serializable]
        public class PortalEvent : UnityEvent<PortalController, Collider> { }


        RogueWaveGameMode gameMode;

        private void Start()
        {
            gameMode = FindObjectOfType<RogueWaveGameMode>();
            gameMode.RegisterPortal(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            onPortalEntered?.Invoke(this, other);
        }
    }
}