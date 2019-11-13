﻿namespace CustomBatteries.Items
{
    using System.Collections.Generic;
    using SMLHelper.V2.Crafting;

    internal class CustomBattery : CbCore
    {
        public CustomBattery(string classId) : base(classId)
        {
        }

        protected override TechType PrefabType => TechType.Battery;
        protected override EquipmentType ChargerType => EquipmentType.BatteryCharger;

        public void CreateBlueprintData(params TechType[] parts)
        {
            var partsList = new List<Ingredient>();

            CreateIngredients(parts, partsList);

            if (partsList.Count == 0)
                partsList.Add(new Ingredient(TechType.Titanium, 1));

            var batteryBlueprint = new TechData
            {
                craftAmount = 1,
                Ingredients = partsList
            };

            this.BlueprintRecipe = batteryBlueprint;
        }

        protected override void AddToList()
        {
            BatteryTechTypes.Add(this.TechType);
            HasBatteries = true;
        }
    }
}
