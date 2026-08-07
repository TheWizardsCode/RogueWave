using RogueWave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using WizardsCode.StoryTeller;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// RogueWaveSavePoint is used to record the current game status.
    /// 
    /// It saves the state of the game and story at the point that this direction is executed.
    /// 
    /// Parameters:
    /// 
    /// KNOT_NAME - the name of the knot at which the story UI should restart.
    /// 
    /// </summary>
    public class RogueWaveSavePointDirection : SetSavePointDirection
    {
        public override string DirectionName => "Rogue Wave Save Point";
        
        public override void Execute(string[] parameters)
        {
            if (!ValidateArgumentCount(parameters, 1, 1))
            {
                return;
            }

            base.Execute(parameters);

            RogueLiteManager.SaveProfile();
        }

    }
}
