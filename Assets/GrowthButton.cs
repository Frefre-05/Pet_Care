using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class GrowthButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Costs & Requirements")]
    [SerializeField] private int appleCost = 20; // 20 apples to grow
    [SerializeField, Range(0, 100)] private float requiredNeed = 70f;
    [SerializeField] private float maxScale = 11f; // stop growing at 11
    [SerializeField] private float scaleStep = 1f; // grow by 1 each time

    [Header("References")]
    [SerializeField] private PetNeeds petNeeds; // we'll overwrite this with the CLONE
    [SerializeField] private TextMeshProUGUI tooltipText; // hover text (optional)

    private Transform petTransform;

    void Awake()
    {
        if (tooltipText != null)
            tooltipText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Always grab PetNeeds from the active player clone (tag "Player").
    /// This is the SAME idea we used for BedSleepInteractor / BathInteractor.
    /// </summary>
    private void RefreshPetReference()
    {
        // If inspector reference is already the active clone, use it
        if (petNeeds != null && petNeeds.gameObject.activeInHierarchy)
        {
            petTransform = petNeeds.transform;
            return;
        }

        // Otherwise, find the player clone by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            petNeeds = player.GetComponentInChildren<PetNeeds>();
            if (petNeeds != null)
            {
                petTransform = petNeeds.transform;
            }
        }
    }

    private bool AllNeedsHighEnough(PetNeeds pn)
    {
        return pn.Hunger >= requiredNeed &&
        pn.Energy >= requiredNeed &&
        pn.Hygiene >= requiredNeed &&
        pn.Happiness >= requiredNeed &&
        pn.Health >= requiredNeed;
    }

    // ======== BUTTON ONCLICK ========

    public void TryGrow()
    {
        // 1) make sure we’re using the CLONE
        RefreshPetReference();

        if (petNeeds == null || petTransform == null)
        {
            Debug.LogError("GrowthButton: no PetNeeds found on the active Player clone.");
            return;
        }

        // 2) check needs
        if (!AllNeedsHighEnough(petNeeds))
        {
            Debug.Log("GrowthButton: needs too low, cannot grow.");
            return;
        }

        // 3) check current scale vs max
        float currentScale = petTransform.localScale.x;
        if (currentScale >= maxScale)
        {
            Debug.Log("GrowthButton: already at max size.");
            return;
        }

        // 4) spend apples using your APPLE_COUNT system (AppleCurrency)
        if (!AppleCurrency.Spend(appleCost))
        {
            Debug.Log("GrowthButton: not enough apples to grow.");
            return;
        }

        // 5) grow ONLY THE CLONE
        float newScale = Mathf.Min(currentScale + scaleStep, maxScale);
        petTransform.localScale = new Vector3(newScale, newScale, petTransform.localScale.z);

        // save scale so it persists between scenes
        PlayerPrefs.SetFloat("PET_SCALE", newScale);
        PlayerPrefs.Save();

        Debug.Log($"GrowthButton: grew CLONE to {newScale}, apples spent: {appleCost}");
    }

    // ======== HOVER TOOLTIP ========

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipText != null)
        {
            tooltipText.text = $"Requires all bars above {requiredNeed} and {appleCost} apples";
            tooltipText.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipText != null)
            tooltipText.gameObject.SetActive(false);
    }
}
