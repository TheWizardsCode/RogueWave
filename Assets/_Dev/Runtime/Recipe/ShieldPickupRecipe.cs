using NeoFPS;
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
    [CreateAssetMenu(fileName = "Shield Pickup Recipe", menuName = "Playground/Shield Pickup Recipe", order = 100)]
    public class ShieldPickupRecipe : ItemRecipe<ShieldPickup>
    {
    }
}