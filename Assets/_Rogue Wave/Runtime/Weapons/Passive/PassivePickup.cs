using NeoFPS;
using UnityEngine;

namespace RogueWave
{
    public class PassivePickup : Pickup
    {
        [SerializeField, Tooltip("The inventory item prefab to give to the character.")]
        internal GameObject itemPrefab = null;
        [SerializeField, Tooltip("The inventory item recipe to give to the character.")]
        internal PassiveItemRecipe recipe = null;

        [Header("Audio")]
        [SerializeField, Tooltip("An optional set of audio clips, one of which will be played when this pickup is collected.")]
        internal AudioClip[] audioClips = default;

        public override void Trigger(ICharacter character)
        {
            base.Trigger(character);

            NanobotManager nanobotManager = character.GetComponent<NanobotManager>();
            if (nanobotManager != null)
            {
                RogueLiteManager.runData.Add(recipe);
                nanobotManager.AddToRunRecipes(recipe);
            }

            if (audioClips.Length > 0)
            {
                NeoFpsAudioManager.Play2DEffectAudio(audioClips[UnityEngine.Random.Range(0, audioClips.Length)]);
            }

            Destroy(gameObject);
        }

    }
}