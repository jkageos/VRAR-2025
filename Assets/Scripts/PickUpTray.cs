using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpTray : MonoBehaviour
{
    private bool isHeld = false;
    private bool hasSpawnedReplacement = false;   // <-- FIX HERE

    private Transform holdPoint;
    private Rigidbody rb;
    public TraySpawner spawner;
    private Collider[] allTrayColliders;

    private Renderer trayRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        trayRenderer = GetComponent<Renderer>();

        holdPoint = new GameObject("HoldPoint").transform;
        holdPoint.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
        holdPoint.SetParent(Camera.main.transform);

        Tray[] trays = FindObjectsOfType<Tray>();
        allTrayColliders = new Collider[trays.Length];
        for (int i = 0; i < trays.Length; i++)
        {
            if (trays[i].GetComponent<Collider>() != null)
                allTrayColliders[i] = trays[i].GetComponent<Collider>();
        }
    }

    // Called by batter
    public void ReceiveBatter(Color batterColor)
    {
        if (trayRenderer != null)
            trayRenderer.material.color = batterColor;
    }

    void Update()
    {
        if (isHeld)
            transform.position = holdPoint.position;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!isHeld && Physics.Raycast(ray, out RaycastHit hit, 5f))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    // PICK UP
                    isHeld = true;
                    rb.isKinematic = true;

                    foreach (var col in allTrayColliders)
                        if (col != GetComponent<Collider>())
                            Physics.IgnoreCollision(GetComponent<Collider>(), col, true);

                    // -------------------------------
                    // FIX: spawn only ONCE per tray
                    // -------------------------------
                    if (!hasSpawnedReplacement)
                    {
                        hasSpawnedReplacement = true;
                        if (spawner != null)
                            spawner.SpawnNewTray();
                    }
                }
            }
            else if (isHeld)
            {
                // DROP tray
                isHeld = false;
                rb.isKinematic = false;

                foreach (var col in allTrayColliders)
                    if (col != GetComponent<Collider>())
                        Physics.IgnoreCollision(GetComponent<Collider>(), col, false);

                SnapToClosestPoint();
            }
        }
    }

    void SnapToClosestPoint()
    {
        TraySnapPoint[] snapPoints = FindObjectsOfType<TraySnapPoint>();
        TraySnapPoint closest = null;
        float minDist = Mathf.Infinity;

        foreach (var snap in snapPoints)
        {
            float dist = Vector3.Distance(transform.position, snap.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = snap;
            }
        }

        if (closest != null && minDist <= 1f)
        {
            closest.SnapTray(gameObject);
        }
    }
}
