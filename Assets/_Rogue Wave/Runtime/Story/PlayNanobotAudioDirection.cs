using System;
using UnityEngine;
using WizardsCode.StoryTeller;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// Play a specified audio clip thjrough the Nanobot audio mixer channel. Clips requested should be saved in
    /// `Resources/Audio/TYPE/NAME`
    /// 
    /// Example usage:
    /// 
    /// TYPE: is an arbitrary FX type name
    /// NAME: is thename of the actual clip file
    /// 
    /// </summary>
    /// <param name="paramaters">SOURCE, TYPE, NAME[, LOOP_TRUE_OR_FALSE_DEFAULT_FALSE]</param>
    public class PlayNanobotAudioDirection : AbstractDirection
    {
        public override string DirectionName => "Play Nanobot Audio";

        public override void Execute(string[] parameters)
        {
            if (!ValidateArgumentCount(parameters, 2, 2))
            {
                return;
            }

            String path = "Audio";
            String clip = parameters[0].Trim() + "/" + parameters[1].Trim();
            AudioClip audio = Resources.Load<AudioClip>($"{path}/{clip}");
            if (audio)
            {
                AudioManager.PlayNanobotOneShot(audio);
            }
            else
            {
                Debug.LogError($"There is a direction to play nanobot audio '{path}/{clip}' but no resource file of that name exists.");
            }
        }
    }
}
