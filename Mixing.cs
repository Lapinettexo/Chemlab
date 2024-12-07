using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class Mixing : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Color highlightColor = Color.green;    // Highlight color for the mixer
    public float highlightIntensity = 1.5f;       // Highlight intensity

    private GameObject currentMixer;              // The mixer currently in range
    private Renderer mixerRenderer;               // Renderer of the mixer
    private Color originalColor;                  // Original color of the mixer

    private PickUpAndDrop inventoryManager;

    // Reference to the ElementRandomizer script to access the random substances
    public TextMeshProUGUI instructionsText;  
    public Image backgroundPanel;
    private ElementRandomizer elementRandomizer;

    private void Start()
    {
        // Automatically find the PickUpAndDrop script on the same GameObject
        inventoryManager = GetComponent<PickUpAndDrop>();

        if (inventoryManager == null)
        {
            Debug.LogError("PickUpAndDrop script not found on the same GameObject!");
        }
        else
        {
            Debug.Log("Successfully found PickUpAndDrop script.");
        }

        // Get the reference to ElementRandomizer
        elementRandomizer = GetComponent<ElementRandomizer>();
        if (elementRandomizer == null)
        {
            Debug.LogError("ElementRandomizer script not found on the same GameObject!");
        }
    }

    private void Update()
    {
        CheckForMixerInRange();

        if (inventoryManager == null || elementRandomizer == null)
        {
            Debug.LogWarning("InventoryManager or ElementRandomizer is null. Cannot check for mixing.");
            return;
        }

        if (Input.GetKeyDown(KeyCode.B)) // Assuming 'B' is your mixer interaction key
        {
            Debug.Log("Key 'B' pressed, attempting to check mixing.");
            PerformMixing();
        }
    }

    private void CheckForMixerInRange()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 2f))
        {
            if (hit.collider.CompareTag("Mixer"))
            {
                if (currentMixer != hit.collider.gameObject)
                {
                    ClearHighlight();
                    currentMixer = hit.collider.gameObject;
                    HighlightMixer(currentMixer);
                }
                return;
            }
        }

        ClearHighlight();
    }

    private void ClearHighlight()
    {
        if (mixerRenderer != null)
        {
            mixerRenderer.material.color = originalColor;
        }

        currentMixer = null;
        mixerRenderer = null;
    }

    private void HighlightMixer(GameObject mixer)
    {
        mixerRenderer = mixer.GetComponent<Renderer>();

        if (mixerRenderer != null)
        {
            originalColor = mixerRenderer.material.color;

            Color highlightedColor = originalColor * highlightColor * highlightIntensity;
            mixerRenderer.material.color = highlightedColor;
        }
    }

    private void PerformMixing()
    {
        List<GameObject> inventory = inventoryManager.GetInventory();
        Debug.Log($"Inventory count: {inventory.Count}");

        if (inventory.Count < 1)
        {
            Debug.Log("Not enough glasses in inventory to mix.");
            instructionsText.gameObject.SetActive(true);
            backgroundPanel.gameObject.SetActive(true);
            instructionsText.text = "Not enough glasses in inventory to mix.";
            return;
        }

        // Retrieve all required substances and resulting compound from ElementRandomizer
        List<string> requiredSubstances = elementRandomizer.GetRequiredSubstances(); // Get all required substances
        string resultingCompound = elementRandomizer.GetResultingCompound(); // Get resulting compound name

        // Create a list to store the substances found in the inventory
        List<string> substancesInInventory = new List<string>();

        // Loop through the inventory to collect the substances
        foreach (GameObject glass in inventory)
        {
            ChemistryGlass glassScript = glass.GetComponent<ChemistryGlass>();

            if (glassScript == null)
            {
                Debug.LogError("Glass does not have ChemistryGlass script!");
                continue;
            }

            // Add the substance to the list if it matches
            if (!substancesInInventory.Contains(glassScript.chemicalName))
            {
                substancesInInventory.Add(glassScript.chemicalName);
            }
        }

        // Check if all required substances are found in the inventory
        bool allRequiredSubstancesPresent = requiredSubstances.All(substance => substancesInInventory.Contains(substance));

        if (allRequiredSubstancesPresent)
        {
            Debug.Log($"You created {resultingCompound}!");
            instructionsText.gameObject.SetActive(true);
            backgroundPanel.gameObject.SetActive(true);
            instructionsText.text = $"You created {resultingCompound}!";
        }
        else
        {
            Debug.Log("The substances do not form the desired compound.");
            instructionsText.gameObject.SetActive(true);
            backgroundPanel.gameObject.SetActive(true);
            instructionsText.text = "The substances do not form the desired compound.";
        }
    }
}
