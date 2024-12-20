using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
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
        internal float range = 5;
        [SerializeField, Tooltip("The speed at which the magnet attracts pickups.")]
        internal float speed = 7;
        [SerializeField, Tooltip("How often the magnet scans for pickups.")]
        private float frequencyOfScans = 0.5f;

        List<Transform> targets = new List<Transform>();
        float timeOfNextScan = 0;

          private void Update()
        {
            if (targets.Count > 0)
            {
                targets.RemoveAll(x => x == null);
                foreach (var target in targets)
                {
                    Vector3 direction = transform.position - target.position;
                    float distance = direction.magnitude;
                    if (distance > 0.1 && distance <= range)
                    {
                        float adjustedSpeed = Mathf.Max(1f, speed * (1 + (range - distance) / range));
                        target.Translate(direction * (adjustedSpeed * Time.deltaTime / distance));
                    }
                }
            }

            if (Time.time > timeOfNextScan)
            {
                var pickups = Physics.OverlapSphere(transform.position, range, LayerMask.GetMask("TriggerZones"));
                foreach (var pickup in pickups)
                {
                    if (pickup.CompareTag("MagneticPickup") == false)
                    {
                        continue;
                    }

                    if (!targets.Contains(pickup.transform))
                    {
                        targets.Add(pickup.transform);
                    }
                }

                timeOfNextScan = Time.time + frequencyOfScans;
            }
        }
    }
}