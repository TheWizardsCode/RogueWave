using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// An interface for recipes that create items. The type of item is identified by the Item property which can be
    /// any UnityEngine.Component.
    /// </summary>
    /// <seealso cref="GenericItemRecipe{T}"/>"/>
    internal interface IItemRecipe : IRecipe
    {
        public Component Item { get; }
    }
}