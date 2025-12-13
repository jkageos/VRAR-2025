using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpBatter : MonoBehaviour
{
    // --- NEW VARIABLE ---
    [Header("Batter Settings")]
    public Color batterColor = Color.white; // Change this in Inspector for each Prefab (Red, Yellow, Brown)

    private bool isHeld = false;
    private Transform holdPoint;
    private Rigidbody rb;
    public BatterSpawner spawner;
    private Collider[] allBatterColliders;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // create a hold point
        holdPoint = new GameObject("HoldPoint").transform;
        holdPoint.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
        holdPoint.SetParent(Camera.main.transform);

        // find all batters
        PickUpBatter[] batters = FindObjectsOfType<PickUpBatter>();
        allBatterColliders = new Collider[batters.Length];
        for (int i = 0; i < batters.Length; i++)
        {
            allBatterColliders[i] = batters[i].GetComponent<Collider>();
        }
    }

    void Update()
    {
        if (isHeld)
        {
            transform.position = holdPoint.position;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            // 1. TRY TO PICK UP (If not held)
            if (!isHeld && Physics.Raycast(ray, out RaycastHit hit, 5f))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    PickupLogic();
                }
            }
            // 2. ACTION (If held) - This is the changed part!
            else if (isHeld)
            {
                // Run a Raycast to see what we are clicking on
                if (Physics.Raycast(ray, out RaycastHit hitTarget, 5f))
                {
                    // Did we hit a Tray?
                    PickUpTray targetTray = hitTarget.collider.GetComponent<PickUpTray>();

                    if (targetTray != null)
                    {
                        // --- APPLY BATTER LOGIC ---
                        targetTray.ReceiveBatter(batterColor);

                        // Tell spawner we are done so it can spawn another if needed
                        if (spawner != null) spawner.ClearCurrentBatter();

                        // Destroy this batter object
                        Destroy(gameObject); 
                        return; // Stop here, do not drop the object physically
                    }
                }

                // If we didn't hit a tray, perform normal physical drop
                DropLogic();
            }
        }
    }

    void PickupLogic()
    {
        isHeld = true;
        rb.isKinematic = true;

        foreach (var col in allBatterColliders)
        {
            if (col != null && col != GetComponent<Collider>())
                Physics.IgnoreCollision(GetComponent<Collider>(), col, true);
        }

        if (spawner != null)
        {
            spawner.ClearCurrentBatter();
            spawner.SpawnNewBatter();
        }
    }

    void DropLogic()
    {
        isHeld = false;
        rb.isKinematic = false;

        foreach (var col in allBatterColliders)
        {
            if (col != null && col != GetComponent<Collider>())
                Physics.IgnoreCollision(GetComponent<Collider>(), col, false);
        }
    }
}