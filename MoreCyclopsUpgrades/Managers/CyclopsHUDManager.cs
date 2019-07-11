﻿namespace MoreCyclopsUpgrades.Managers
{
    using System.Collections.Generic;
    using Common;
    using CommonCyclopsUpgrades;
    using MoreCyclopsUpgrades.API.Charging;
    using MoreCyclopsUpgrades.Config;
    using MoreCyclopsUpgrades.Config.ChoiceEnums;
    using UnityEngine;
    using UnityEngine.UI;

    internal class CyclopsHUDManager
    {
        private class Indicator
        {
            internal uGUI_Icon Icon;
            internal Text Text;

            internal Indicator(uGUI_Icon icon, Text text)
            {
                Icon = icon;
                Text = text;
                SetEnabled(false);
            }

            internal void SetEnabled(bool value)
            {
                Icon.enabled = value;
                Text.enabled = value;
            }
        }

        private Indicator[] HelmIndicatorsOdd;
        private Indicator[] HelmIndicatorsEven;

        private Indicator[] HealthBarIndicatorsOdd;
        private Indicator[] HealthBarIndicatorsEven;
        private bool lastKnownTextVisibility = false;
        private bool powerIconTextVisibility = false;

        private readonly SubRoot Cyclops;

        private IChargeManager chargeManager;
        private IChargeManager ChargeManager => chargeManager ?? (chargeManager = CyclopsManager.GetManager(Cyclops).Charge);

        private bool powerIconsInitialized = false;

        private CyclopsHolographicHUD holographicHUD;
        private readonly IModConfig settings = ModConfig.Main;

        private HelmEnergyDisplay lastDisplay = HelmEnergyDisplay.PowerCellPercentage;

        internal CyclopsHUDManager(SubRoot cyclops)
        {
            Cyclops = cyclops;
        }

        internal void UpdateTextVisibility()
        {
            powerIconTextVisibility =
                Player.main.currentSub == Cyclops &&
                holographicHUD != null &&
                Mathf.Abs(Vector3.Distance(holographicHUD.transform.position, Player.main.transform.position)) <= 4f;

            if (lastKnownTextVisibility != powerIconTextVisibility)
            {
                UpdatePowerIcons(this.ChargeManager.Chargers);
                lastKnownTextVisibility = powerIconTextVisibility;
            }
        }

        internal void UpdateHelmHUD(CyclopsHelmHUDManager cyclopsHelmHUD, int lastPowerInt)
        {
            if (!cyclopsHelmHUD.LOD.IsFull() || Player.main.currentSub != Cyclops)
            {
                return; // Same early exit
            }

            if (!powerIconsInitialized)
                AddPowerIcons(cyclopsHelmHUD, this.ChargeManager.Chargers.Count);
            else
                UpdatePowerIcons(this.ChargeManager.Chargers);

            if (lastPowerInt < 0f)
                return;

            PowerRelay powerRelay = Cyclops.powerRelay;

            switch (lastDisplay = settings.EnergyDisplay)
            {
                case HelmEnergyDisplay.PowerCellAmount:
                    cyclopsHelmHUD.powerText.text = NumberFormatter.FormatValue(powerRelay.GetPower());
                    break;
                case HelmEnergyDisplay.PercentageOverPowerCells:
                    // Max out at 999 because only 4 characters fit on the display
                    float percentOver = (powerRelay.GetPower() + this.ChargeManager.GetTotalReservePower()) / powerRelay.GetMaxPower();
                    cyclopsHelmHUD.powerText.text = $"{NumberFormatter.FormatValue(Mathf.Min(percentOver * 100f, 999f))}%";
                    break;
                case HelmEnergyDisplay.CombinedAmount:
                    cyclopsHelmHUD.powerText.text = NumberFormatter.FormatValue(powerRelay.GetPower() + this.ChargeManager.GetTotalReservePower());
                    break;
                default: // HelmEnergyDisplay.PowerCellPercentage:
                    float percent = powerRelay.GetPower() / powerRelay.GetMaxPower();
                    int percentInt = Mathf.CeilToInt(percent * 100f);
                    cyclopsHelmHUD.powerText.text = $"{percentInt}%";
                    break;
            }
        }

        /// <summary>
        /// Updates the console HUD using data from all equipment modules across all upgrade consoles.
        /// </summary>
        /// <param name="hudManager">The console HUD manager.</param>
        internal void UpdateConsoleHUD(CyclopsUpgradeConsoleHUDManager hudManager)
        {
            if (!Cyclops.LOD.IsFull() || Player.main.currentSub != Cyclops)
            {
                return; // Same early exit
            }

            hudManager.healthCur.text = IntStringCache.GetStringForInt(Mathf.FloorToInt(hudManager.liveMixin.health));
            int maxHealth = Mathf.CeilToInt(hudManager.liveMixin.health);
            if (hudManager.lastHealthMaxDisplayed != maxHealth)
            {
                hudManager.healthMax.text = "/" + IntStringCache.GetStringForInt(maxHealth);
                hudManager.lastHealthMaxDisplayed = maxHealth;
            }

            int currentReservePower = this.ChargeManager.GetTotalReservePower();
            float currentBatteryPower = Cyclops.powerRelay.GetPower();

            int TotalPowerUnits = Mathf.CeilToInt(currentBatteryPower + currentReservePower);
            float normalMaxPower = Cyclops.powerRelay.GetMaxPower();
            int normalMaxPowerInt = Mathf.CeilToInt(normalMaxPower);

            hudManager.energyCur.color = NumberFormatter.GetNumberColor(currentBatteryPower, normalMaxPower, 0f);
            hudManager.energyCur.text = NumberFormatter.FormatValue(TotalPowerUnits);

            if (hudManager.lastMaxSubPowerDisplayed != normalMaxPowerInt)
            {
                hudManager.energyMax.text = "/" + NumberFormatter.FormatValue(normalMaxPowerInt);
                hudManager.lastMaxSubPowerDisplayed = normalMaxPowerInt;
            }

            settings.UpdateCyclopsMaxPower(normalMaxPower);

            UpdateTextVisibility();
        }

        private void AddPowerIcons(CyclopsHelmHUDManager cyclopsHelmHUD, int totalIcons)
        {
            holographicHUD = cyclopsHelmHUD.subRoot.GetComponentInChildren<CyclopsHolographicHUD>();

            Canvas pilotingCanvas = cyclopsHelmHUD.powerText.canvas;
            Canvas holoCanvas = holographicHUD.healthBar.canvas;

            /* --- 3-1-2 --- */
            /* ---- 1-2 ---- */

            if (totalIcons % 2 != 0)
                totalIcons--;

            HelmIndicatorsOdd = new Indicator[totalIcons + 1];
            HelmIndicatorsEven = new Indicator[totalIcons];
            HealthBarIndicatorsOdd = new Indicator[totalIcons + 1];
            HealthBarIndicatorsEven = new Indicator[totalIcons];

            const float helmspacing = 140;
            const float helmzoffset = 0.05f;
            const float helmyoffset = -225;
            const float helmscale = 1.40f;

            const float healthbarxoffset = 100;
            const float healthbarzoffset = 0.05f;
            const float healthbaryoffset = -300;
            const float healthbarscale = 0.70f;

            HelmIndicatorsOdd[0] = CreatePowerIndicatorIcon(pilotingCanvas, 0, helmyoffset, helmzoffset, helmscale);
            HealthBarIndicatorsOdd[0] = CreatePowerIndicatorIcon(holoCanvas, healthbarxoffset + 0, healthbaryoffset, healthbarzoffset, healthbarscale);

            int index = 0;
            float spacing = helmspacing;
            float spacingSmall = helmspacing / 2;
            do
            {
                HelmIndicatorsOdd[index + 1] = CreatePowerIndicatorIcon(pilotingCanvas, spacing, helmyoffset, helmzoffset, helmscale);
                HelmIndicatorsOdd[index + 2] = CreatePowerIndicatorIcon(pilotingCanvas, -spacing, helmyoffset, helmzoffset, helmscale);

                HelmIndicatorsEven[index] = CreatePowerIndicatorIcon(pilotingCanvas, -spacing / 2, helmyoffset, helmzoffset, helmscale);
                HelmIndicatorsEven[index + 1] = CreatePowerIndicatorIcon(pilotingCanvas, spacing / 2, helmyoffset, helmzoffset, helmscale);

                HealthBarIndicatorsOdd[index + 1] = CreatePowerIndicatorIcon(holoCanvas, healthbarxoffset + spacingSmall, healthbaryoffset, healthbarzoffset, healthbarscale);
                HealthBarIndicatorsOdd[index + 2] = CreatePowerIndicatorIcon(holoCanvas, healthbarxoffset + -spacingSmall, healthbaryoffset, healthbarzoffset, healthbarscale);

                HealthBarIndicatorsEven[index] = CreatePowerIndicatorIcon(holoCanvas, healthbarxoffset + -spacingSmall / 2, healthbaryoffset, healthbarzoffset, healthbarscale);
                HealthBarIndicatorsEven[index + 1] = CreatePowerIndicatorIcon(holoCanvas, healthbarxoffset + spacingSmall / 2, healthbaryoffset, healthbarzoffset, healthbarscale);

                index += 2;
                spacing += helmspacing;
                spacingSmall += spacingSmall;

            } while (totalIcons > index);

            powerIconsInitialized = true;

            QuickLogger.Debug("Linked CyclopsHUDManager to HelmHUD");
        }

        private static Indicator CreatePowerIndicatorIcon(Canvas canvas, float xoffset, float yoffset, float zoffset, float scale)
        {
            var iconGo = new GameObject("IconGo", typeof(RectTransform));
            iconGo.transform.SetParent(canvas.transform, false);
            iconGo.transform.localPosition = new Vector3(xoffset, yoffset, zoffset);
            iconGo.transform.localScale = new Vector3(scale, scale, scale);

            uGUI_Icon icon = iconGo.AddComponent<uGUI_Icon>();

            var arial = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            var textGO = new GameObject("TextGo");

            textGO.transform.SetParent(iconGo.transform, false);

            Text text = textGO.AddComponent<Text>();
            text.font = arial;
            text.material = arial.material;
            text.text = "??";
            text.fontSize = 22;
            text.alignment = TextAnchor.LowerCenter;
            text.color = Color.white;

            Outline outline = textGO.AddComponent<Outline>();
            outline.effectColor = Color.black;

            RectTransform rectTransform = text.GetComponent<RectTransform>();
            rectTransform.localScale = Vector3.one;
            rectTransform.anchoredPosition3D = Vector3.zero;
            rectTransform.anchoredPosition += new Vector2(0, -15f);
            return new Indicator(icon, text);
        }

        private void UpdatePowerIcons<C>(IEnumerable<C> cyclopsChargers) where C : ICyclopsCharger
        {
            if (!powerIconsInitialized)
                return;

            HidePowerIcons();

            if (settings.HidePowerIcons)
                return;

            bool isEven = true;
            foreach (ICyclopsCharger charger in cyclopsChargers)
            {
                if (charger.ShowStatusIcon)
                    isEven = !isEven;
            }

            Indicator[] helmRow = isEven ? HelmIndicatorsEven : HelmIndicatorsOdd;
            Indicator[] healthBarRow = isEven ? HealthBarIndicatorsEven : HealthBarIndicatorsOdd;
            int index = 0;

            bool showIconsOnHoloDisplay = settings.ShowIconsOnHoloDisplay;
            bool showIconsWhilePiloting = settings.ShowIconsWhilePiloting;
            foreach (ICyclopsCharger charger in cyclopsChargers)
            {
                if (!charger.ShowStatusIcon)
                    continue;

                Indicator helmIcon = helmRow[index];
                Indicator hpIcon = healthBarRow[index++];

                hpIcon.SetEnabled(showIconsOnHoloDisplay);
                helmIcon.SetEnabled(showIconsWhilePiloting);

                hpIcon.Icon.sprite = helmIcon.Icon.sprite = charger.StatusSprite();

                hpIcon.Text.enabled = powerIconTextVisibility;
                hpIcon.Text.text = helmIcon.Text.text = charger.StatusText();
                hpIcon.Text.color = helmIcon.Text.color = charger.StatusTextColor();
            }
        }

        private void HidePowerIcons()
        {
            foreach (Indicator indicator in HelmIndicatorsOdd)
                indicator.SetEnabled(false);

            foreach (Indicator indicator in HelmIndicatorsEven)
                indicator.SetEnabled(false);

            foreach (Indicator indicator in HealthBarIndicatorsOdd)
                indicator.SetEnabled(false);

            foreach (Indicator indicator in HealthBarIndicatorsEven)
                indicator.SetEnabled(false);
        }
    }
}
