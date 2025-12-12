using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpIcing : MonoBehaviour
{
    private bool isHeld = false;

    [HideInInspector]
    public bool hasSpawnedReplacement = false; // <- make it public

    private Transform holdPoint;
    private Rigidbody rb;
    public IcingSpawner spawner;
    private Collider[] allIcingColliders;

    private Renderer icingRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        icingRenderer = GetComponent<Renderer>();

        // Create a hold point
        holdPoint = new GameObject("IcingHoldPoint").transform;
        holdPoint.position = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
        holdPoint.SetParent(Camera.main.transform);

        // Find all icing objects to disable collisions between held icing
        PickUpIcing[] icings = FindObjectsOfType<PickUpIcing>();
        allIcingColliders = new Collider[icings.Length];
        for (int i = 0; i < icings.Length; i++)
        {
            if (icings[i].GetComponent<Collider>() != null)
                allIcingColliders[i] = icings[i].GetComponent<Collider>();
        }
    }

    void Update()
    {
        if (isHeld)
            transform.position = holdPoint.position;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            // PICK UP
            if (!isHeld && Physics.Raycast(ray, out RaycastHit hit, 5f))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    isHeld = true;
                    rb.isKinematic = true;

                    foreach (var col in allIcingColliders)
                        if (col != GetComponent<Collider>())
                            Physics.IgnoreCollision(GetComponent<Collider>(), col, true);

                    // Spawn replacement only once
                    if (!hasSpawnedReplacement)
                    {
                        hasSpawnedReplacement = true;
                        if (spawner != null)
                            spawner.SpawnNewIcing();
                    }
                }
            }
            // DROP
            else if (isHeld)
            {
                isHeld = false;
                rb.isKinematic = false;

                foreach (var col in allIcingColliders)
                    if (col != GetComponent<Collider>())
                        Physics.IgnoreCollision(GetComponent<Collider>(), col, false);
            }
        }
    }
}
