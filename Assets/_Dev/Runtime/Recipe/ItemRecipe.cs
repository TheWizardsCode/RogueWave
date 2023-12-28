using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Playground
{
    /// <summary>
    /// Creates a recipe for a item. This is a generic class that can be used to create any pickup item.
    /// However, some pickup items may require additional functionality. In that case, create a new class that
    /// extends this one. For example, see the WeaponPickupRecipe class.
    /// 
    /// We don't deal directly with the item itself because we want to enable both pickup spawning and direct creation.
    /// Therefore we work with the pickup prefab and create an instance when needed.
    /// </summary>
    /// <typeparam name="T">The type of item that will be created.</typeparam>
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