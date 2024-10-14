using MoreMountains.Feedbacks;
using NaughtyAttributes;
using NeoFPS.Samples.SinglePlayer.MultiScene;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    public class WeaponBehaviour : MonoBehaviour
    {
        [SerializeField, Tooltip("Should this behaviour have a fixed duration or should it be stopped manually by calling `StopNebhaviour`? Note, even if set to a fixed duration it can still be stopped early with `StopBehaviour`."), BoxGroup("Effects")]
        private bool hasFixedDuration = true;
        [SerializeField, Tooltip("The duration the behaviour should be active after it has been started. Set to 0 to leave it active until the behaviour is stopped."), ShowIf("hasFixedDuration"), BoxGroup("Effects")]
        private float duration = 0.5f;

        //[Header("Audio")]
        [SerializeField, Tooltip("Should this audio be played as a 3D sound? If true this will be played in 3D space, if false it will be played in 2D space with position encoded in the sound itself."), BoxGroup("Audio")]
        private bool is3DSound = false;
        [ValidateInput("HasValidAudio", "At least one clip is valid, see Errors in the console for more information.")]
        [SerializeField, Tooltip("The audio to play when this behaviour is started."), BoxGroup("Audio")]
        private AudioClip[] audioClips;
        [SerializeField, Tooltip("The object to attach the audio to if it is a 3D sound. If this is left empty then the audio will be played as 2D sound with no position."), ShowIf("is3DSound"), BoxGroup("Audio")]
        private Transform audioPosition;

        //[Header("Events")]
        [SerializeField, Tooltip("An event that will be triggered when this behaviour is started. Note that this is a generic start event, it is fired by all behaviours and may not represent what you are looking for in some specific use cases. Look for other events available."), BoxGroup("Events")]
        private GameEvent behaviourStarted;
        [SerializeField, Tooltip("An event that will be triggered when this behaviour is stopped. Note that this is a generic stop event, it is fired by all behaviours and may not represent what you are looking for in some specific use cases. Look for other events available."), BoxGroup("Events")]
        private GameEvent behaviourStopped;

        protected MMF_Player feelPlayer;
        protected AudioSource audioSource;
        protected Transform target;
        private bool isStopping;
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

            isStopping = false;

            this.target = target;

            feelPlayer?.PlayFeedbacks();
            if (audioClips.Length > 0)
            {
                if (is3DSound)
                {
                    audioSource = AudioManager.Play3DEnemyOneShot(audioClips[Random.Range(0, audioClips.Length)], audioPosition.position);
                }
                else
                {
                    audioSource = AudioManager.Play2DEnemyOneShot(audioClips[Random.Range(0, audioClips.Length)]);
                }
            }

            behaviourStarted?.Raise();
        }

        public virtual void StopBehaviour()
        {
            if (isStopping) return;

            isStopping = true;
            feelPlayer?.StopFeedbacks();
            if (audioClips.Length > 0 && audioSource != null)
            {
                StartCoroutine(AudioManager.FadeOut(audioSource, 0.2f));
            }

            behaviourStopped?.Raise();
        }

        protected virtual void Update()
        {
            if (hasFixedDuration && !isStopping && endTime <= Time.time)
            {
                StopBehaviour();
            }

            UpdateEffects(false);
        }

        protected virtual void UpdateEffects(bool force)
        {
            // OPTIMIZATION: Do we want to update the effects every frame?
        }

#if UNITY_EDITOR
        bool HasValidAudio()
        {
            if (audioClips.Length == 0)
            {
                return true;
            }

            foreach (AudioClip clip in audioClips)
            {
                if (clip == null)
                {
                    Debug.LogError($"At least one Audio clip is null.");
                    return false;
                }

                if (is3DSound && clip.channels > 1)
                {
                    Debug.LogError("Clip " + clip.name + " has more than one channel but is set to be played as a 3D sound. It should only have one channel.");
                    return false;
                }
            }

            return true;
        }
#endif
    }
}
