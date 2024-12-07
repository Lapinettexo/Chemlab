using System.Collections.Generic;
using UnityEngine;

public class PickUpAndDrop : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int maxInventorySize = 5; // Maximum number of items in inventory
    private List<GameObject> inventory = new List<GameObject>(); // Inventory list
    private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>(); // Track original positions
    

    [Header("Raycast Settings")]
    public float pickUpRange = 2f; // How far the player can pick up objects
    public Transform playerCam; // Camera transform for raycast origin

    [Header("Highlight Settings")]
    public Color highlightColor = Color.yellow; // Highlight color
    public float highlightIntensity = 1.5f; // Highlight intensity

    private GameObject currentObject; // Object in range for pickup
    private Renderer currentObjectRenderer; // Renderer of the current object
    private Color originalColor; // Original color of the object

    private int selectedItemIndex = -1; // Keeps track of the currently selected item in inventory

    public int InventoryCount => inventory.Count;

    public List<GameObject> GetInventory()
    {
        return new List<GameObject>(inventory); // Return a copy to avoid modifying the original list
    }

    private void Start()
    {
        if (playerCam == null)
        {
            playerCam = Camera.main.transform; // Automatically find the main camera
        }
    }

    private void Update()
    {
        // Detect objects with a raycast
        CheckForObjectInRange();

        // Check for pickup
        if (Input.GetKeyDown(KeyCode.E) && currentObject != null && inventory.Count < maxInventorySize)
        {
            PickUp(currentObject);
        }

        // Select the next item in the inventory
        if (Input.GetKeyDown(KeyCode.Tab) && inventory.Count > 0)
        {
            CycleInventory();
        }

        // Check for drop
        if (Input.GetKeyDown(KeyCode.R) && selectedItemIndex >= 0)
        {
            Drop(selectedItemIndex);
        }
    }

    private void CheckForObjectInRange()
    {
        // Send a raycast from the player's camera
        if (Physics.Raycast(playerCam.position, playerCam.forward, out RaycastHit hit, pickUpRange))
        {
            // Check if the object is pickable
            if (hit.collider.CompareTag("Pickable"))
            {
                // If it's a new object, handle highlighting
                if (currentObject != hit.collider.gameObject)
                {
                    ClearHighlight();
                    currentObject = hit.collider.gameObject;
                    HighlightObject(currentObject);
                }
                return;
            }
        }

        // If no valid object is hit, clear the highlight
        ClearHighlight();
    }

    private void ClearHighlight()
    {
        if (currentObjectRenderer != null)
        {
            currentObjectRenderer.material.color = originalColor; // Reset to original color
        }

        // Hide the element name via the ChemistryGlass script
        if (currentObject != null)
        {
            ChemistryGlass glassScript = currentObject.GetComponent<ChemistryGlass>();
            if (glassScript != null)
            {
                glassScript.HideText();
            }
        }

        currentObject = null;
        currentObjectRenderer = null;
    }


    private void HighlightObject(GameObject obj)
    {
        // Get the renderer component for highlighting
        currentObjectRenderer = obj.GetComponent<Renderer>();

        if (currentObjectRenderer != null)
        {
            // Store the original color
            originalColor = currentObjectRenderer.material.color;

            // Calculate and apply the highlighted color
            Color highlightedColor = originalColor * highlightColor * highlightIntensity;
            currentObjectRenderer.material.color = highlightedColor;
        }

        // Show the element name via the ChemistryGlass script
        ChemistryGlass glassScript = obj.GetComponent<ChemistryGlass>();
        if (glassScript != null)
        {
            glassScript.ShowText();
        }
    }


    private void PickUp(GameObject obj)
    {
        // Remove highlight before picking up
        ClearHighlight();

        inventory.Add(obj); // Add to inventory
        originalPositions[obj] = obj.transform.position; // Store original position
        obj.SetActive(false); // Deactivate in the scene

        if (inventory.Count == 1)
        {
            selectedItemIndex = 0; // Select the first item automatically
        }

        Debug.Log($"Picked up {obj.name}");
    }

    private void CycleInventory()
    {
        // Cycle through the inventory items
        selectedItemIndex = (selectedItemIndex + 1) % inventory.Count;
        Debug.Log($"Selected Item: {inventory[selectedItemIndex].name}");
    }

    public void Drop(int itemIndex)
    {
        // Get the selected item from the inventory
        GameObject objToDrop = inventory[itemIndex];
        inventory.RemoveAt(itemIndex); // Remove from inventory

        // Reactivate the object in the scene and place it at its original position
        if (originalPositions.TryGetValue(objToDrop, out Vector3 position))
        {
            objToDrop.transform.position = position;
        }
        objToDrop.SetActive(true);

        Debug.Log($"Dropped {objToDrop.name} back in the scene");

        // Adjust the selected index if necessary
        if (inventory.Count == 0)
        {
            selectedItemIndex = -1; // No items left
        }
        else if (selectedItemIndex >= inventory.Count)
        {
            selectedItemIndex = 0; // Reset to first item
        }
    }

    
}
