using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PetColorChanger : MonoBehaviour
{
    [Range(0f, 1f)]
    public float tintAlpha = 0.5f; // how strong the tint is (0.5 = 50%)

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Called by PetNeeds whenever a need value changes
    public void UpdateColors(float hunger, float energy, float hygiene, float happiness, float health)
    {

        // start with no tint
        Color tint = Color.white;
        bool useTint = false;

        // Order: health > hygiene > happiness > hunger > energy
        if (health < 50f)
        {
            tint = Color.red; // low health
            useTint = true;
        }
        else if (hygiene < 60f)
        {
            tint = new Color(0.6f, 0.3f, 0f); // brown-ish
            useTint = true;
        }
        else if (happiness < 70f)
        {
            tint = Color.blue; // sad
            useTint = true;
        }
        else if (hunger < 80f)
        {
            tint = Color.yellow; // hungry
            useTint = true;
        }
        else if (energy < 80f)
        {
            tint = Color.cyan; // tired
            useTint = true;
        }

        if (useTint)
        {
            tint.a = tintAlpha; // 50% opacity-ish
            sr.color = tint;
        }
        else
        {
            sr.color = Color.white; // normal colors
        }
    }
}
