using TMPro; // If using TextMeshPro
using UnityEngine;

public class ChemistryGlass : MonoBehaviour
{
    public string chemicalName; // Set this in the inspector for each glass
    public TextMeshProUGUI elementText; // Assign the text UI object for the element here

    private void Start()
    {
        // Ensure the text is initially hidden
        if (elementText != null)
        {
            elementText.text = ""; // Clear the text
            elementText.gameObject.SetActive(false); // Hide the text
        }
    }

    // Show the element name
    public void ShowText()
    {
        if (elementText != null)
        {
            elementText.text = $"{chemicalName}";
            Debug.Log($"Setting text to: Element: {chemicalName}");

            elementText.gameObject.SetActive(true);
        }
    }

    // Hide the element name
    public void HideText()
    {
        if (elementText != null)
        {
            elementText.text = "";
            elementText.gameObject.SetActive(false);
        }
    }
}
