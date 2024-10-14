using NaughtyAttributes;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    public class LineWeaponBehaviour : WeaponBehaviour
    {
        [SerializeField, Tooltip("A line renderer to show what enemy is locking on and where they are currently targeting."), Required, BoxGroup("Effects")]
        private LineRenderer _lineRenderer;

        protected LineRenderer lineRenderer { get => _lineRenderer; }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }

        public override void StartBehaviour(Transform target)
        {
            base.StartBehaviour(target);
            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
            }
        }

        public override void StopBehaviour()
        {
            base.StopBehaviour();
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }

        /// <summary>
        /// Update the effects to match the current state. Unless you set `force = true`
        /// the update may not happen, depending on optimization settings.
        /// </summary>
        /// <param name="force"></param>
        protected override void UpdateEffects(bool force)
        {
            // OPTIMIZATION: Do we want to update the effects every frame?
            if (lineRenderer != null && target != null)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, target.position);
            }
        }
    }
}
