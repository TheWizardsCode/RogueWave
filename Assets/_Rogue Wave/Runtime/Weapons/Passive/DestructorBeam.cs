using NeoFPS;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    public class DestructorBeam : PassiveWeapon
    {
        [Header("Beam Configuration")]
        [SerializeField, Tooltip("The number of beams to fire.")]
        private int beamCount = 2;
        [SerializeField, Tooltip("The size of the beam. This will translate to a box that is beamSize meters square.")]
        private float beamSize = 0.5f;
        [SerializeField, Tooltip("The duration of the beam in seconds.")]
        private float beamDuration = 0.5f;
        [SerializeField, Tooltip("The start angle of the beam in degrees. This is the angle of the first beam.")]
        float startAngle = -35f;
        [SerializeField, Tooltip("The end angle of the beam in degrees. This is the angle of the last beam.")]
        float endAngle = 35f;
        [SerializeField, Tooltip("The material to use for the beam.")]
        private Material material;

        private LineRenderer[] lineRenderers;
        private float[] firingEndTime;

        internal override void Awake()
        {
            base.Awake();
        }

        private void SetLineRenderers()
        {
            if (lineRenderers != null && lineRenderers.Length == beamCount)
                return;

            if (lineRenderers != null && lineRenderers.Length > 0) {
                foreach (LineRenderer lr in lineRenderers) {
                    Destroy(lr.gameObject);
                }
            }

            lineRenderers = new LineRenderer[beamCount];
            firingEndTime = new float[beamCount];
            for (int i = 0; i < beamCount; i++)
            {
                GameObject go = new GameObject("Beam " + i);
                go.transform.SetParent(transform);
                go.transform.localPosition = positionOffset;
                lineRenderers[i] = go.AddComponent<LineRenderer>();
                lineRenderers[i].material = material;
                lineRenderers[i].startWidth = 0.01f;
                lineRenderers[i].endWidth = beamSize;

                firingEndTime[i] = 0;
            }
        }

        public override void Fire()
        {
            PlayFireSFX();
            FireBeams();
        }

        public void FireBeams()
        {
            for (int i = 0; i < beamCount; i++)
            {
                firingEndTime[i] = beamDuration + Time.timeSinceLevelLoad;
            }
        }
        

        protected override void Update()
        {
            SetLineRenderers();

            float angleStep = (endAngle - startAngle) / (beamCount - 1);

            for (int idx = 0; idx < beamCount; idx++)
            {
                if (firingEndTime[idx] < Time.timeSinceLevelLoad )
                {
                    base.Update();
                    lineRenderers[idx].enabled = false;
                    continue;
                }

                float angle = startAngle + angleStep * idx;
                RaycastHit hit;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
                Vector3 endPoint;

                if (Physics.BoxCast(transform.position, new Vector3(beamSize, beamSize, beamSize), direction, out hit, Quaternion.identity, range, layers))
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

                lineRenderers[idx].enabled = true;
                lineRenderers[idx].SetPosition(0, lineRenderers[idx].transform.position);
                lineRenderers[idx].SetPosition(1, endPoint);
            }
        }
    }
}
