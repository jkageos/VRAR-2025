using UnityEngine;
using TMPro; // Needed for TextMeshPro

public class CustomerController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 3f;
    public float stopDistance = 0.5f;

    [Header("References")]
    public TMP_Text orderTextRenderer; // This looks for TEXT now
    public GameObject orderCanvas;     
    public Animator myAnimator;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool orderCompleted = false;

    // --- THIS IS THE FIX ---
    // We changed 'Sprite orderSprite' to 'string orderDescription'
    public void Initialize(Vector3 destination, string orderDescription)
    {
        targetPosition = destination;

        // Set the text on the bubble
        if (orderTextRenderer != null)
            orderTextRenderer.text = orderDescription;

        // Hide bubble initially
        if (orderCanvas != null) orderCanvas.SetActive(false);

        isMoving = true;
        if (myAnimator) myAnimator.SetBool("isWalking", true);
    }
    // -----------------------

    void Update()
    {
        if (isMoving)
        {
            // Look target logic to prevent tilting
            Vector3 lookTarget = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
            
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.LookAt(lookTarget);

            if (Vector3.Distance(transform.position, targetPosition) < stopDistance)
            {
                if (!orderCompleted) ArrivedAtCounter();
                else Destroy(gameObject);
            }
        }
    }

    void ArrivedAtCounter()
    {
        isMoving = false;
        if (myAnimator) myAnimator.SetBool("isWalking", false);
        if (orderCanvas != null) orderCanvas.SetActive(true);
    }

    public void LeaveShop(Vector3 exitPos)
    {
        if (orderCanvas != null) orderCanvas.SetActive(false);
        orderCompleted = true;
        targetPosition = exitPos;
        isMoving = true;
        if (myAnimator) myAnimator.SetBool("isWalking", true);
    }
}