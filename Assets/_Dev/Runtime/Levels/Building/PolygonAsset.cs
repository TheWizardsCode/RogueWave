using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RogueWave.Procedural
{
    [CreateAssetMenu(menuName = "Rogue Wave/Buildings/Foundation Polygon", order = 0)]
    public class PolygonAsset : ScriptableObject
    {
        [SerializeField, Tooltip("If set to true then vertices will be generated automatically.")]
        internal bool autoGenerate = false;
        [SerializeField, Tooltip("Randomize the number of sides of the polygon."), ShowIf("autoGenerate")]
        internal bool randomizeSides = false;
        [SerializeField, Tooltip("The number of sides of the polygon."), MinValue(3), MaxValue(15), ShowIf("autoGenerate")]
        int sides = 6;
        [SerializeField, Tooltip("The size of each side in the foundations polygon."), MinMaxSlider(5, 20)]
        private Vector2Int sideLength = new Vector2Int(5, 20);
        [SerializeField, Tooltip("If true, the polygon will be a star shape. Star shapes move alternatve vertices in towards the center."), ShowIf("autoGenerate")]
        bool isStar = false;
        [SerializeField, Tooltip("The vertices of the polygon."), HideIf("autoGenerate")]
        internal List<Vector2> vertices = new List<Vector2>();

        public List<Vector2> GenerateShape(int sides, int sideLength, bool isStar = false)
        {
            if (sides < 3)
            {
                Debug.LogWarning("A shape must have at least 3 sides. Defaulting to 3.");
                sides = 3;
            }

            List<Vector2> coordinates = new List<Vector2>();
            float angleStep = 360.0f / sides;

            for (int i = 0; i < sides; i++)
            {
                float angleInDegrees = i * angleStep;
                float angleInRadians = MathF.PI / 180.0f * angleInDegrees;
                Vector2 point = new Vector2(MathF.Sin(angleInRadians), MathF.Cos(angleInRadians));

                if (isStar && i % 2 == 0)
                {
                    point *= sideLength * 0.5f; // Adjust the radius for every second point to create a star shape
                } else
                {
                    point *= sideLength;
                }

                coordinates.Add(point);
            }

            return coordinates;
        }

        [Button, ShowIf("autoGenerate")]
        internal void Randomize()
        {
            if (!autoGenerate) {
                Debug.Log("Auto Generate is set to false. Refusing to randomize. Set Auto Generate to true if you want to overwrite the current vectors.");
            }

            if (randomizeSides)
            {
                sides = Random.Range(3, 15);
            }

            isStar = Random.value >= 0.5f;
            OnValidate();
        }

        private void OnValidate()
        {

            if (autoGenerate)
            {
                vertices = GenerateShape(sides, Random.Range(sideLength.x, sideLength.y), isStar);
            }
        }
    }
}
