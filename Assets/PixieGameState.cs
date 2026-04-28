using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PixieGameState
{
    public int apples;
    public float health;
    public float hunger;
    public float energy;
    public float hygiene;
    public float happiness;
    public int maxUnlockedLevel;
    public bool levelsBlockedByLowEnergy;
    public bool hungerSlowsPet;
    public List<string> allowedActions = new List<string>();

    public static PixieGameState Capture(PetNeeds petNeeds, ShopButtons shopButtons, PixieDecisionConfig config)
    {
        PixieGameState state = new PixieGameState
        {
            apples = AppleCurrency.Get(),
            maxUnlockedLevel = LevelProgress.MaxUnlocked
        };

        if (petNeeds != null)
        {
            state.health = petNeeds.Health;
            state.hunger = petNeeds.Hunger;
            state.energy = petNeeds.Energy;
            state.hygiene = petNeeds.Hygiene;
            state.happiness = petNeeds.Happiness;
        }
        else
        {
            state.health = 100f;
            state.hunger = 100f;
            state.energy = 100f;
            state.hygiene = 100f;
            state.happiness = 100f;
        }

        float energyGateThreshold = config != null ? config.EnergyGateThreshold : PixieDecisionConfig.DefaultEnergyGateThreshold;
        float hungerSlowThreshold = config != null ? config.HungerSlowThreshold : PixieDecisionConfig.DefaultHungerSlowThreshold;
        state.levelsBlockedByLowEnergy = state.energy < energyGateThreshold;
        state.hungerSlowsPet = state.hunger <= hungerSlowThreshold;
        return state;
    }
}

[Serializable]
public class PixieResponse
{
    public string recommended_action;
    public string reason;
    public string priority_stat;
    public bool should_save_apples;
    public string message;
}

[Serializable]
public class PixieDecisionResult
{
    public string recommendedAction;
    public string reason;
    public string priorityStat;
    public bool shouldSaveApples;
    public List<string> allowedActions = new List<string>();

    public PixieResponse ToResponse()
    {
        return new PixieResponse
        {
            recommended_action = recommendedAction,
            reason = reason,
            priority_stat = priorityStat,
            should_save_apples = shouldSaveApples,
            message = reason
        };
    }
}
