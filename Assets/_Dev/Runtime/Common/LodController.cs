using NeoFPS.SinglePlayer;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// The LOD Controller is responsible for managing the level of detail of the game objects.
    /// The designer provides a list of components that should be enabled or disabled based on the level of detail.
    /// This is used to optimize the game by disabling components that are not needed when a certain distance from the player.
    /// </summary>
    public class LodController : MonoBehaviour
    {
        [SerializeField, Tooltip("The list of components that should be enabled or disabled based on the level of detail.")]
        private MonoBehaviour[] components = default;
        [SerializeField, Tooltip("If true any animator in the hierarchy of this Game Object will be enabled/disabled based on the LOD.")]
        private bool controlAnimator = false;
        [SerializeField, Tooltip("The distance from the player at which the components should be enabled.")]
        private float distance = 10f;
        [SerializeField, Tooltip("The frequency, in seconds, at which the LOD controller should update.")]
        private float updateFrequency = 2f;

        Animator[] animators = default;
        float sqrDistance;
        int currentLOD = 0;

        private void Start()
        {
            sqrDistance = distance * distance;

            animators = GetComponentsInChildren<Animator>();
            InvokeRepeating("UpdateLOD", 0f, updateFrequency);
        }

        private void UpdateLOD()
        {
            if (FpsSoloCharacter.localPlayerCharacter != null 
                && Vector3.SqrMagnitude(FpsSoloCharacter.localPlayerCharacter.transform.position - transform.position) <= sqrDistance)
            {
                ConfigureComponents(0);
            }
            else
            {
                ConfigureComponents(1);
            }
        }

        private void ConfigureComponents(int lodLevel)
        {
            if (lodLevel == currentLOD)
                return;

            currentLOD = lodLevel;
            bool state = lodLevel == 0;

            if (controlAnimator)
            {
                foreach (Animator animator in animators)
                {
                    animator.enabled = state;
                }
            }

            foreach (MonoBehaviour component in components)
            {
                component.enabled = state;
            }
        }
    }
}
