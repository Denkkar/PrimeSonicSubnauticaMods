﻿namespace MoreCyclopsUpgrades
{
    using System.Collections.Generic;
    using System.IO;
    using Common.EasyMarkup;
    using SMLHelper.V2.Utility;

    public class AuxUpgradeConsoleSaveData : EmPropertyCollection
    {
        private readonly string ID;

        private readonly EmModuleSaveData Module1;
        private readonly EmModuleSaveData Module2;
        private readonly EmModuleSaveData Module3;
        private readonly EmModuleSaveData Module4;
        private readonly EmModuleSaveData Module5;
        private readonly EmModuleSaveData Module6;

        private static ICollection<EmProperty> AucUpConsoleDefs => new List<EmProperty>(6)
        {
            new EmModuleSaveData("M1"),
            new EmModuleSaveData("M2"),
            new EmModuleSaveData("M3"),
            new EmModuleSaveData("M4"),
            new EmModuleSaveData("M5"),
            new EmModuleSaveData("M6"),
        };        

        public EmModuleSaveData GetModuleInSlot(string slot)
        {
            switch (slot)
            {
                case "Module1": return Module1;
                case "Module2": return Module2;
                case "Module3": return Module3;
                case "Module4": return Module4;
                case "Module5": return Module5;
                case "Module6": return Module6;
                default: return null;
            }
        }

        public AuxUpgradeConsoleSaveData(string preFabID) : base("AuxUpgradeConsoleSaveData", AucUpConsoleDefs)
        {
            ID = preFabID;

            Module1 = (EmModuleSaveData)base.Properties["M1"];
            Module2 = (EmModuleSaveData)base.Properties["M2"];
            Module3 = (EmModuleSaveData)base.Properties["M3"];
            Module4 = (EmModuleSaveData)base.Properties["M4"];
            Module5 = (EmModuleSaveData)base.Properties["M5"];
            Module6 = (EmModuleSaveData)base.Properties["M6"];
        }

        private string SaveDirectory => Path.Combine(SaveUtils.GetCurrentSaveDataDir(), "AuxUpgradeConsole");
        private string SaveFile => Path.Combine(SaveDirectory, ID + ".txt");

        public void Save()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }

            File.WriteAllLines(SaveFile, new[]
            {
                "# This save data was generated by EasyMarkup #",
                this.ToString(),
            });
        }

        public bool Load()
        {
            string saveDir = SaveFile;
            if (!File.Exists(saveDir))
            {
                Save();
                return false;
            }

            string serializedData = File.ReadAllText(saveDir);

            bool validData = this.FromString(serializedData);

            if (!validData)
            {
                Save();
                return false;
            }

            return true;
        }

        internal override EmProperty Copy() => new AuxUpgradeConsoleSaveData(ID);


    }
}

