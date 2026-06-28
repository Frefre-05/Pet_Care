using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class PixieDecisionConfig
{
    public const float DefaultHungerLowThreshold = 35f;
    public const float DefaultHungerCriticalThreshold = 20f;
    public const float DefaultEnergyLowThreshold = 35f;
    public const float DefaultEnergyCriticalThreshold = 20f;
    public const float DefaultEnergyGateThreshold = 50f;
    public const float DefaultHealthLowThreshold = 35f;
    public const float DefaultHygieneLowThreshold = 35f;
    public const float DefaultHappinessLowThreshold = 35f;
    public const float DefaultHungerSlowThreshold = 25f;

    public const int DefaultKiwiPrice = 5;
    public const int DefaultPineapplePrice = 20;
    public const int DefaultHealthPotionPrice = 20;
    public const int DefaultEnergyPotionPrice = 20;

    [Header("Need thresholds")]
    public float HungerLowThreshold = DefaultHungerLowThreshold;
    public float HungerCriticalThreshold = DefaultHungerCriticalThreshold;
    public float EnergyLowThreshold = DefaultEnergyLowThreshold;
    public float EnergyCriticalThreshold = DefaultEnergyCriticalThreshold;
    public float EnergyGateThreshold = DefaultEnergyGateThreshold;
    public float HealthLowThreshold = DefaultHealthLowThreshold;
    public float HygieneLowThreshold = DefaultHygieneLowThreshold;
    public float HappinessLowThreshold = DefaultHappinessLowThreshold;
    public float HungerSlowThreshold = DefaultHungerSlowThreshold;

    [Header("Shop prices")]
    public int KiwiPrice = DefaultKiwiPrice;
    public int PineapplePrice = DefaultPineapplePrice;
    public int HealthPotionPrice = DefaultHealthPotionPrice;
    public int EnergyPotionPrice = DefaultEnergyPotionPrice;

    public static PixieDecisionConfig FromShop(ShopButtons shopButtons)
    {
        PixieDecisionConfig config = new PixieDecisionConfig();
        if (shopButtons != null)
        {
            config.KiwiPrice = shopButtons.smallFoodCost;
            config.PineapplePrice = shopButtons.bigFoodCost;
            config.HealthPotionPrice = shopButtons.healthPotionCost;
            config.EnergyPotionPrice = shopButtons.energyPotionCost;
        }
        return config;
    }
}

public static class PixieActionIds
{
    public const string BuyKiwi = "buy_kiwi";
    public const string BuyPineapple = "buy_pineapple";
    public const string BuyHealthPotion = "buy_health_potion";
    public const string BuyEnergyPotion = "buy_energy_potion";
    public const string Sleep = "sleep";
    public const string Bathe = "bathe";
    public const string DoChore = "do_chore";
    public const string PlayUnlockedLevel = "play_unlocked_level";
    public const string SaveApples = "save_apples";
}

