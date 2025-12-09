using UnityEngine;

public class TraySnapPoint : MonoBehaviour
{
    public Vector3 snapOffset = new Vector3(1.5f, 0.075f, 0f);
    public Transform snapTransform; // Assign in inspector

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
        tray.transform.position = snapTransform.position + snapOffset;
        tray.transform.rotation = snapTransform.rotation;
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
