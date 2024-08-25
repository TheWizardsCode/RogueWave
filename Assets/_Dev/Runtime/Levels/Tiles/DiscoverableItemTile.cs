using UnityEngine;

namespace RogueWave
{
    public class DiscoverableItemTile : BaseTile
    {
        [Header("Discoverable Content")]
        [SerializeField,  Tooltip("The prefab for the discoverable item that will be placed on this tile.")]
        Transform itemPrefab = null;

        internal override void GenerateTileContent(int x, int y, BaseTile[,] tiles, LevelGenerator levelGenerator)
        {
            PlaceItem(Instantiate(itemPrefab, transform).transform);
            base.GenerateTileContent(x, y, tiles, levelGenerator);
        }
    }
}
