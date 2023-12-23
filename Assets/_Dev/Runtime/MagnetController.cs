using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
    /// <summary>
    /// The magenet is available to the player on startup. It attracts pockups to the player.
    /// This means the player does not need to go and collect them, though that will be quicker in some situations.
    /// 
    /// The magnet can be upgraded to increase the range and speed at which it attracts pickups.
    /// </summary>
    public class MagnetController : MonoBehaviour
    {
        [SerializeField, Tooltip("The range of the magnet.")]
        private float range = 5;
        [SerializeField, Tooltip("The speed at which the magnet attracts pickups.")]
        private float speed = 2;

        private void Update()
        {
            var pickups = Physics.OverlapSphere(transform.position, range, LayerMask.GetMask("TriggerZones"));
            foreach (var pickup in pickups)
            {
                if (pickup.CompareTag("MagneticPickup") == false)
                {
                    continue;
                }

                Vector3 direction = transform.position - pickup.transform.position;
                pickup.transform.Translate(direction.normalized * speed * Time.deltaTime);
            }
        }
    }
}