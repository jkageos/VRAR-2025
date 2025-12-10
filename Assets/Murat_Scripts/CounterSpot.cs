using UnityEngine;

public class CounterSpot : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tray"))
        {
            other.transform.position = transform.position;
            other.GetComponent<Rigidbody>().isKinematic = true; // lock in place
        }
    }
}
