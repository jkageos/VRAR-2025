using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject characterPrefab;

    [Header("Menu Options")]
    public string[] shapes;    // Round, Square, Heart
    public string[] fillings;  // Chocolate, Strawberry
    public string[] icings;    // Vanilla, Chocolate
    public string[] toppings;  // Sprinkles, Cherry

    [Header("Locations")]
    public Transform spawnPoint;
    public Transform counterPoint;
    public Transform exitPoint;

    private CustomerController currentCustomerScript;

    void Start()
    {
        SpawnCustomer();
    }

    public void SpawnCustomer()
    {
        // 1. Pick Ingredients
        string shape = shapes[Random.Range(0, shapes.Length)];
        string filling = fillings[Random.Range(0, fillings.Length)];
        string icing = icings[Random.Range(0, icings.Length)];
        string topping = toppings[Random.Range(0, toppings.Length)];

        // 2. Decide Difficulty (Layers)
        int layers = Random.Range(1, 4); 
        string finalOrder = "";

        // --- Difficulty Header ---
        string difficultyHeader = "";
        if (layers == 1) difficultyHeader = "<size=150%><color=green><b>[EASY]</b></color></size>\n";
        else if (layers == 2) difficultyHeader = "<size=150%><color=yellow><b>[MEDIUM]</b></color></size>\n";
        else difficultyHeader = "<size=150%><color=red><b>[HARD]</b></color></size>\n";

        // 3. Construct the Text
        if (layers == 1)
        {
            // FIXED: Added missing '+' after shape
            finalOrder = difficultyHeader + "Single Layer\n" + shape + " Base";
            
            if (filling != "None") finalOrder += "\n" + filling + " Filling";
            finalOrder += "\n" + icing + " Icing";
        }
        else if (layers == 2)
        {
            // FIXED: Added missing '+' after shape
            finalOrder = difficultyHeader + "Double Layer\n" + shape + " Base";
            
            if (filling != "None") finalOrder += "\n" + filling + " Filling";
            finalOrder += "\n" + icing + " Icing";
            if (topping != "None") finalOrder += "\n+ " + topping;
        }
        else // Hard
        {
            finalOrder = difficultyHeader + "Triple Layer\n" + shape + " Base";
            
            if (filling != "None") finalOrder += "\n" + filling + " Filling";
            finalOrder += "\n" + icing + " Icing";
            if (topping != "None") finalOrder += "\n+ " + topping;
        }

        // 4. Spawn & Initialize
        GameObject newObj = Instantiate(characterPrefab, spawnPoint.position, Quaternion.identity);
        currentCustomerScript = newObj.GetComponent<CustomerController>();

        if (currentCustomerScript != null)
        {
            currentCustomerScript.Initialize(counterPoint.position, finalOrder);
        }
    }

    public void CompleteOrder()
    {
        if (currentCustomerScript != null)
        {
            currentCustomerScript.LeaveShop(exitPoint.position);
            currentCustomerScript = null;
            Invoke("SpawnCustomer", 2.0f);
        }
    }
}