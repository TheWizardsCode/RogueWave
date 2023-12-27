using NeoFPS.SinglePlayer;
using NeoFPS;
using System.Collections.Generic;
using System.Reflection;

namespace Playground
{
    public static class InteractivePickupExtension
    {
        public static FpsInventoryItemBase GetItemPrefab(this InteractivePickup instance)
        {
            var fieldInfo = typeof(InteractivePickup).GetField("m_Item", BindingFlags.NonPublic | BindingFlags.Instance);
            return fieldInfo.GetValue(instance) as FpsInventoryItemBase;
        }
    }

    public static class LoadoutBuilderSlotExtensions
    {
        public static void AddOption(this LoadoutBuilderSlot slot, FpsInventoryItemBase option)
        {
            var field = typeof(LoadoutBuilderSlot).GetField("m_Options", BindingFlags.NonPublic | BindingFlags.Instance);
            var options = (FpsInventoryItemBase[])field.GetValue(slot);
            var optionsList = new List<FpsInventoryItemBase>(options) { option };
            field.SetValue(slot, optionsList.ToArray());
        }
    }
}