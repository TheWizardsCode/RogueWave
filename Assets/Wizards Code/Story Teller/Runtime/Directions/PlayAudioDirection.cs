using NAudio.CoreAudioApi;
using System;
using UnityEngine;

namespace WizardsCode.StoryTeller
{
    /// <summary>
    /// Play a specified audio clip. Clips requested should be saved in
    /// `Resources/Audio/TYPE/NAME`
    /// 
    /// Example usage:
    /// 
    /// SOURCE: is the game object from which the sound will be played (must have an AudioSource)
    /// TYPE: is an arbitrary FX type name
    /// NAME: is thename of the actual clip file
    /// LOOP: [OPTIONAL] is 'true' if you want the sound to loop, this defaults to false. Any other value will be interpreted as false.
    /// 
    /// </summary>
    /// <param name="paramaters">SOURCE, TYPE, NAME[, LOOP_TRUE_OR_FALSE_DEFAULT_FALSE]</param>
    public class PlayAudioDirection : AbstractDirection
    {
        public override string DirectionName => "Play Audio";

        public override void Execute(string[] parameters)
        {
            if(!ValidateArgumentCount(parameters, 3, 4))
            {
                return;
            }

            AudioSource source;
            Transform obj = StoryManager.Instance.FindTarget(parameters[0].Trim());
            if (!obj)
            {
                Debug.LogError($"Direction to play Audio clip with the arguments {string.Join(", ", parameters)} but no source object with the name {parameters[0].Trim()} can be found");
                return;
            }
            else
            {
                //OPTIMIZATION: cache audio source
                source = obj.GetComponentInChildren<AudioSource>();
                source.outputAudioMixerGroup.audioMixer.SetFloat("Volume", 1);
                if (!source)
                {
                    Debug.LogError($"Direction to play SoundFX with the arguments {string.Join(", ", parameters)} but no audio source was found on the the object with the name name {parameters[0].Trim()}.");
                    return;
                }
            }

            String path = "Audio";
            String clip = parameters[1].Trim() + "/" + parameters[2].Trim();
            AudioClip audio = Resources.Load<AudioClip>($"{path}/{clip}");
            if (audio)
            {
                bool isLooping = false;
                if (parameters.Length == 4 && parameters[3].ToLower().Trim() == "true")
                {
                    isLooping = true;
                }

                if (source.clip != audio)
                {
                    source.clip = audio;
                    source.loop = isLooping;
                    source.Play();
                }
            }
            else
            {
                Debug.LogError($"There is a direction to play the soundFX '{path}/{clip}' but no resource file of that name exists.");
            }
        }
    }
}