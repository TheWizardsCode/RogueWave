using NaughtyAttributes;
using NeoFPS;

namespace RogueWave
{
    public class BuildingController : DestructibleController
    {

        [Button]
        void DestroyBuilding()
        {
            GetComponent<BasicHealthManager>().SetHealth(0, false, null);
        }
    }
}