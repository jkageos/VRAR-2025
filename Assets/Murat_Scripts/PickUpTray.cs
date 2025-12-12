using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpTray : MonoBehaviour
{
    private bool isHeld = false;
    private bool hasSpawnedReplacement = false;

    private Transform holdPoint;
    private Rigidbody rb;
    public TraySpawner spawner;
    private Collider[] allTrayColliders;
    private Renderer trayRenderer;

    [Header("Icing Layer Settings")]
    public GameObject icingLayerPrefab;
    public float surfaceOffset = 0.005f;
    public float icingThickness = 0.02f;

    private int icingLayerCount = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        trayRenderer = GetComponent<Renderer>();

        if (Camera.main != null)
        {
            holdPoint = new GameObject("TrayHoldPoint").transform;
            holdPoint.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
            holdPoint.SetParent(Camera.main.transform);
        }

        PickUpTray[] trays = FindObjectsOfType<PickUpTray>();
        allTrayColliders = new Collider[trays.Length];
        for (int i = 0; i < trays.Length; i++)
        {
            if (trays[i].GetComponent<Collider>() != null)
                allTrayColliders[i] = trays[i].GetComponent<Collider>();
        }
    }

    // --- BATTER LOGIC ---
    public void ReceiveBatter(Color batterColor)
    {
        if (trayRenderer != null)
            trayRenderer.material.color = batterColor;
    }

    // --- ICING LOGIC ---
    public void ReceiveIcing(GameObject icingSource)
    {
        if (icingSource == null) return;

        // 1. GET COLOR FROM SCRIPT
        Color colorToApply = Color.white;
        PickUpIcing icingScript = icingSource.GetComponent<PickUpIcing>();

        if (icingScript != null)
        {
            // USE THE COLOR YOU SET IN THE INSPECTOR
            colorToApply = icingScript.icingColor;
        }
        else
        {
            // Fallback (try to guess from mesh)
            Renderer sourceRend = icingSource.GetComponentInChildren<Renderer>();
            if (sourceRend != null) colorToApply = sourceRend.material.color;
        }

        Destroy(icingSource);

        if (icingLayerPrefab == null) return;

        // 2. INSTANTIATE
        GameObject newLayer = Instantiate(icingLayerPrefab, transform);
        if (newLayer.GetComponent<Rigidbody>()) Destroy(newLayer.GetComponent<Rigidbody>());

        // 3. SCALE FIX (Anti-Squish)
        Vector3 desiredScale = icingLayerPrefab.transform.localScale;
        Vector3 parentScale = transform.localScale;

        newLayer.transform.localScale = new Vector3(
            desiredScale.x / parentScale.x,
            desiredScale.y / parentScale.y,
            desiredScale.z / parentScale.z
        );

        // 4. POSITION FIX (Surface Detection)
        float trayTopY = 0.5f;
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null) trayTopY = mf.mesh.bounds.extents.y;

        float heightPerLayer = icingThickness / parentScale.y;
        float startHeight = trayTopY + (surfaceOffset / parentScale.y);
        float finalY = startHeight + (icingLayerCount * heightPerLayer);

        newLayer.transform.localPosition = new Vector3(0, finalY, 0);
        newLayer.transform.localRotation = Quaternion.identity;

        // 5. APPLY COLOR
        Renderer layerRend = newLayer.GetComponentInChildren<Renderer>();
        if (layerRend != null)
        {
            layerRend.material.color = colorToApply;
        }

        icingLayerCount++;
    }

    void Update()
    {
        if (isHeld)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            transform.position = Vector3.Lerp(transform.position, holdPoint.position, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Lerp(transform.rotation, holdPoint.rotation, Time.deltaTime * 10f);
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!isHeld)
            {
                if (Physics.Raycast(ray, out RaycastHit hit, 5f))
                {
                    if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    {
                        Pickup();
                    }
                }
            }
            else
            {
                Drop();
            }
        }
    }

    void Pickup()
    {
        isHeld = true;
        rb.isKinematic = true;

        foreach (var col in allTrayColliders)
            if (col != null && GetComponent<Collider>() != null)
                Physics.IgnoreCollision(GetComponent<Collider>(), col, true);

        if (!hasSpawnedReplacement && spawner != null)
        {
            hasSpawnedReplacement = true;
            spawner.SpawnNewTray();
        }
    }

    void Drop()
    {
        isHeld = false;
        rb.isKinematic = false;

        foreach (var col in allTrayColliders)
            if (col != null && GetComponent<Collider>() != null)
                Physics.IgnoreCollision(GetComponent<Collider>(), col, false);

        SnapToClosestPoint();
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