using NaughtyAttributes;
using NUnit.Framework;
using UnityEngine;

namespace RogueWave
{
    public class MultiCellTile : BaseTile
    {
        private const string k_SubcellsObjectName = "Subcells";
        [SerializeField, Tooltip("The number of map tiles that this tile occupies in the level. This is used to determine the size of the tile.")]
        Vector3 m_TileArea = Vector3.one;
        [SerializeField, Tooltip("The parent of the subcells for this area.")]
        private GameObject subcells;

        bool hasGenerated = false;

        internal override Vector3 TileArea => m_TileArea;

        public override void Generate(int x, int y, BaseTile[,] tiles, LevelGenerator levelGenerator)
        {
            if (hasGenerated)
            {
                return;
            }

            this.levelGenerator = levelGenerator;
            tileWidth = levelGenerator.levelDefinition.lotSize.x;
            tileDepth = levelGenerator.levelDefinition.lotSize.y;

            foreach (Transform cell in subcells.GetComponentsInChildren<Transform>())
            {
                BaseTile tile = cell.GetComponent<BaseTile>();
                if (tile != null)
                {
                    tile.tileDefinition = tileDefinition;
                    tile.Generate(x, y, tiles, levelGenerator);
                }
            }
            
            GenerateEnemies(x, y, tiles, levelGenerator);

            hasGenerated = true;
        }

#if UNITY_EDITOR
        [Button]
        void CreateSubCells()
        {
            // TODO: don't hard code the subcell name, probably want a controller object on this.
            if (subcells != null) 
            {
                if (UnityEditor.EditorUtility.DisplayDialog("Delete Subcells", "Are you sure you want to delete the subcells?", "Yes", "No"))
                {
                    DestroyImmediate(subcells);
                }
                else
                {
                    return;
                }
            }

            subcells = new GameObject(k_SubcellsObjectName);
            subcells.transform.parent = transform;

            for (int i = 0; i < TileArea.x; i++)
            {
                for (int j = 0; j < TileArea.z; j++)
                {
                    GameObject subcell = new GameObject($"Subcell ({i}, {j})");
                    subcell.transform.parent = subcells.transform;
                    subcell.transform.localPosition = new Vector3(i * tileWidth, 0, j * tileDepth);

                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.parent = subcell.transform;
                    cube.transform.localPosition = new Vector3(tileWidth / 2, 0, tileDepth / 2); ;
                    cube.transform.localScale = new Vector3(tileWidth * 0.8f, (tileWidth + tileDepth) / 2, tileDepth * 0.8f);
                }
            }
        }
#endif
    }
}
