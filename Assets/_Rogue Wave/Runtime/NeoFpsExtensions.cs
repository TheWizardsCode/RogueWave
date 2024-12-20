using NeoFPS.SinglePlayer;
using NeoFPS;
using System.Collections.Generic;
using System.Reflection;
using NeoFPS.Constants;

namespace RogueWave
{
    public static class InteractivePickupExtension
    {
        public static FpsInventoryItemBase GetItemPrefab(this InteractivePickup instance)
        {
            var fieldInfo = typeof(InteractivePickup).GetField("m_Item", BindingFlags.NonPublic | BindingFlags.Instance);
            return fieldInfo.GetValue(instance) as FpsInventoryItemBase;
        }
    }
    
    public static class InventoryItemPickupExtension
    {
        public static FpsInventoryItemBase GetItemPrefab(this InventoryItemPickup instance)
        {
            var fieldInfo = typeof(InventoryItemPickup).GetField("m_ItemPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
            return fieldInfo.GetValue(instance) as FpsInventoryItemBase;
        }
    }

    public static class  HealthPickupExtension
    {
        public static float GetHealAmount(this HealthPickup instance)
        {
            var fieldInfo = typeof(HealthPickup).GetField("m_HealAmount", BindingFlags.NonPublic | BindingFlags.Instance);
            return (float)fieldInfo.GetValue(instance);
        }

        public static HealthPickup.HealType GetHealType(this HealthPickup instance)
        {
            var fieldInfo = typeof(HealthPickup).GetField("m_HealType", BindingFlags.NonPublic | BindingFlags.Instance);
            return (HealthPickup.HealType)fieldInfo.GetValue(instance);
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

    public static class ShieldManagerExtensions
    {
        public static void AddDamageMitigation(this ShieldManager instance, float value)
        {
            var field = typeof(ShieldManager).GetField("m_DamageMitigation", BindingFlags.NonPublic | BindingFlags.Instance);
            var damageMitigation = (float)field.GetValue(instance);
            damageMitigation += value;
            field.SetValue(instance, damageMitigation);
        }
    }

    public static class HudAdvancedCrosshairExtensions
    {
        public static HudAdvancedCrosshairStyleBase GetCurrentCrosshair(this HudAdvancedCrosshair instance)
        {
            var field = typeof(HudAdvancedCrosshair).GetField("m_CurrentCrosshair", BindingFlags.NonPublic | BindingFlags.Instance);
            FpsCrosshair crosshair = (FpsCrosshair)field.GetValue(instance);
            HudAdvancedCrosshairStyleBase[] crosshairs = (HudAdvancedCrosshairStyleBase[])typeof(HudAdvancedCrosshair).GetField("m_Crosshairs", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);

            return crosshairs[crosshair];
        }
    }
}