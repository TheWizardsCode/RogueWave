using NeoFPS;
using NeoFPS.SinglePlayer;
using UnityEngine;

namespace RogueWave
{
    [CreateAssetMenu(fileName = "Health Recipe", menuName = "Rogue Wave/Recipe/Health Recipe", order = 10)]
    public class HealthRecipe : BaseStatRecipe
    {
        public override string Category => "Health";

        internal override void Apply()
        {
            Apply(FpsSoloCharacter.localPlayerCharacter.GetComponentInChildren<BasicHealthManager>() as MonoBehaviour);
        }
    }
}
