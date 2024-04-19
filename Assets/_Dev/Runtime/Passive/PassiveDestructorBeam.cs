using NeoFPS;
using System.Collections;
using UnityEngine;

namespace RogueWave
{
    public class PassiveDestructorBeam : PassiveWeapon
    {
        [Header("Beam Configuration")]
        [SerializeField, Tooltip("The number of beams to fire.")]
        private int beamCount = 4;
        [SerializeField, Tooltip("The size of the beam. This is the radius in meters.")]
        private float beamSize = 0.5f;
        [SerializeField, Tooltip("The material to use for the beam.")]
        private Material material;

        private LineRenderer[] lineRenderers;

        internal override void Awake()
        {
            base.Awake();

            lineRenderers = new LineRenderer[beamCount];
            for (int i = 0; i < beamCount; i++)
            {
                GameObject go = new GameObject("Beam " + i);
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0, 1f, 0);
                lineRenderers[i] = go.AddComponent<LineRenderer>();
                lineRenderers[i].material = material;
                lineRenderers[i].startWidth = beamSize;
                lineRenderers[i].endWidth = beamSize;
            }
        }

        public override void Fire()
        {
            base.Fire();

            FireBeams(beamCount);
        }

        public void FireBeams(int beamCount)
        {
            float angleStep = 360f / beamCount;
            float startAngle = 0f;

            for (int i = 0; i < beamCount; i++)
            {
                StartCoroutine(FireBeam(startAngle + angleStep * i, lineRenderers[i]));
            }
        }

        public IEnumerator FireBeam(float angle, LineRenderer lineRenderer)
        {
            RaycastHit hit;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            Vector3 endPoint;

            if (Physics.BoxCast(transform.position, new Vector3(beamSize, beamSize * 2, beamSize), direction, out hit, Quaternion.identity, range, layerMask))
            {
                IDamageHandler damageHandler = hit.transform.GetComponent<IDamageHandler>();

                if (damageHandler != null)
                {
                    damageHandler.AddDamage(damage);
                }

                endPoint = hit.point;
            }
            else
            {
                endPoint = transform.position + direction * range;
            }

            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, lineRenderer.transform.position);
            lineRenderer.SetPosition(1, endPoint);

            yield return new WaitForSeconds(0.2f);

            lineRenderer.enabled = false;
        }
    }
}
