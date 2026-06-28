using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class PixiePromptBuilder
{
    public static string BuildSystemPrompt()
    {
        return
            "You are Pixie AI from the game Pixel Care.\n" +
            "You must only recommend actions from the allowed_actions list.\n" +
            "Never invent prices, items, or rules.\n" +
            "Prioritize survival and progression using these rules:\n" +
            "1. If energy is critically low and levels are blocked, recommend sleep or energy recovery first.\n" +
            "2. If hunger is low, recommend affordable food.\n" +
            "3. If health is low, recommend health potion only if affordable and necessary.\n" +
            "4. If no useful purchase is affordable, recommend saving Gold Coins or doing chores.\n" +
            "Currency name shown to the player: Gold Coins. 1 Gold Coin = $1.\n" +
            "Return valid JSON only with keys: recommended_action, reason, priority_stat, should_save_apples, message.";
    }

    public static string BuildUserPrompt(PixieGameState state, PixieDecisionConfig config, PixieDecisionResult decision)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("game_state:");
        sb.AppendLine("{");
        sb.AppendLine("  \"gold_coins\": " + state.apples + ",");
        sb.AppendLine("  \"health\": " + FormatFloat(state.health) + ",");
        sb.AppendLine("  \"hunger\": " + FormatFloat(state.hunger) + ",");
        sb.AppendLine("  \"energy\": " + FormatFloat(state.energy) + ",");
        sb.AppendLine("  \"hygiene\": " + FormatFloat(state.hygiene) + ",");
        sb.AppendLine("  \"happiness\": " + FormatFloat(state.happiness) + ",");
        sb.AppendLine("  \"max_unlocked_level\": " + state.maxUnlockedLevel + ",");
        sb.AppendLine("  \"levels_blocked_due_to_low_energy\": " + ToJsonBool(state.levelsBlockedByLowEnergy) + ",");
        sb.AppendLine("  \"hunger_slows_pet\": " + ToJsonBool(state.hungerSlowsPet));
        sb.AppendLine("}");
        sb.AppendLine("allowed_actions: [" + string.Join(", ", QuoteEach(state.allowedActions)) + "]");
        sb.AppendLine("shop_prices:");
        sb.AppendLine("{");
        sb.AppendLine("  \"kiwi\": " + config.KiwiPrice + ",");
        sb.AppendLine("  \"pineapple\": " + config.PineapplePrice + ",");
        sb.AppendLine("  \"health_potion\": " + config.HealthPotionPrice + ",");
        sb.AppendLine("  \"energy_potion\": " + config.EnergyPotionPrice);
        sb.AppendLine("}");
        sb.AppendLine("rule_summary:");
        sb.AppendLine("- Hunger can only be restored through shop food.");
        sb.AppendLine("- Health can be restored using a health potion.");
        sb.AppendLine("- Energy can be restored by sleeping or buying an energy potion.");
        sb.AppendLine("- If energy is too low, levels are blocked.");
        sb.AppendLine("- If hunger is too low, the pet becomes slower.");
        sb.AppendLine("- Hygiene is restored by bathing for free.");
        sb.AppendLine("- Completing an endpoint gives 0 Happiness.");
        sb.AppendLine("deterministic_recommendation:");
        sb.AppendLine("{");
        sb.AppendLine("  \"recommended_action\": \"" + decision.recommendedAction + "\",");
        sb.AppendLine("  \"priority_stat\": \"" + decision.priorityStat + "\",");
        sb.AppendLine("  \"should_save_apples\": " + ToJsonBool(decision.shouldSaveApples) + ",");
        sb.AppendLine("  \"reason\": \"" + Escape(decision.reason) + "\"");
        sb.AppendLine("}");
        return sb.ToString();
    }

    static string FormatFloat(float value)
    {
        return value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
    }

    static string Escape(string value)
    {
        return string.IsNullOrEmpty(value) ? string.Empty : value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    static string ToJsonBool(bool value)
    {
        return value ? "true" : "false";
    }

    static IEnumerable<string> QuoteEach(List<string> values)
    {
        if (values == null)
            yield break;

        for (int i = 0; i < values.Count; i++)
            yield return "\"" + values[i] + "\"";
    }
}
