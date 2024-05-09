using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    public class RadarController : NanobotPawnUpgrade
    {
        [SerializeField, Tooltip("The radiues the radar will scan.")]
        float radarRadius = 10f;
        [SerializeField, Tooltip("The number of items the radar will detect. This is not the number of items the radar will show, but rather then number of things internal systems will detect. See `radarBlipCount`")]
        int radarDetectionCount = 10;
        [SerializeField, Tooltip("The number of blips the radar will display. The radar will try to display only the closest items, however, the total number of items detected is controlled by `radarDetectionCount`, if this is too low it is possible the radar will not detect the nearest items.")]
        int radarBlipCount = 3;
        [SerializeField, Tooltip("The time interval between radar updates.")]
        float radarUpdateInterval = 0.25f;
        [SerializeField, Tooltip("The layer mask that the radar will detect.")]
        LayerMask radarLayerMask;
        [SerializeField, Tooltip("The offset from the radar blip parent that the radar blip lines will be drawn to.")]
        Vector3 startOffset = new Vector3(0, 0.4f, 0);

        Collider[] colliders;
        float[] colliderDistances;
        Queue<KeyValuePair<float, Collider>> m_collidersQueue = new Queue<KeyValuePair<float, Collider>>();
        bool isQueueInvalid = true;

        public Queue<KeyValuePair<float, Collider>> sortedColliders
        {
            get
            {
                if (isQueueInvalid)
                {
                    m_collidersQueue.Clear();
                    for (int i = 0; i < radarDetectionCount; i++)
                    {
                        m_collidersQueue.Enqueue(new KeyValuePair<float, Collider>(colliderDistances[i], colliders[i]));
                    }
                    isQueueInvalid = false;
                }
                return m_collidersQueue;
            }
        }

        /// <summary>
        /// Peek at the first non-null collider in the queue. If there are no colliders in the queue, this will return a default KeyValuePair (key = 0, collider = null).
        /// 
        /// If there are null colliders in the queue, they will be removed and the next collider will be peeked at.
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<float, Collider> Peek()
        {
            if (sortedColliders.Count > 0 && sortedColliders.Peek().Value == null)
            {
                sortedColliders.Dequeue();
                return Peek();
            }
            return sortedColliders.Count > 0 ? sortedColliders.Peek() : default(KeyValuePair<float, Collider>);
        }

        public KeyValuePair<float, Collider> Dequeue()
        {
            if (sortedColliders.Peek().Value == null)
            {
                sortedColliders.Dequeue();
                return Dequeue();
            }
            return sortedColliders.Dequeue();
        }

        public void Enqueue(float distance, Collider collider)
        {
            sortedColliders.Enqueue(new KeyValuePair<float, Collider>(distance, collider));
        }

        float radarRadiusSqr;
        LineRenderer[] lineRenderers;
        protected override NanobotPawnController nanobotPawn
        {
            get
            {
                if (m_NanobotPawn == null)
                {
                    m_NanobotPawn = FindObjectOfType<NanobotPawnController>();
                    lineRenderers = new LineRenderer[radarBlipCount];
                    for (int i = 0; i < radarBlipCount; i++)
                    {
                        GameObject blip = new GameObject($"RadarBlip {i}");
                        blip.transform.parent = m_NanobotPawn.transform;

                        LineRenderer lineRenderer = blip.AddComponent<LineRenderer>();
                        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                        lineRenderer.positionCount = 2;
                        lineRenderer.startWidth = 0.01f;
                        lineRenderer.endWidth = 0.05f;
                        lineRenderers[i] = lineRenderer;
                    }
                }
                return m_NanobotPawn;
            }
        }

        private void Awake()
        {
            radarRadiusSqr = radarRadius * radarRadius;
            colliders = new Collider[radarDetectionCount];
            colliderDistances = new float[radarDetectionCount];
        }

        private void Update()
        {
            if (nanobotPawn == null)
                return;

            Array.Clear(colliders, 0, radarBlipCount);
            int count = Physics.OverlapSphereNonAlloc(transform.position, radarRadius, colliders, radarLayerMask);
            for (int i = 0; i < radarDetectionCount; i++)
            {
                colliderDistances[i] = colliders[i] != null ? Vector3.Distance(transform.position, colliders[i].transform.position) : float.MaxValue;
            }

            Array.Sort(colliderDistances, colliders);
            isQueueInvalid = true;

            for (int i = 0; i < radarBlipCount; i++)
            {
                if (colliders[i] == null)
                {
                    lineRenderers[i].enabled = false;
                    continue;
                }

                lineRenderers[i].enabled = true;
                lineRenderers[i].SetPosition(0, colliders[i].transform.position);
                lineRenderers[i].SetPosition(1, nanobotPawn.transform.position + startOffset);
                lineRenderers[i].startColor = Color.Lerp(Color.red, Color.yellow, colliderDistances[i] / radarRadiusSqr);
                lineRenderers[i].endColor = Color.Lerp(Color.red, Color.yellow, colliderDistances[i] / radarRadiusSqr);
            }
        }
    }
}
