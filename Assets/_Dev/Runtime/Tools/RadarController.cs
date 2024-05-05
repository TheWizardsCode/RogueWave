using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave
{
    public class RadarController : MonoBehaviour
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
        float radarRadiusSqr;
        LineRenderer[] lineRenderers;
        NanobotPawnController nanobotPawn;

        private void Awake()
        {
            nanobotPawn = FindAnyObjectByType<NanobotPawnController>();
            if (nanobotPawn == null)
            {
                Debug.LogError("RadarController: Could not find NanobotPawnController in scene. The Nanobot Pawn is supposed to be a dependency of the Radar, what happened here?");
                enabled = false;
                return;
            }

            radarRadiusSqr = radarRadius * radarRadius;
            colliders = new Collider[radarDetectionCount];
            colliderDistances = new float[radarDetectionCount];
            lineRenderers = new LineRenderer[radarBlipCount];
            for (int i = 0; i < radarBlipCount; i++)
            {
                GameObject blip = new GameObject($"RadarBlip {i}");
                blip.transform.parent = nanobotPawn.transform;

                LineRenderer lineRenderer = blip.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.positionCount = 2;
                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.01f;
                lineRenderers[i] = lineRenderer;
            }
        }

        private void Update()
        {
            Array.Clear(colliders, 0, radarBlipCount);
            int count = Physics.OverlapSphereNonAlloc(transform.position, radarRadius, colliders, radarLayerMask);
            for (int i = 0; i < radarDetectionCount; i++)
            {
                colliderDistances[i] = colliders[i] != null ? (transform.position - colliders[i].transform.position).sqrMagnitude : float.MaxValue;
            }

            Array.Sort(colliderDistances, colliders);

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
