using MoreMountains.Feedbacks;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace WizardsCode.RogueWave
{
    public class BasicWeaponBehaviour : MonoBehaviour
    {
        [ValidateInput("Validate", "This behaviour is not valid, run the validator in the Dev Management Window for details.")]

        [SerializeField, Tooltip("Should this behaviour have a fixed duration or should it be stopped manually by calling `StopNebhaviour`? Note, even if set to a fixed duration it can still be stopped early with `StopBehaviour`."), BoxGroup("Metadata")]
        private bool hasFixedDuration = true;
        [SerializeField, Tooltip("The duration the behaviour should be active after it has been started. Set to 0 to leave it active until the behaviour is stopped."), ShowIf("hasFixedDuration"), BoxGroup("Metadata")]
        private float duration = 0.5f;

        //[Header("Audio")]
        [SerializeField, Tooltip("Should this audio be played as a 3D sound? If true this will be played in 3D space, if false it will be played in 2D space with position encoded in the sound itself."), FormerlySerializedAs("has3DSound"), BoxGroup("Audio")]
        private bool has3DSound = false;
        [SerializeField, Tooltip("The audio to play when this behaviour is started."), BoxGroup("Audio")]
        private AudioClip[] audioClips;
        [SerializeField, Tooltip("The object to attach the audio to if it is a 3D sound. If this is left empty then the audio will be played as 2D sound with no position."), ShowIf("has3DSound"), BoxGroup("Audio")]
        private Transform audioPosition;

        // Events
        [SerializeField, Tooltip("An event that will be triggered when this behaviour is started. Note that this is a generic start event, it is fired by all behaviours and may not represent what you are looking for in some specific use cases. Look for other events available."), Foldout("Events")]
        public UnityEvent onBehaviourStarted;
        [SerializeField, Tooltip("An event that will be triggered when this behaviour is stopped. Note that this is a generic stop event, it is fired by all behaviours and may not represent what you are looking for in some specific use cases. Look for other events available."), Foldout("Events")]
        public UnityEvent onBehaviourStopped;

        protected MMF_Player feelPlayer;
        protected AudioSource audioSource;
        protected Transform target;
        private bool isRunning = false;
        private bool isStopping = false;
        private float endTime = float.MaxValue;

        protected virtual void OnEnable()
        {
            feelPlayer = GetComponent<MMF_Player>();
        }

        protected virtual void OnDisable()
        {
            feelPlayer = null;
        }

        public virtual void StartBehaviour(Transform target)
        {
            endTime = Time.time + duration;

            isRunning = true;
            isStopping = false;

            this.target = target;

            feelPlayer?.PlayFeedbacks();
            if (audioClips.Length > 0)
            {
                if (has3DSound)
                {
                    audioSource = AudioManager.Play3DEnemyOneShot(audioClips[Random.Range(0, audioClips.Length)], audioPosition.position);
                }
                else
                {
                    audioSource = AudioManager.Play2DEnemyOneShot(audioClips[Random.Range(0, audioClips.Length)]);
                }
            }

            onBehaviourStarted?.Invoke();
        }

        public virtual void StopBehaviour()
        {
            if (isStopping) return;

            isRunning = false;
            isStopping = true;

            feelPlayer?.StopFeedbacks();
            if (audioClips.Length > 0 && audioSource != null)
            {
                AudioManager.FadeOut(audioSource, 0.2f);
            }

            onBehaviourStopped?.Invoke();
        }

        protected virtual void Update()
        {
            if (!isRunning) return;

            if (hasFixedDuration && !isStopping && endTime <= Time.time)
            {
                StopBehaviour();
            }


            // OPTIMIZATION: Control how frequently this updates
            UpdateEffects(false);
        }

        protected virtual void UpdateEffects(bool force)
        {
            // Subclasses should override this to update effects
        }

        internal bool Validate()
        {
            return IsValid(out string message);
        }

        internal virtual bool IsValid(out string message)
        {
            message = string.Empty;

            if (has3DSound && audioPosition == null)
            {
                message = "Audio Position is required for 3D sound.";
                return false;
            }

            return true;
        }
    }
}
