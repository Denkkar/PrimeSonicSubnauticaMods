﻿namespace CyclopsBioReactor.Management
{
    using CommonCyclopsUpgrades;
    using MoreCyclopsUpgrades.API;
    using MoreCyclopsUpgrades.API.Charging;
    using UnityEngine;

    internal class BioChargeHandler : ICyclopsCharger
    {
        internal const string ChargerName = "BioChrHldr";

        private const float BioReactorRateLimiter = 0.85f;
        private const float BatteryDrainRate = 0.01f * BioReactorRateLimiter;

        private BioAuxCyclopsManager manager;
        private BioAuxCyclopsManager Manager => manager ?? (manager = MCUServices.Find.AuxCyclopsManager<BioAuxCyclopsManager>(Cyclops, BioAuxCyclopsManager.ManagerName));

        public bool IsRenewable { get; } = false;

        public string Name { get; } = ChargerName;

        internal const int MaxBioReactors = 6;
        internal bool ProducingPower = false;

        private float totalBioCharge = 0f;
        private float totalBioCapacity = 0f;

        private readonly Atlas.Sprite sprite;

        public readonly SubRoot Cyclops;

        public BioChargeHandler(TechType cyBioBooster, SubRoot cyclops)
        {
            sprite = SpriteManager.Get(cyBioBooster);
            Cyclops = cyclops;
        }

        public Atlas.Sprite GetIndicatorSprite()
        {
            return sprite;
        }

        public string GetIndicatorText()
        {
            return NumberFormatter.FormatValue(totalBioCharge);
        }

        public Color GetIndicatorTextColor()
        {
            return NumberFormatter.GetNumberColor(totalBioCharge, totalBioCapacity, 0f);
        }

        public bool HasPowerIndicatorInfo()
        {
            return ProducingPower;
        }

        public float ProducePower(float requestedPower)
        {
            if (this.Manager.CyBioReactors.Count == 0)
            {
                ProducingPower = false;
                return 0f;
            }

            float tempBioCharge = 0f;
            float tempBioCapacity = 0f;
            float charge = 0f;

            int poweredReactors = 0;
            foreach (CyBioReactorMono reactor in this.Manager.CyBioReactors)
            {
                if (!reactor.HasPower)
                    continue;

                if (poweredReactors < MaxBioReactors)
                {
                    poweredReactors++;

                    charge += reactor.GetBatteryPower(BatteryDrainRate, requestedPower);

                    tempBioCharge += reactor.Charge;
                    tempBioCapacity = reactor.Capacity;
                }
            }

            ProducingPower = poweredReactors > 0;

            totalBioCharge = tempBioCharge;
            totalBioCapacity = tempBioCapacity;

            return charge;
        }

        public float TotalReservePower()
        {
            float totalPower = 0f;
            foreach (CyBioReactorMono reactor in this.Manager.CyBioReactors)
                totalPower += reactor.Charge;

            return totalPower;
        }
    }
}
