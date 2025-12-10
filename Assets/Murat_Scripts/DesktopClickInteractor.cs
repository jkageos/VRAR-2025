using UnityEngine;

public class DesktopClickInteractor : MonoBehaviour
{
    public Camera cam; // assign Main Camera
    public float maxDistance = 5f;
    public LayerMask interactableLayer;

    void Update()
    {
        if (cam == null) return;

        // Check for left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactableLayer))
            {
                // Try to call "OnClick" method on the hit object
                var interactable = hit.collider.GetComponent<IDesktopInteractable>();
                if (interactable != null)
                    interactable.OnClick();
            }
        }
    }
}

// Interface for your objects
public interface IDesktopInteractable
{
    void OnClick();
}
