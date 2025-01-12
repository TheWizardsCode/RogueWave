using System;
using UnityEngine;
using UnityEngine.Audio;

namespace WizardsCode.StoryTeller
{
    /// <summary>
    /// Adjust a setting in an audio mixer
    /// 
    /// Example usage:
    ///
    /// MIXER: the name of the Audio Mixer to work on. Note the name needs to be the complete path from the Resources folder, e.g. "Audio/Mixers/MyMixer"
    /// PARAMETER_NAME: the name of the exposed parameter in the mixer to adjust
    /// VALUE: the volume to set the group to (normalised 0-1)
    /// 
    /// </summary>
    /// <param name="paramaters">COMMAND, MIXER, PARAMETER_NAME, VALUE</param>
    public class AudioMixerDirection : AbstractDirection
    {
        public override string DirectionName => "Audio Mixer";

        public override void Execute(string[] parameters)
        {
            if (!ValidateArgumentCount(parameters, 3, 3))
            {
                return;
            }

            string mixerName = parameters[0];
            string parameterName = parameters[1];
            float value = float.Parse(parameters[2]);

            // OPTIMIZATION: cache the audio mixer in a dictionary
            AudioMixer audioMixer = Resources.Load<AudioMixer>(mixerName);
            audioMixer.SetFloat(parameterName, ConvertNormalizedToDb(value));
        }

        public static float ConvertNormalizedToDb(float targetValueNormalized)
        {
            if (targetValueNormalized < 0.001)
            {
                return -80;
            }
            else
            {
                return Mathf.Log10(targetValueNormalized) * 20f;
            }
        }
    }
}