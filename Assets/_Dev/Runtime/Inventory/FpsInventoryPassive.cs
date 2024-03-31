using NeoFPS;
using NeoFPS.Constants;
using UnityEngine;

namespace Playground
{
    public class FpsInventoryPassive : FpsInventoryItem, ISwappable
    {
        [SerializeField, Tooltip("The wieldable category.")]
        private FpsSwappableCategory m_Category = FpsSwappableCategory.Passive;

        public FpsSwappableCategory category
        {
            get { return m_Category; }
        }
    }
}