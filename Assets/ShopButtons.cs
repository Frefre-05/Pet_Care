using UnityEngine;

public class ShopButtons : MonoBehaviour
{
    [Header("Drag your pet / player here (optional)")]
    public PetNeeds petNeeds; // we won't trust this, we’ll always look for the active one

    [Header("Potion costs (in apples)")]
    public int healthPotionCost = 20;
    public int energyPotionCost = 20; // or 10, change in Inspector


    [Header("Food costs (in apples)")]
    public int smallFoodCost = 5; // item 1 -> +25 Hunger
    public int bigFoodCost = 20; // item 2 -> 100 Hunger


    // ================== NEW HELPER ==================
    // Returns the PetNeeds on the *active* pet (your clone).
    PetNeeds GetActivePetNeeds()
    {
        // Look through every PetNeeds in the scene (even inactive roots)
        PetNeeds[] all = FindObjectsOfType<PetNeeds>(true);

        foreach (var pn in all)
        {
            // only use the one that is actually active in the hierarchy
            if (pn.gameObject.activeInHierarchy)
            {
                return pn;
            }
        }

        return null; // none found
    }


    // ================== EXISTING BUTTONS ==================

    // Called by Health button
    public void BuyHealthPotion()
    {
        PetNeeds target = GetActivePetNeeds(); // <<< use active clone

        if (target == null)
        {
            Debug.LogError("ShopButtons: no ACTIVE PetNeeds found in scene!");
            return;
        }

        // try to pay apples
        if (AppleCurrency.Spend(healthPotionCost))
        {
            // raise bar(s)
            target.Health = 100f;
            Debug.Log("Bought Health Potion (-" + healthPotionCost + " apples)");
        }
        else
        {
            Debug.Log("Not enough apples for Health Potion");
        }
    }

    // Called by Energy button
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
            Debug.Log("Bought Energy Potion (-" + energyPotionCost + " apples)");
        }
        else
        {
            Debug.Log("Not enough apples for Energy Potion");
        }
    }


    // ================== HUNGER FOOD BUTTONS ==================

    // Called by SMALL FOOD button (costs 5 apples, +25 hunger)
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
            Debug.Log("Bought Small Food (-" + smallFoodCost + " apples, +25 Hunger)");
        }
        else
        {
            Debug.Log("Not enough apples for Small Food");
        }
    }

    // Called by BIG FOOD button (costs 20 apples, Hunger = 100)
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
            Debug.Log("Bought Big Food (-" + bigFoodCost + " apples, Hunger = 100)");
        }
        else
        {
            Debug.Log("Not enough apples for Big Food");
        }
    }
}
