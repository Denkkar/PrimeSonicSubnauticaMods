﻿namespace UpgradedVehicles
{
    using UnityEngine;
    using Common;
    using System.Reflection;
    using System;

    internal class VehicleUpgrader
    {
        internal const float SeaMothNormalHP = 200f;
        internal const float SeaMothMk2HP = 400f;
        
        private static readonly float[] ForwardForces = { 13f, 17f };
        private static readonly float[] BackwardForces = { 5f, 7f };
        private static readonly float[] SidewardForces = { 11.5f, 15f  };

        internal static void UpgradeSeaMoth(SeaMoth seamoth)
        {
            string classId = seamoth.GetComponent<PrefabIdentifier>().ClassId;
            if (classId != SeaMothMk2.NameID)
            {
                SetSeamothSpeed(seamoth, 0);
                return; // This is a normal Seamoth. Do not upgrade.
            }

            // Minimum crush depth of 900 without upgrades
            float extraCrush = seamoth.crushDamage.extraCrushDepth;

            seamoth.crushDamage.SetExtraCrushDepth(Mathf.Max(700f, extraCrush));

            var deluxeStorage = seamoth.gameObject.GetComponent<SeaMothStorageDeluxe>();

            if (!deluxeStorage.Initialized)
            {
                Console.WriteLine($"[UpgradedVehicles] SeaMothStorageDeluxe : Forced initialize");
                deluxeStorage.Init(seamoth);
                deluxeStorage.Initialized = true;
            }

            SetSeamothSpeed(seamoth, 1);
#if DEBUG
            Console.WriteLine($"[UpgradedVehicles] UpgradeSeaMoth : Finish");
#endif            
        }



        internal static void UpgradeVehicle(Vehicle vehicle)
        {
            var nameID = vehicle.GetComponent<PrefabIdentifier>().ClassId;

            if (nameID != SeaMothMk2.NameID) // TODO ExoSuitMk2
            {
                return; // This is a normal Seamoth. Do not upgrade.
            }

            // Minimum of +2 to engine eficiency
            int powerModuleCount = vehicle.modules.GetCount(TechType.VehiclePowerUpgradeModule);
            powerModuleCount += 2;
            DealDamageOnImpact component = vehicle.GetComponent<DealDamageOnImpact>();
            component.mirroredSelfDamageFraction = 0.5f * Mathf.Pow(0.5f, powerModuleCount);

            // Minium of +2 to armor plating
            int armorModuleCount = vehicle.modules.GetCount(TechType.VehicleArmorPlating);
            armorModuleCount += 2;
            float powerRating = 1f + 1f * armorModuleCount;
            vehicle.SetPrivateField("enginePowerRating", powerRating);

#if DEBUG
            Console.WriteLine($"[UpgradedVehicles] UpgradeVehicle : End");
#endif
        }

        internal static void SetSeamothSpeed(SeaMoth seamoth, int speedFactor)
        {
            seamoth.forwardForce = ForwardForces[speedFactor];
            seamoth.backwardForce = BackwardForces[speedFactor];
            seamoth.sidewardForce = SidewardForces[speedFactor];
        }
    }
}
