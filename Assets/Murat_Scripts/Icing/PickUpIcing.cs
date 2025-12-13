using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpIcing : MonoBehaviour
{
    [Header("Icing Properties")]
    // --- 1. SET THIS COLOR IN THE INSPECTOR FOR EACH PREFAB ---
    public Color icingColor = Color.white; 
    // ----------------------------------------------------------

    private bool isHeld = false;
    
    [HideInInspector] 
    public bool hasSpawnedReplacement = false;

    private Transform holdPoint;
    private Rigidbody rb;
    private Collider myCollider;

    public IcingSpawner spawner;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();

        if (Camera.main != null)
        {
            holdPoint = new GameObject("IcingHoldPoint").transform;
            holdPoint.position = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
            holdPoint.SetParent(Camera.main.transform);
        }
    }

    void Update()
    {
        if (isHeld)
        {
            // Unity 6 use rb.linearVelocity. Older Unity use rb.velocity.
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
                // PICK UP
                if (Physics.Raycast(ray, out RaycastHit hit, 5f))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        Pickup();
                    }
                }
            }
            else 
            {
                // PLACE ON TRAY
                if (Physics.Raycast(ray, out RaycastHit hitTarget, 5f))
                {
                    PickUpTray targetTray = hitTarget.collider.GetComponentInParent<PickUpTray>();

                    if (targetTray != null)
                    {
                        targetTray.ReceiveIcing(gameObject);
                        return; 
                    }
                }
                DropLogic();
            }
        }
    }

    void Pickup()
    {
        isHeld = true;
        rb.isKinematic = true; 
        if (myCollider != null) myCollider.enabled = false;

        if (!hasSpawnedReplacement)
        {
            hasSpawnedReplacement = true;
            if (spawner != null) spawner.SpawnNewIcing();
        }
    }

    void DropLogic()
    {
        isHeld = false;
        rb.isKinematic = false;
        if (myCollider != null) myCollider.enabled = true;
    }
}