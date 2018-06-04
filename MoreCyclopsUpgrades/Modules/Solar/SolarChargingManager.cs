﻿namespace MoreCyclopsUpgrades
{
    using UnityEngine;

    /// <summary>
    /// This class handles the solar power charging.
    /// </summary>
    internal static class SolarChargingManager
    {
        private const float baseSolarChargingFactor = 0.03f;
        private const float maxDepth = 200f;

        public static void UpdateSolarCharger(ref SubRoot __instance)
        {
            Equipment modules = __instance.upgradeConsole.modules;
            int numberOfSolarChargers = 0; // Yes, they stack!

            foreach (string slotName in SlotHelper.SlotNames)
            {
                TechType techTypeInSlot = modules.GetTechTypeInSlot(slotName);

                if (techTypeInSlot == SolarCharger.CySolarChargerTechType)
                {
                    numberOfSolarChargers++; // Yes, they stack!                    
                }
            }

            if (numberOfSolarChargers > 0)
            {
                // The code here mostly replicates what the UpdateSolarRecharge() method does from the SeaMoth class.
                // Consessions were made for the differences between the Seamoth and Cyclops upgrade modules.
                DayNightCycle main = DayNightCycle.main;
                if (main == null)
                {
                    return; // This was probably put here for safety
                }

                // This is 1-to-1 the same way the Seamoth calculates its solar charging rate.
                float proximityToSurface = Mathf.Clamp01((maxDepth + __instance.transform.position.y) / maxDepth);
                float localLightScalar = main.GetLocalLightScalar();

                float chargeAmt = baseSolarChargingFactor * localLightScalar * proximityToSurface * numberOfSolarChargers;
                // Yes, the charge rate does scale linearly with the number of solar chargers.
                // I figure, you'd be giving up a lot of slots for good upgrades to do it so you might as well get the benefit.
                // So no need to bother with coding in dimishing returns.

                __instance.powerRelay.AddEnergy(chargeAmt, out float amtStored);
            }
        }
    }
}