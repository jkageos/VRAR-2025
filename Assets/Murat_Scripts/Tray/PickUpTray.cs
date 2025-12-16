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

    // Changed to Public so we can calculate stack height
    public int icingLayerCount = 0;

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

        PickUpTray[] trays = FindObjectsByType<PickUpTray>(FindObjectsSortMode.None);
        allTrayColliders = new Collider[trays.Length];
        for (int i = 0; i < trays.Length; i++)
        {
            if (trays[i].GetComponent<Collider>() != null)
                allTrayColliders[i] = trays[i].GetComponent<Collider>();
        }
    }

    // --- HELPER: Calculate where the top of the stack is ---
    public float GetLocalStackTop()
    {
        float trayTopY = 0.5f;
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null) trayTopY = mf.mesh.bounds.extents.y;

        Vector3 parentScale = transform.localScale;

        // Base Mesh Height + Surface Offset + All Icing Layers
        float totalHeight = trayTopY + (surfaceOffset / parentScale.y) + (icingLayerCount * (icingThickness / parentScale.y));
        return totalHeight;
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

        Color colorToApply = Color.white;
        PickUpIcing icingScript = icingSource.GetComponent<PickUpIcing>();

        if (icingScript != null) colorToApply = icingScript.icingColor;
        else
        {
            Renderer sourceRend = icingSource.GetComponentInChildren<Renderer>();
            if (sourceRend != null) colorToApply = sourceRend.material.color;
        }

        Destroy(icingSource);

        if (icingLayerPrefab == null) return;

        GameObject newLayer = Instantiate(icingLayerPrefab, transform);
        if (newLayer.GetComponent<Rigidbody>()) Destroy(newLayer.GetComponent<Rigidbody>());

        // Anti-Squish Scale
        Vector3 desiredScale = icingLayerPrefab.transform.localScale;
        Vector3 parentScale = transform.localScale;
        newLayer.transform.localScale = new Vector3(
            desiredScale.x / parentScale.x,
            desiredScale.y / parentScale.y,
            desiredScale.z / parentScale.z
        );

        // Position Logic
        float trayTopY = 0.5f;
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null) trayTopY = mf.mesh.bounds.extents.y;

        float heightPerLayer = icingThickness / parentScale.y;
        float startHeight = trayTopY + (surfaceOffset / parentScale.y);
        float finalY = startHeight + (icingLayerCount * heightPerLayer);

        newLayer.transform.localPosition = new Vector3(0, finalY, 0);
        newLayer.transform.localRotation = Quaternion.identity;

        Renderer layerRend = newLayer.GetComponentInChildren<Renderer>();
        if (layerRend != null) layerRend.material.color = colorToApply;

        icingLayerCount++;
    }

    void Update()
    {
        // 1. Prevent moving if we are the child of another tray (bottom tray handles movement)
        if (transform.parent != null && transform.parent.GetComponent<PickUpTray>() != null)
        {
            return;
        }

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
                // PICKUP
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
                // DROP / STACK
                // First, check if we are aiming at another tray
                if (Physics.Raycast(ray, out RaycastHit hitTarget, 5f))
                {
                    PickUpTray targetTray = hitTarget.collider.GetComponentInParent<PickUpTray>();

                    // If we found a tray, and it's not ourselves
                    if (targetTray != null && targetTray != this)
                    {
                        StackOntoTray(targetTray);
                        return;
                    }
                }

                // If no tray found, do normal drop
                Drop();
            }
        }
    }

    // --- NEW STACKING LOGIC ---
    void StackOntoTray(PickUpTray targetBase)
    {
        isHeld = false;

        // 1. Parent to the target
        transform.SetParent(targetBase.transform);

        // 2. SCALE FIX (Anti-Squish for Tray)
        // If the base tray is Scale(0.5), we need to become Scale(2.0) to look normal.
        Vector3 parentScale = targetBase.transform.localScale;
        transform.localScale = new Vector3(
            1f / parentScale.x,
            1f / parentScale.y,
            1f / parentScale.z
        );

        // 3. POSITION FIX
        // Ask the base tray how high its stack is
        float baseTopY = targetBase.GetLocalStackTop();

        // Get our own half-height
        float myHalfHeight = 0.5f;
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null) myHalfHeight = mf.mesh.bounds.extents.y;

        // Calculate final Y local position
        float finalLocalY = baseTopY + (myHalfHeight / parentScale.y);

        transform.localPosition = new Vector3(0, finalLocalY, 0);

        // 4. ROTATION FIX
        // Reset rotation to align perfectly with the base tray (flattens it)
        transform.localRotation = Quaternion.identity;

        // 5. PHYSICS LOCK
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 6. COLLISION IGNORE
        // We must ignore collision with the tray beneath us, or physics will push us off
        foreach (var col in allTrayColliders)
        {
            if (col != null && GetComponent<Collider>() != null)
            {
                if (col.gameObject == targetBase.gameObject)
                {
                    Physics.IgnoreCollision(GetComponent<Collider>(), col, true);
                }
                else
                {
                    Physics.IgnoreCollision(GetComponent<Collider>(), col, false);
                }
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
        TraySnapPoint[] snapPoints = FindObjectsByType<TraySnapPoint>(FindObjectsSortMode.None);
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