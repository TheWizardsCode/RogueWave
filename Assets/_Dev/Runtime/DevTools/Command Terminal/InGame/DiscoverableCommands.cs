using NeoFPS.SinglePlayer;
using UnityEditor;
using UnityEngine;
using WizardsCode.CommandTerminal;

namespace WizardsCode.RogueWave.CommandTerminal.InGame
{
    public class DiscoverableCommands
    {
#if UNITY_EDITOR
        [RegisterCommand(Help = "Create a random discoverable 10 meteres in front of the player.", MinArgCount = 0, MaxArgCount = 0)]
        static void SpawnDiscoverable(CommandArg[] args)
        {   
            DiscoverableController controller = AssetDatabase.LoadAssetAtPath<DiscoverableController>("Assets/_Dev/Resources/Prefabs/Destructible/Discoverable Item.prefab");
            DiscoverableController item = GameObject.Instantiate(controller, FpsSoloCharacter.localPlayerCharacter.transform.position + FpsSoloCharacter.localPlayerCharacter.transform.forward * 10, FpsSoloCharacter.localPlayerCharacter.transform.rotation);
        }
#endif
    }
}
