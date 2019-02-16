﻿namespace CustomCraft2SML.Interfaces
{
    using System.Collections.Generic;
    using CustomCraft2SML.Serialization.Components;

    public interface IModifiedRecipe : ITechTyped, ICustomCraft
    {
        short? AmountCrafted { get; }
        bool ForceUnlockAtStart { get; }

        IList<EmIngredient> Ingredients { get; }
        IList<string> LinkedItemIDs { get; }
        IList<string> Unlocks { get; }
    }
}