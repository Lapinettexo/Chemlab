using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class ElementRandomizer : MonoBehaviour
{
    [System.Serializable]
    public class Compound
    {
        public int id;
        public string CompoundName;
        public string MolecularFormula;
    }


    [Header("References")]
    public ChemistryGlass[] glasses; // Drag all glasses here in the inspector
    public TextAsset elementsTextFile; // Drag the Elements.txt file here
    public TextAsset compoundsJsonFile; // Drag the compounds JSON file here

    public TextMeshProUGUI instructionsText;
    public Image backgroundPanel;
    public PickUpAndDrop inventoryManager;

    private List<string> allElements = new List<string>();
    private List<Compound> compounds = new List<Compound>();
    private Compound currentTargetCompound;

    private void Start()
    {
        if (inventoryManager == null)
        {
            inventoryManager = GetComponent<PickUpAndDrop>();
            if (inventoryManager == null)
            {
                Debug.LogError("PickUpAndDrop script not found!");
            }
        }
        // Parse elements and compounds
        ParseElements();
        ParseCompounds();

        // Generate a round of gameplay
        GenerateRound();
    }

    private void ReturnAllGlassesToOriginalPositions()
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning("Inventory Manager is not assigned.");
            return;
        }

        List<GameObject> inventory = inventoryManager.GetInventory();

        for (int i = inventory.Count - 1; i >= 0; i--) // Iterate backward to avoid index shift issues
        {
            inventoryManager.Drop(i); // Use Drop to return glasses to their original positions
        }

        Debug.Log("All glasses returned to their original positions.");
    }

    private void ParseElements()
    {
        if (elementsTextFile == null)
        {
            Debug.LogError("Elements text file is not assigned!");
            return;
        }

        Debug.Log("Raw file contents:\n" + elementsTextFile.text);

        // Split the text file content into lines and extract the chemical symbol
        allElements = elementsTextFile.text
            .Split('\n')
            .Select(line =>
            {
                var parts = line.Trim().Split('-');
                return parts.Length > 1 ? parts[1].Trim() : null; // Get the chemical symbol
            })
            .Where(symbol => !string.IsNullOrEmpty(symbol)) // Filter out null or empty values
            .ToList();

        Debug.Log($"Parsed {allElements.Count} elements ta3 elments");

        foreach (var element in allElements)
        {
            Debug.Log($"element: {element}");
        }
    }
    private void ParseCompounds()
    {
        if (compoundsJsonFile == null)
        {
            Debug.LogError("Compounds text file is not assigned!");
            return;
        }

        // Split the text file content into lines
        string[] lines = compoundsJsonFile.text.Split('\n');

        //compounds.Clear(); // Clear existing compounds

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Trim().Split(',');
            if (parts.Length >= 3)
            {
                Compound compound = new Compound
                {
                    id = int.Parse(parts[0]),
                    CompoundName = parts[1],
                    MolecularFormula = parts[2]
                };

                compounds.Add(compound);
            }
        }

        Debug.Log($"Parsed {compounds.Count} compounds ta3 parse");

        // Optional: Log parsed compounds for verification
        /*foreach (var compound in compounds)
        {
            Debug.Log($"Compound: {compound.CompoundName}, Formula: {compound.MolecularFormula}");
        }*/
    }

    // New method to get the required substances for the current compound
    public List<string> GetRequiredSubstances()
    {
        // Extract required elements from the formula
        HashSet<string> requiredElements = ExtractElementsFromFormula(currentTargetCompound.MolecularFormula);
        return requiredElements.ToList();
    }

    public string GetResultingCompound()
    {
        return currentTargetCompound.CompoundName;
    }

    public void GenerateRound()
    {
        // Select a random compound
        //ParseElements();
        //ParseCompounds();
        if (compounds.Count > 0)
        {
            currentTargetCompound = compounds[Random.Range(0, compounds.Count)];
            if (instructionsText != null)
            {
                instructionsText.text = $"Target Compound: {currentTargetCompound.CompoundName}\nMolecular Formula: {currentTargetCompound.MolecularFormula}\nPress Enter to dissappear";
                instructionsText.gameObject.SetActive(true);
                backgroundPanel.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Instructions text object is not assigned!");
            }
            Debug.Log($"Target Compound: {currentTargetCompound.CompoundName}");
            Debug.Log($"Molecular Formula: {currentTargetCompound.MolecularFormula}");

            // Check if the MolecularFormula is not empty
            if (string.IsNullOrEmpty(currentTargetCompound.MolecularFormula))
            {
                Debug.LogError("Molecular formula is empty for the selected compound.");
                return; // Early exit if formula is invalid
            }

            // Extract required elements from the formula
            HashSet<string> requiredElements = ExtractElementsFromFormula(currentTargetCompound.MolecularFormula);

            // Prepare a list of elements to assign
            List<string> elementsToAssign = new List<string>(requiredElements);

            // Shuffle the available elements to randomize additional element selection
            List<string> availableElements = allElements.Except(requiredElements).OrderBy(x => Random.value).ToList();

            var shuffledGlasses = glasses.OrderBy(x => Random.value).ToArray();

            // Assign elements to glasses
            for (int i = 0; i < shuffledGlasses.Length; i++)
            {
                if (i < elementsToAssign.Count)
                {
                    // Assign required elements to random glasses
                    shuffledGlasses[i].chemicalName = elementsToAssign[i];
                    Debug.Log($"Glass {i} assigned required element: {elementsToAssign[i]}");
                }
                else
                {
                    // Assign random elements to remaining glasses
                    if (availableElements.Count > 0)
                    {
                        string randomElement = availableElements[0];
                        shuffledGlasses[i].chemicalName = randomElement;
                        Debug.Log($"Glass {i} assigned random element: {randomElement}");
                        availableElements.RemoveAt(0);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("No compounds found in the list. ta3 generate");
        }
    }

    private HashSet<string> ExtractElementsFromFormula(string formula)
    {
        HashSet<string> elements = new HashSet<string>();
        
        // Remove any whitespace from the formula
        formula = formula.Replace(" ", "");
        
        for (int i = 0; i < formula.Length; i++)
        {
            // Check for uppercase letter (start of an element symbol)
            if (char.IsUpper(formula[i]))
            {
                string element = formula[i].ToString();
                
                // Check if next character is a lowercase letter (part of the element symbol)
                if (i + 1 < formula.Length && char.IsLower(formula[i + 1]))
                {
                    element += formula[i + 1];
                    i++; // Skip the next character
                }
                
                elements.Add(element);
            }
        }
        
        return elements;
    }
    // Debug method to force a new round
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) // Press R to regenerate
        {
            ReturnAllGlassesToOriginalPositions();
            GenerateRound();
        }
        if (Input.GetKeyDown(KeyCode.Return) && instructionsText != null)
        {
            instructionsText.gameObject.SetActive(false);
            backgroundPanel.gameObject.SetActive(false);
        }
    }
}
