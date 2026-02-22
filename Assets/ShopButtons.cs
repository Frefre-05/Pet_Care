using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ShopButtons : MonoBehaviour
{
    private const string TotalSpentKey = "CHATBOT_TOTAL_APPLES_SPENT";
    private const string TotalBoughtKey = "CHATBOT_TOTAL_ITEMS_BOUGHT";
    private const string HealthPotionKey = "CHATBOT_BOUGHT_health_potion";
    private const string EnergyPotionKey = "CHATBOT_BOUGHT_energy_potion";
    private const string SmallFoodKey = "CHATBOT_BOUGHT_small_food";
    private const string BigFoodKey = "CHATBOT_BOUGHT_big_food";

    [Header("Drag your pet / player here (optional)")]
    public PetNeeds petNeeds;

    [Header("Potion costs (in apples)")]
    public int healthPotionCost = 20;
    public int energyPotionCost = 20;

    [Header("Food costs (in apples)")]
    public int smallFoodCost = 5;
    public int bigFoodCost = 20;

    private PetNeeds GetActivePetNeeds()
    {
        PetNeeds[] all = UnityEngine.Object.FindObjectsByType<PetNeeds>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (PetNeeds pn in all)
        {
            if (pn != null && pn.gameObject.activeInHierarchy)
                return pn;
        }

        return null;
    }

    public void BuyHealthPotion()
    {
        PetNeeds target = GetActivePetNeeds();
        if (target == null)
        {
            Debug.LogError("ShopButtons: no ACTIVE PetNeeds found in scene!");
            return;
        }

        if (AppleCurrency.Spend(healthPotionCost))
        {
            target.Health = 100f;
            RecordPurchase(HealthPotionKey, healthPotionCost, 1);
            PixiePurchaseMemory.RecordPurchase("Health Potion", healthPotionCost, 1, AppleCurrency.Get());
            Debug.Log("Bought Health Potion (-" + healthPotionCost + " apples)");
        }
        else
        {
            Debug.Log("Not enough apples for Health Potion");
        }
    }

    public void BuyEnergyPotion()
    {
        PetNeeds target = GetActivePetNeeds();
        if (target == null)
        {
            Debug.LogError("ShopButtons: no ACTIVE PetNeeds found in scene!");
            return;
        }

        if (AppleCurrency.Spend(energyPotionCost))
        {
            target.Energy = 100f;
            RecordPurchase(EnergyPotionKey, energyPotionCost, 1);
            PixiePurchaseMemory.RecordPurchase("Energy Potion", energyPotionCost, 1, AppleCurrency.Get());
            Debug.Log("Bought Energy Potion (-" + energyPotionCost + " apples)");
        }
        else
        {
            Debug.Log("Not enough apples for Energy Potion");
        }
    }

    public void BuySmallFood()
    {
        PetNeeds target = GetActivePetNeeds();
        if (target == null)
        {
            Debug.LogError("ShopButtons: no ACTIVE PetNeeds found in scene!");
            return;
        }

        if (AppleCurrency.Spend(smallFoodCost))
        {
            target.Hunger = Mathf.Clamp(target.Hunger + 25f, 0f, 100f);
            RecordPurchase(SmallFoodKey, smallFoodCost, 1);
            PixiePurchaseMemory.RecordPurchase("Pineapple", smallFoodCost, 1, AppleCurrency.Get());
            Debug.Log("Bought Small Food (-" + smallFoodCost + " apples, +25 Hunger)");
        }
        else
        {
            Debug.Log("Not enough apples for Small Food");
        }
    }

    public void BuyBigFood()
    {
        PetNeeds target = GetActivePetNeeds();
        if (target == null)
        {
            Debug.LogError("ShopButtons: no ACTIVE PetNeeds found in scene!");
            return;
        }

        if (AppleCurrency.Spend(bigFoodCost))
        {
            target.Hunger = 100f;
            RecordPurchase(BigFoodKey, bigFoodCost, 1);
            PixiePurchaseMemory.RecordPurchase("Big Food", bigFoodCost, 1, AppleCurrency.Get());
            Debug.Log("Bought Big Food (-" + bigFoodCost + " apples, Hunger = 100)");
        }
        else
        {
            Debug.Log("Not enough apples for Big Food");
        }
    }

    private void RecordPurchase(string perItemKey, int costPerItem, int quantity)
    {
        if (quantity <= 0) return;
        int safeCost = Mathf.Max(0, costPerItem);

        int currentItemCount = PlayerPrefs.GetInt(perItemKey, 0);
        PlayerPrefs.SetInt(perItemKey, currentItemCount + quantity);

        int currentTotalBought = PlayerPrefs.GetInt(TotalBoughtKey, 0);
        PlayerPrefs.SetInt(TotalBoughtKey, currentTotalBought + quantity);

        int currentTotalSpent = PlayerPrefs.GetInt(TotalSpentKey, 0);
        PlayerPrefs.SetInt(TotalSpentKey, currentTotalSpent + (safeCost * quantity));

        PlayerPrefs.Save();
    }
}

[Serializable]
public class PixiePurchaseEntry
{
    public string isoTime;
    public string item;
    public int quantity;
    public int cost;
    public int remainingApples;
}

