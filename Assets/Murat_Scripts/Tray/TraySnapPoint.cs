using UnityEngine;

public class TraySnapPoint : MonoBehaviour
{
    public Vector3 snapOffset = new Vector3(1.5f, 0.075f, 0f);
    public Transform snapTransform;

    private Renderer trayRenderer;
    private Transform trayTransform;

    void Start()
    {
        if (snapTransform == null)
            snapTransform = transform;

        trayRenderer = GetComponent<Renderer>();
    }

    public void SnapTray(GameObject tray)
    {
        trayTransform = tray.transform;

        // 1. Disable physics
        Rigidbody rb = tray.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 2. Parent to snap position (IMPORTANT!)
        tray.transform.SetParent(snapTransform);

        // 3. Position
        tray.transform.localPosition = snapOffset;

        // 4. Rotation
        float yRotation = snapTransform.eulerAngles.y;
        tray.transform.localRotation = Quaternion.Euler(0, yRotation, 0);
    }

    public bool HasTray()
    {
        return trayTransform != null;
    }

    public void ApplyBatterColor(Color batterColor)
    {
        if (trayRenderer != null)
            trayRenderer.material.color = batterColor;
    }
}