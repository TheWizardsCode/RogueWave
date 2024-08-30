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
        [SerializeField, Tooltip("The number of blips the radar will display. The radar will try to display only the closest items, however, the total number of items detected is controlled by `radarDetectionCount`, if this is too low it is possible the radar will not detect the nearest items.")]
        int radarBlipCount = 3;
        [SerializeField, Tooltip("The offset from the radar blip parent that the radar blip lines will be drawn to.")]
        Vector3 startOffset = new Vector3(0, 0.4f, 0);


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

                        lineRenderer.textureMode = LineTextureMode.Tile;
                        lineRenderer.material.mainTexture = GenerateDashTexture();
                        lineRenderer.material.mainTextureScale = new Vector2(1f / lineRenderer.startWidth, 1.0f);

                        lineRenderers[i] = lineRenderer;
                    }
                }
                return m_NanobotPawn;
            }
        }

        private void Awake()
        {
            radarRadiusSqr = radarRadius * radarRadius;
        }

        private void Update()
        {
            if (nanobotPawn == null)
                return;

            for (int i = 0; i < radarBlipCount; i++)
            {
                KeyValuePair<float, Collider> detectedObject = nanobotPawn.ObjectAt(i);
                if (detectedObject.Value == null)
                {
                    lineRenderers[i].enabled = false;
                    continue;
                }

                lineRenderers[i].enabled = true;
                lineRenderers[i].SetPosition(0, detectedObject.Value.transform.position);
                lineRenderers[i].SetPosition(1, nanobotPawn.transform.position + startOffset);
                lineRenderers[i].startColor = Color.Lerp(Color.red, Color.yellow, detectedObject.Key / radarRadius);
                lineRenderers[i].endColor = Color.Lerp(Color.red, Color.yellow, detectedObject.Key / radarRadius);
            }
        }

        private Texture2D GenerateDashTexture()
        {
            Texture2D texture = new Texture2D(2, 1);
            texture.SetPixel(0, 0, Color.clear);
            texture.SetPixel(1, 0, Color.white);
            texture.Apply();
            return texture;
        }
    }
}
