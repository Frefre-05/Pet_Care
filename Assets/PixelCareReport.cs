using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum PixelCareSpendingCategory
{
    Food,
    Toys,
    Health,
    Activity
}

public sealed class PixelCareReportData
{
    public int applesEarned;
    public int applesSpent;
    public int currentBalance;
    public int foodSpending;
    public int toySpending;
    public int healthSpending;
    public int activitySpending;
    public int tasksCompleted;
    public string mostSpentCategory;
    public string simpleStatus;
    public string nextAction;

    public string ToDisplayText()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Pixel Care Report");
        sb.AppendLine();
        sb.AppendLine("Balance");
        sb.AppendLine("Gold Coins Earned -> " + applesEarned);
        sb.AppendLine("Gold Coins Spent -> " + applesSpent);
        sb.AppendLine("Current Balance -> " + currentBalance);
        sb.AppendLine();
        sb.AppendLine("Spending Breakdown");
        sb.AppendLine("Food Spending -> " + foodSpending);
        sb.AppendLine("Toy Spending -> " + toySpending);
        sb.AppendLine("Health Spending -> " + healthSpending);
        sb.AppendLine("Activity Spending -> " + activitySpending);
        sb.AppendLine();
        sb.AppendLine("Tasks Completed -> " + tasksCompleted);
        sb.AppendLine();
        sb.AppendLine("Insight");
        sb.AppendLine("Most Spent Category -> " + mostSpentCategory);
        sb.AppendLine("Simple Status -> " + simpleStatus);
        sb.AppendLine("What should I do next? -> " + nextAction);
        return sb.ToString().TrimEnd();
    }
}

public static class PixelCareReport
{
    private const string TasksCompletedKey = "PIXEL_CARE_TASKS_COMPLETED";

    public static int TasksCompleted => PlayerPrefs.GetInt(TasksCompletedKey, 0);

    public static void RecordTaskCompleted()
    {
        PlayerPrefs.SetInt(TasksCompletedKey, TasksCompleted + 1);
        PlayerPrefs.Save();
    }

    public static PixelCareReportData Build()
    {
        PixelCareReportData report = new PixelCareReportData
        {
            currentBalance = AppleCurrency.Get(),
            tasksCompleted = TasksCompleted
        };

        List<PixiePurchaseEntry> purchases = PixiePurchaseMemory.GetPurchaseLog(200);
        for (int i = 0; i < purchases.Count; i++)
        {
            PixiePurchaseEntry entry = purchases[i];
            if (entry == null)
                continue;

            int spent = Mathf.Max(0, entry.cost) * Mathf.Max(1, entry.quantity);
            report.applesSpent += spent;

            switch (ClassifyPurchase(entry.item))
            {
                case PixelCareSpendingCategory.Food:
                    report.foodSpending += spent;
                    break;
                case PixelCareSpendingCategory.Toys:
                    report.toySpending += spent;
                    break;
                case PixelCareSpendingCategory.Health:
                    report.healthSpending += spent;
                    break;
                case PixelCareSpendingCategory.Activity:
                    report.activitySpending += spent;
                    break;
            }
        }

        report.applesEarned = Mathf.Max(AppleCurrency.GetTotalEarned(), report.currentBalance + report.applesSpent);
        report.mostSpentCategory = GetMostSpentCategory(report);
        report.simpleStatus = BuildSimpleStatus(report);
        report.nextAction = BuildNextAction(report);
        return report;
    }

    private static PixelCareSpendingCategory ClassifyPurchase(string item)
    {
        string value = (item ?? string.Empty).Trim().ToLowerInvariant();
        if (value.Contains("kiwi") || value.Contains("pineapple") || value.Contains("cherry") || value.Contains("food"))
            return PixelCareSpendingCategory.Food;
        if (value.Contains("health") || value.Contains("energy potion") || value.Contains("medicine"))
            return PixelCareSpendingCategory.Health;
        if (value.Contains("jump boost") || value.Contains("speed boost"))
            return PixelCareSpendingCategory.Activity;
        if (value.Contains("toy") || value.Contains("teddy") || value.Contains("volleyball") || value.Contains("volley ball") || value.Contains("beach ball"))
            return PixelCareSpendingCategory.Toys;
        return PixelCareSpendingCategory.Activity;
    }

    private static string GetMostSpentCategory(PixelCareReportData report)
    {
        string bestName = "None";
        int bestValue = 0;
        Consider("Food", report.foodSpending, ref bestName, ref bestValue);
        Consider("Toys", report.toySpending, ref bestName, ref bestValue);
        Consider("Health", report.healthSpending, ref bestName, ref bestValue);
        Consider("Activity", report.activitySpending, ref bestName, ref bestValue);
        return bestName;
    }

    private static void Consider(string name, int value, ref string bestName, ref int bestValue)
    {
        if (value > bestValue)
        {
            bestName = name;
            bestValue = value;
        }
    }

    private static string BuildSimpleStatus(PixelCareReportData report)
    {
        if (report.applesEarned <= 0 && report.applesSpent <= 0)
            return "No report activity yet";
        if (report.applesSpent <= report.applesEarned / 2)
            return "Saving well";
        if (report.applesSpent <= report.applesEarned)
            return "Spending is balanced";
        return "Spending is high";
    }

    private static string BuildNextAction(PixelCareReportData report)
    {
        if (report.currentBalance < 5)
            return "Gather more Gold Coins before buying anything else.";
        if (report.healthSpending == 0 && report.currentBalance >= 20)
            return "Keep enough Gold Coins ready for a Health Potion.";
        if (report.foodSpending == 0 && report.currentBalance >= 5)
            return "Buy food if Hunger starts dropping.";
        if (report.applesSpent > report.applesEarned)
            return "Pause spending and complete chores or levels.";
        return "Keep saving Gold Coins and spend only when a care stat needs it.";
    }
}
