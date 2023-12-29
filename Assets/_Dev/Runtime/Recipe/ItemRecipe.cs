using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Playground
{
    /// <summary>
    /// Creates a recipe for an item. This is a generic class that can be used to create any item.
    /// </summary>
    /// <typeparam name="T">The type of item that will be created.</typeparam>
    /// <seealso cref="AmmoPickupRecipe"/>
    /// <seealso cref="WeaponPickupRecipe"/>
    /// <seealso cref="ToolPickupRecipe"/>
    [CreateAssetMenu(fileName = "Item Pickup Recipe", menuName = "Playground/Generic Item Pickup Recipe", order = 100)]
    public class ItemRecipe<T> : AbstractRecipe, IItemRecipe where T : MonoBehaviour
    {
        [Header("Item")]
        [SerializeField, Tooltip("The pickup item this recipe creates.")]
        [FormerlySerializedAs("item")]
        internal T pickup;

        public Component Item => pickup;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(uniqueID))
            {
                uniqueID = Guid.NewGuid().ToString();
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }
#endif
    }
}