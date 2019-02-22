﻿namespace CustomCraft2SML.Serialization.Entries
{
    using System.Collections.Generic;
    using Common;
    using Common.EasyMarkup;
    using CustomCraft2SML.Interfaces;
    using CustomCraft2SML.PublicAPI;
    using CustomCraft2SML.Serialization.Components;
    using CustomCraft2SML.Serialization.Lists;
    using SMLHelper.V2.Handlers;

    internal class MovedRecipe : EmTechTyped, IMovedRecipe
    {
        private const string OldPathKey = "OldPath";
        private const string NewPathKey = "NewPath";
        private const string HiddenKey = "Hidden";
        private const string CopyKey = "CopyToNewPath";

        public const string TypeName = "MovedRecipe";

        internal static readonly string[] TutorialText = new[]
        {
           $"{MovedRecipeList.ListKey}: Further customize the crafting tree to your liking. Move a crafting node or get rid of it.",
           $"    {OldPathKey}: First locate the crafting node you want to change.",
           $"        This node must always be present.",
           $"        This cannot be used to access paths in modded or custom fabricators.",
           $"    {NewPathKey}: If you want to move or copy the recipe to a new location, set the path here. It could even be a different (non-custom) crafting tree.",
           $"        This node is optional if {HiddenKey} is set to 'YES'.",
           $"        This node must be present if {CopyKey} is set to 'YES'.",
           $"        if neither {CopyKey} or {HiddenKey} are present, then the recipe will be removed from the {OldPathKey} and added to the {NewPathKey}.",
           $"    {CopyKey}: If you want, you can copy the recipe to the new path without removing the original by setting this to 'YES'.",
           $"        This node is optional and will default to 'NO' when not present.",
           $"        {CopyKey} cannot be set to 'YES' if {HiddenKey} is also set to 'YES'.",
           $"    {HiddenKey}: Or you can set this property to 'YES' to simply remove the crafting node instead.",
           $"        This node is optional and will default to 'NO' when not present.",
           $"        {HiddenKey} cannot be set to 'YES' if {CopyKey} is also set to 'YES'.",
        };

        private readonly EmProperty<string> oldPath;
        private readonly EmProperty<string> newPath;
        private readonly EmYesNo hidden;
        private readonly EmYesNo copyToNewPath;

        protected static List<EmProperty> MovedRecipeProperties => new List<EmProperty>(TechTypedProperties)
        {
            new EmProperty<string>(OldPathKey),
            new EmProperty<string>(NewPathKey),
            new EmYesNo(HiddenKey, false){ Optional = true },
            new EmYesNo(CopyKey, false){ Optional = true }
        };

        public OriginFile Origin { get; set; }

        public MovedRecipe() : this(TypeName, MovedRecipeProperties)
        {
        }

        protected MovedRecipe(string key, ICollection<EmProperty> definitions) : base(key, definitions)
        {
            oldPath = (EmProperty<string>)Properties[OldPathKey];
            newPath = (EmProperty<string>)Properties[NewPathKey];
            hidden = (EmYesNo)Properties[HiddenKey];
            copyToNewPath = (EmYesNo)Properties[CopyKey];
        }

        public string OldPath
        {
            get => oldPath.Value;
            set => oldPath.Value = value;
        }

        public string NewPath
        {
            get => newPath.Value;
            set => newPath.Value = value;
        }

        public bool Hidden
        {
            get => hidden.Value;
            set => hidden.Value = value;
        }

        public bool CopyToNewPath
        {
            get => copyToNewPath.Value;
            set => copyToNewPath.Value = value;
        }

        public string ID => this.ItemID;

        internal override EmProperty Copy() => new MovedRecipe(this.Key, this.CopyDefinitions);

        public override bool PassesPreValidation() => base.PassesPreValidation() & IsValidState();

        private bool IsValidState()
        {
            if (string.IsNullOrEmpty(this.OldPath))
            {
                QuickLogger.Warning($"{OldPathKey} missing in {this.Key} for '{this.ItemID}' from {this.Origin}");
                return false;
            }

            if (this.CopyToNewPath && this.Hidden)
            {
                QuickLogger.Warning($"Invalid request in {this.Key} for '{this.ItemID}' from {this.Origin}. {CopyKey} and {HiddenKey} cannot both be set to 'YES'");
                return false;
            }

            if (string.IsNullOrEmpty(this.NewPath) && (this.CopyToNewPath || !this.Hidden))
            {
                QuickLogger.Warning($"{NewPathKey} value missing in {this.Key} for '{this.ItemID}' from {this.Origin}");
                return false;
            }

            return true;
        }

        public bool SendToSMLHelper()
        {
            var oldPath = new CraftingPath(this.OldPath, this.ItemID);

            CraftTreeHandler.RemoveNode(oldPath.Scheme, oldPath.CraftNodeSteps);
            QuickLogger.Message($"Removed crafting node at '{this.ItemID}' - Entry from {this.Origin}");
            if (this.Hidden)
            {
                return true;
            }

            HandleCraftTreeAddition();

            return true;
        }

        protected virtual void HandleCraftTreeAddition()
        {
            var newPath = new CraftingPath(this.NewPath, this.ItemID);

            AddCraftNode(newPath, this.TechType);
        }
    }
}
