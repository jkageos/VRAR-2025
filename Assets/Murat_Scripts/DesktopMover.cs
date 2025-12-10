using UnityEngine;
using UnityEngine.InputSystem;

public class DesktopMover : MonoBehaviour
{
    public float speed = 3f;
    public float mouseSensitivity = 0.2f;

    float rotationX = 0f;
    Camera cam;

    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        // Optional: lock cursor for mouse look
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Safety: require input devices
        if (Keyboard.current == null || Mouse.current == null)
            return;

        // Mouse look (delta from new Input System)
        Vector2 md = Mouse.current.delta.ReadValue();
        float mouseX = md.x * mouseSensitivity;
        float mouseY = md.y * mouseSensitivity;

        // Horizontal rotation on the rig
        transform.Rotate(0f, mouseX, 0f);

        // Vertical rotation applied to the camera only
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);
        if (cam != null)
            cam.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        // WASD movement using keyboard keys
        Vector2 move = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) move.y += 1f;
        if (Keyboard.current.sKey.isPressed) move.y -= 1f;
        if (Keyboard.current.aKey.isPressed) move.x -= 1f;
        if (Keyboard.current.dKey.isPressed) move.x += 1f;

        Vector3 forward = transform.forward;
        forward.y = 0f;
        Vector3 right = transform.right;
        right.y = 0f;

        Vector3 motion = (forward * move.y + right * move.x);
        if (motion.sqrMagnitude > 1f) motion.Normalize();
        transform.position += motion * speed * Time.deltaTime;
    }
}
