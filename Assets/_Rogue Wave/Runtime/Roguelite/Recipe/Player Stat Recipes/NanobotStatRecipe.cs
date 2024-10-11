using NeoFPS.SinglePlayer;
using UnityEngine;

namespace RogueWave
{
    [CreateAssetMenu(fileName = "Nanobot Stat Recipe", menuName = "Rogue Wave/Recipe/Nanobot Stat", order = 10)]
    public class NanobotStatRecipe : BaseStatRecipe
    {
        public override string Category => "Tools";

        internal override void Apply()
        {
            Apply(FpsSoloCharacter.localPlayerCharacter.GetComponentInChildren<NanobotManager>() as MonoBehaviour);
        }
    }
}
