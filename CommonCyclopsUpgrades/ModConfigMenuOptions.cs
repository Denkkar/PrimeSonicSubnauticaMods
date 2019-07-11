﻿namespace CommonCyclopsUpgrades
{
    using System.Collections.Generic;
    using CommonCyclopsUpgrades.Options;
    using SMLHelper.V2.Handlers;
    using SMLHelper.V2.Options;

    internal class ModConfigMenuOptions : ModOptions
    {
        private readonly IEnumerable<ConfigOption> configOptions;

        public ModConfigMenuOptions(IEnumerable<ConfigOption> options) : base("MoreCyclopsUpgrades Config Options")
        {
            configOptions = options;
        }

        internal void Register()
        {
            SetUpEvents();
            OptionsPanelHandler.RegisterModOptions(this);
        }

        private void SetUpEvents()
        {
            foreach (ConfigOption item in configOptions)
            {
                switch (item.OptionType)
                {
                    case OptionTypes.Slider when item is SliderOption slider:
                        base.SliderChanged += (object sender, SliderChangedEventArgs e) =>
                        {
                            if (e.Id == slider.Id)
                                slider?.ValueChanged(e.Value);
                        };
                        break;
                    case OptionTypes.Choice when item is ChoiceOption choice:
                        base.ChoiceChanged += (object sender, ChoiceChangedEventArgs e) =>
                        {
                            if (e.Id == choice.Id)
                                choice?.ChoiceChanged(e.Index);
                        };
                        break;
                    case OptionTypes.Toggle when item is ToggleOption toggle:
                        base.ToggleChanged += (object sender, ToggleChangedEventArgs e) =>
                        {
                            if (e.Id == toggle.Id)
                                toggle?.OptionToggled(e.Value);
                        };
                        break;
                }
            }
        }

        public override void BuildModOptions()
        {
            foreach (ConfigOption item in configOptions)
            {
                switch (item.OptionType)
                {
                    case OptionTypes.Slider when item is SliderOption slider:
                        base.AddSliderOption(slider.Id, slider.Label, slider.MinValue, slider.MaxValue, slider.Value);
                        break;
                    case OptionTypes.Choice when item is ChoiceOption choice:
                        base.AddChoiceOption(choice.Id, choice.Label, choice.Choices, choice.Index);
                        break;
                    case OptionTypes.Toggle when item is ToggleOption toggle:
                        base.AddToggleOption(toggle.Id, toggle.Label, toggle.State);
                        break;
                }
            }
        }
    }
}