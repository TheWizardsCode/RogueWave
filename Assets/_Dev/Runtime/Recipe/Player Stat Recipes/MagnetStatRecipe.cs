using NeoFPS.SinglePlayer;
using UnityEngine;

namespace RogueWave
{
    [CreateAssetMenu(fileName = "Magnet Stats Recipe", menuName = "Rogue Wave/Recipe/Magnet Stats Recipe", order = 10)]
    // REFACTOR: can we remove this and make it a BaseStatRecipe instead?
    public class MagnetStatRecipe : GenericStatRecipe<MonoBehaviour>
    {
        [Header("Stat Modifier")]
        [SerializeField, Tooltip("The range multiplier to add to the magnets range.")]
        float rangeMultiplier = 1.00f;
        [SerializeField, Tooltip("A multiplier for the magnets strength, the stronger a magnet the more force it can exert on objects.")]
        float strengthMultiplier = 1.00f;

        public override string Category => "Magnet Stat";

        internal override void Apply()
        {
            MagnetController magnetController = FpsSoloCharacter.localPlayerCharacter.GetComponent<MagnetController>();
            if (magnetController != null)
            {
                if (rangeMultiplier > 0)
                    magnetController.range *= rangeMultiplier;


                if (strengthMultiplier != 0)
                    magnetController.speed *= strengthMultiplier;
            }
        }
    }
}