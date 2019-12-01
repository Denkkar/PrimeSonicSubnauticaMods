﻿namespace CustomBatteries.Items
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Common;
    using SMLHelper.V2.Assets;
    using SMLHelper.V2.Crafting;
    using SMLHelper.V2.Handlers;
    using SMLHelper.V2.Utility;
    using UnityEngine;

    internal abstract class CbCore : ModPrefab
    {
        private const string BatteryCraftTab = "BatteryTab";
        private const string PowCellCraftTab = "PowCellTab";
        private const string ElecCraftTab = "Electronics";
        private const string ResCraftTab = "Resources";
        protected static readonly string[] BatteryCraftPath = new[] { ResCraftTab, ElecCraftTab, BatteryCraftTab };
        protected static readonly string[] PowCellCraftPath = new[] { ResCraftTab, ElecCraftTab, PowCellCraftTab };

        private static bool CraftingTabsPatched = false;

        public static string ExecutingFolder { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static List<TechType> BatteryTechTypes { get; } = new List<TechType>();
        public static bool HasBatteries { get; protected set; } = false;
        public static TechType SampleBattery => BatteryTechTypes[0];

        public static List<TechType> PowerCellTechTypes { get; } = new List<TechType>();
        public static bool HasPowerCells { get; protected set; } = false;
        public static TechType SamplePowerCell => PowerCellTechTypes[0];

        protected abstract TechType PrefabType { get; } // Should only ever be Battery or PowerCell
        protected abstract EquipmentType ChargerType { get; } // Should only ever be BatteryCharger or PowerCellCharger

        public TechType RequiredForUnlock { get; } = TechType.None;

        public TechData BlueprintRecipe { get; set; }

        public float PowerCapacity { get; set; }

        public string FriendlyName { get; set; }

        public string Description { get; set; }

        public string IconFileName { get; set; }

        public string PluginPackName { get; set; }

        protected CbCore(string classId)
            : base(classId, $"{classId}PreFab", TechType.None)
        {
        }

        public override GameObject GetGameObject()
        {
            GameObject prefab = CraftData.GetPrefabForTechType(this.PrefabType);
            var obj = GameObject.Instantiate(prefab);

            Battery battery = obj.GetComponent<Battery>();
            battery._capacity = this.PowerCapacity;
            battery.name = $"{this.ClassID}BatteryCell";

            return obj;
        }

        protected static void CreateIngredients(IEnumerable<TechType> parts, List<Ingredient> partsList)
        {
            if (parts == null)
                return;

            foreach (TechType part in parts)
            {
                Ingredient priorIngredient = partsList.Find(i => i.techType == part);

                if (priorIngredient != null)
                    priorIngredient.amount++;
                else
                    partsList.Add(new Ingredient(part, 1));
            }
        }

        protected abstract void AddToList();

        protected abstract string[] StepsToFabricatorTab { get; }

        public void Patch()
        {
            this.TechType = TechTypeHandler.AddTechType(this.ClassID, this.FriendlyName, this.Description, false);

            SpriteHandler.RegisterSprite(this.TechType, IOUtilities.Combine(ExecutingFolder, this.PluginPackName, this.IconFileName));

            CraftDataHandler.SetTechData(this.TechType, this.BlueprintRecipe);

            CraftDataHandler.AddToGroup(TechGroup.Resources, TechCategory.Electronics, this.TechType);

            CraftDataHandler.SetEquipmentType(this.TechType, this.ChargerType);

            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, this.TechType, this.StepsToFabricatorTab);

            PrefabHandler.RegisterPrefab(this);

            AddToList();
        }

        internal static void PatchCraftingTabs()
        {
            if (CraftingTabsPatched)
                return; // Just a safety

            QuickLogger.Info("Patching crafting tabs");

            // Remove original crafting nodes
            CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, ResCraftTab, ElecCraftTab, TechType.Battery.ToString());
            CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, ResCraftTab, ElecCraftTab, TechType.PrecursorIonBattery.ToString());
            CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, ResCraftTab, ElecCraftTab, TechType.PowerCell.ToString());
            CraftTreeHandler.RemoveNode(CraftTree.Type.Fabricator, ResCraftTab, ElecCraftTab, TechType.PrecursorIonPowerCell.ToString());

            // Add a new set of tab nodes for batteries and power cells
            CraftTreeHandler.AddTabNode(CraftTree.Type.Fabricator, BatteryCraftTab, "Batteries", SpriteManager.Get(TechType.Battery), ResCraftTab, ElecCraftTab);
            CraftTreeHandler.AddTabNode(CraftTree.Type.Fabricator, PowCellCraftTab, "Power Cells", SpriteManager.Get(TechType.PowerCell), ResCraftTab, ElecCraftTab);

            // Move the original batteries and power cells into these new tabs
            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.Battery, BatteryCraftPath);
            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PrecursorIonBattery, BatteryCraftPath);
            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PowerCell, PowCellCraftPath);
            CraftTreeHandler.AddCraftingNode(CraftTree.Type.Fabricator, TechType.PrecursorIonPowerCell, PowCellCraftPath);

            CraftingTabsPatched = true;
        }
    }
}