public static class PixiePurchaseMemory
{
    private const string LastItemKey = "PIXIE_LAST_PURCHASE_ITEM";
    private const string LastCostKey = "PIXIE_LAST_PURCHASE_COST";
    private const string LastQtyKey = "PIXIE_LAST_PURCHASE_QTY";
    private const string LastRemainingKey = "PIXIE_LAST_PURCHASE_REMAINING";
    private const string LastTimeKey = "PIXIE_LAST_PURCHASE_TIME";
    private const string LogKey = "PIXIE_PURCHASE_LOG";
    private const int MaxLogEntries = 120;

    public static void RecordPurchase(string item, int cost, int quantity, int remainingApples)
    {
        if (string.IsNullOrWhiteSpace(item) || quantity <= 0) return;

        string nowIso = DateTime.UtcNow.ToString("O");
        PlayerPrefs.SetString(LastItemKey, item);
        PlayerPrefs.SetInt(LastCostKey, Mathf.Max(0, cost));
        PlayerPrefs.SetInt(LastQtyKey, quantity);
        PlayerPrefs.SetInt(LastRemainingKey, Mathf.Max(0, remainingApples));
        PlayerPrefs.SetString(LastTimeKey, nowIso);

        List<PixiePurchaseEntry> entries = GetPurchaseLog(200);
        entries.Add(new PixiePurchaseEntry
        {
            isoTime = nowIso,
            item = item,
            quantity = quantity,
            cost = Mathf.Max(0, cost),
            remainingApples = Mathf.Max(0, remainingApples)
        });

        if (entries.Count > MaxLogEntries)
            entries.RemoveRange(0, entries.Count - MaxLogEntries);

        SaveEntries(entries);
        PlayerPrefs.Save();
    }

    public static string GetLastPurchaseSummary()
    {
        string item = PlayerPrefs.GetString(LastItemKey, string.Empty);
        if (string.IsNullOrWhiteSpace(item))
            return "No purchase history yet.";

        int cost = PlayerPrefs.GetInt(LastCostKey, 0);
        int qty = PlayerPrefs.GetInt(LastQtyKey, 1);
        int remaining = PlayerPrefs.GetInt(LastRemainingKey, 0);
        string isoTime = PlayerPrefs.GetString(LastTimeKey, string.Empty);

        string whenText = string.Empty;
        if (DateTime.TryParse(isoTime, out DateTime dt))
            whenText = dt.ToLocalTime().ToString("g");

        if (!string.IsNullOrEmpty(whenText))
            return "Last purchase: " + item + " x" + qty + " for " + cost + " apples at " + whenText + ". Remaining apples: " + remaining + ".";

        return "Last purchase: " + item + " x" + qty + " for " + cost + " apples. Remaining apples: " + remaining + ".";
    }

    public static List<PixiePurchaseEntry> GetPurchaseLog(int maxEntries)
    {
        string raw = PlayerPrefs.GetString(LogKey, string.Empty);
        List<PixiePurchaseEntry> list = new List<PixiePurchaseEntry>();

        if (!string.IsNullOrWhiteSpace(raw))
        {
            string[] lines = raw.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] p = line.Split('|');
                if (p.Length < 5) continue;
                if (!int.TryParse(p[2], out int qty)) qty = 1;
                if (!int.TryParse(p[3], out int cost)) cost = 0;
                if (!int.TryParse(p[4], out int remaining)) remaining = 0;

                list.Add(new PixiePurchaseEntry
                {
                    isoTime = p[0],
                    item = p[1],
                    quantity = qty,
                    cost = cost,
                    remainingApples = remaining
                });
            }
        }

        if (maxEntries > 0 && list.Count > maxEntries)
            list.RemoveRange(0, list.Count - maxEntries);

        return list;
    }

    public static string GetPurchaseLogSummary(int maxEntries)
    {
        List<PixiePurchaseEntry> entries = GetPurchaseLog(maxEntries);
        if (entries.Count == 0) return "No purchases logged.";

        StringBuilder sb = new StringBuilder();
        int start = Mathf.Max(0, entries.Count - maxEntries);
        for (int i = start; i < entries.Count; i++)
        {
            PixiePurchaseEntry e = entries[i];
            sb.Append(e.item).Append(" x").Append(e.quantity).Append(" (").Append(e.cost).Append(" apples)");
            if (i < entries.Count - 1) sb.Append(" | ");
        }

        return sb.ToString();
    }

    private static void SaveEntries(List<PixiePurchaseEntry> entries)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < entries.Count; i++)
        {
            PixiePurchaseEntry e = entries[i];
            sb.Append(Safe(e.isoTime)).Append('|')
              .Append(Safe(e.item)).Append('|')
              .Append(e.quantity).Append('|')
              .Append(e.cost).Append('|')
              .Append(e.remainingApples);
            if (i < entries.Count - 1) sb.Append('\n');
        }

        PlayerPrefs.SetString(LogKey, sb.ToString());
    }

    private static string Safe(string value)
    {
        return string.IsNullOrEmpty(value) ? string.Empty : value.Replace('|', '/').Replace('\n', ' ');
    }
}