public static class PixieDecisionEngine
{
    public static PixieDecisionResult Evaluate(PixieGameState state, PixieDecisionConfig config)
    {
        PixieDecisionResult result = new PixieDecisionResult();
        if (state == null)
        {
            result.recommendedAction = PixieActionIds.SaveApples;
            result.priorityStat = "unknown";
            result.shouldSaveApples = true;
            result.reason = "Game state is unavailable, so save Gold Coins until the pet state is loaded.";
            return result;
        }

        if (config == null)
            config = new PixieDecisionConfig();

        List<string> allowedActions = BuildAllowedActions(state, config);
        state.allowedActions = new List<string>(allowedActions);
        result.allowedActions = allowedActions;

        if (state.energy <= config.EnergyCriticalThreshold && state.levelsBlockedByLowEnergy)
        {
            result.priorityStat = "energy";
            if (allowedActions.Contains(PixieActionIds.Sleep))
            {
                result.recommendedAction = PixieActionIds.Sleep;
                result.reason = "Energy is critically low and levels are blocked, so sleeping is the fastest free recovery.";
                return result;
            }

            if (allowedActions.Contains(PixieActionIds.BuyEnergyPotion))
            {
                result.recommendedAction = PixieActionIds.BuyEnergyPotion;
                result.reason = "Energy is critically low and levels are blocked, so buy an energy potion if you cannot sleep right away.";
                return result;
            }

            result.recommendedAction = allowedActions.Contains(PixieActionIds.DoChore) ? PixieActionIds.DoChore : PixieActionIds.SaveApples;
            result.shouldSaveApples = true;
            result.reason = "Energy is critically low and levels are blocked, but you cannot recover it immediately, so earn or save Gold Coins first.";
            return result;
        }

        if (state.hunger <= config.HungerCriticalThreshold)
        {
            result.priorityStat = "hunger";
            if (allowedActions.Contains(PixieActionIds.BuyKiwi))
            {
                result.recommendedAction = PixieActionIds.BuyKiwi;
                result.reason = "Hunger is critically low and kiwi is the cheapest food you can afford right now.";
                return result;
            }

            if (allowedActions.Contains(PixieActionIds.BuyPineapple))
            {
                result.recommendedAction = PixieActionIds.BuyPineapple;
                result.reason = "Hunger is critically low and pineapple is affordable, so it restores hunger immediately.";
                return result;
            }

            result.recommendedAction = allowedActions.Contains(PixieActionIds.DoChore) ? PixieActionIds.DoChore : PixieActionIds.SaveApples;
            result.shouldSaveApples = true;
            result.reason = "Hunger is critically low, but food is not affordable yet, so earn Gold Coins before spending.";
            return result;
        }

        if (state.health <= config.HealthLowThreshold)
        {
            result.priorityStat = "health";
            if (allowedActions.Contains(PixieActionIds.BuyHealthPotion))
            {
                result.recommendedAction = PixieActionIds.BuyHealthPotion;
                result.reason = "Health is low and a health potion is affordable, so restoring health is the safest next step.";
                return result;
            }

            result.recommendedAction = allowedActions.Contains(PixieActionIds.DoChore) ? PixieActionIds.DoChore : PixieActionIds.SaveApples;
            result.shouldSaveApples = true;
            result.reason = "Health is low, but a health potion is not affordable yet, so save Gold Coins or earn more first.";
            return result;
        }

        if (state.hunger <= config.HungerLowThreshold)
        {
            result.priorityStat = "hunger";
            if (allowedActions.Contains(PixieActionIds.BuyKiwi))
            {
                result.recommendedAction = PixieActionIds.BuyKiwi;
                result.reason = "Hunger is low and kiwi solves it without overspending.";
                return result;
            }

            if (allowedActions.Contains(PixieActionIds.BuyPineapple))
            {
                result.recommendedAction = PixieActionIds.BuyPineapple;
                result.reason = "Hunger is low and pineapple is affordable, so it can fully refill hunger.";
                return result;
            }

            result.recommendedAction = allowedActions.Contains(PixieActionIds.DoChore) ? PixieActionIds.DoChore : PixieActionIds.SaveApples;
            result.shouldSaveApples = true;
            result.reason = "Hunger is low, but food is not affordable yet, so earn Gold Coins first.";
            return result;
        }

        if (state.energy <= config.EnergyLowThreshold)
        {
            result.priorityStat = "energy";
            result.recommendedAction = allowedActions.Contains(PixieActionIds.Sleep) ? PixieActionIds.Sleep : allowedActions.Contains(PixieActionIds.BuyEnergyPotion) ? PixieActionIds.BuyEnergyPotion : PixieActionIds.SaveApples;
            result.reason = result.recommendedAction == PixieActionIds.Sleep
                ? "Energy is low, and sleeping restores it for free."
                : result.recommendedAction == PixieActionIds.BuyEnergyPotion
                    ? "Energy is low, and an energy potion is the available recovery option."
                    : "Energy is low, so avoid spending until you can recover it safely.";
            result.shouldSaveApples = result.recommendedAction == PixieActionIds.SaveApples;
            return result;
        }

        if (state.hygiene <= config.HygieneLowThreshold)
        {
            result.priorityStat = "hygiene";
            result.recommendedAction = PixieActionIds.Bathe;
            result.reason = "Hygiene is low, and bathing fixes it without spending Gold Coins.";
            return result;
        }

        if (state.happiness <= config.HappinessLowThreshold)
        {
            result.priorityStat = "happiness";
            result.recommendedAction = allowedActions.Contains(PixieActionIds.DoChore) ? PixieActionIds.DoChore : PixieActionIds.PlayUnlockedLevel;
            result.reason = result.recommendedAction == PixieActionIds.DoChore
                ? "Happiness is low, and endpoints give 0 Happiness, so do a chore instead of expecting a level endpoint to restore it."
                : "Happiness is low, but endpoints give 0 Happiness, so use the level only for progression.";
            return result;
        }

        result.priorityStat = "progression";
        if (allowedActions.Contains(PixieActionIds.PlayUnlockedLevel) && !state.levelsBlockedByLowEnergy)
        {
            result.recommendedAction = PixieActionIds.PlayUnlockedLevel;
            result.reason = "Your pet is stable, so the best move is to progress through the next unlocked level.";
            return result;
        }

        result.recommendedAction = PixieActionIds.SaveApples;
        result.shouldSaveApples = true;
        result.reason = "Your pet is stable right now, so save Gold Coins for the next urgent need.";
        return result;
    }

    public static string BuildSummary(PixieDecisionResult result)
    {
        if (result == null)
            return "No recommendation available.";

        StringBuilder sb = new StringBuilder();
        sb.Append(result.recommendedAction);
        if (!string.IsNullOrWhiteSpace(result.reason))
            sb.Append(": ").Append(result.reason);
        return sb.ToString();
    }

    static List<string> BuildAllowedActions(PixieGameState state, PixieDecisionConfig config)
    {
        List<string> actions = new List<string>
        {
            PixieActionIds.Sleep,
            PixieActionIds.Bathe,
            PixieActionIds.DoChore,
            PixieActionIds.SaveApples
        };

        if (!state.levelsBlockedByLowEnergy)
            actions.Add(PixieActionIds.PlayUnlockedLevel);

        if (state.apples >= config.KiwiPrice)
            actions.Add(PixieActionIds.BuyKiwi);
        if (state.apples >= config.PineapplePrice)
            actions.Add(PixieActionIds.BuyPineapple);
        if (state.apples >= config.HealthPotionPrice)
            actions.Add(PixieActionIds.BuyHealthPotion);
        if (state.apples >= config.EnergyPotionPrice)
            actions.Add(PixieActionIds.BuyEnergyPotion);

        return actions;
    }
}
