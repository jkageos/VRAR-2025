using UnityEngine;

public class IcingSpawner : MonoBehaviour
{
    public GameObject icingPrefab;
    private GameObject currentIcing;
    public Transform spawnPoint;

    void Start()
    {
        // Force upright rotation on first spawn
        Quaternion uprightRotation = Quaternion.Euler(0, 0, 0);

        // Spawn the first icing
        currentIcing = Instantiate(icingPrefab, spawnPoint.position, uprightRotation);

        // Assign spawner and reset hasSpawnedReplacement
        PickUpIcing pickup = currentIcing.GetComponent<PickUpIcing>();
        if (pickup != null)
        {
            pickup.spawner = this;
            pickup.hasSpawnedReplacement = false;
        }
    }

    public void SpawnNewIcing()
    {
        if (icingPrefab == null) return;

        // Force upright rotation for every new icing
        Quaternion uprightRotation = Quaternion.Euler(0, 0, 0);

        // Spawn the new icing
        currentIcing = Instantiate(icingPrefab, spawnPoint.position, uprightRotation);

        // Assign spawner and reset hasSpawnedReplacement
        PickUpIcing pickup = currentIcing.GetComponent<PickUpIcing>();
        if (pickup != null)
        {
            pickup.spawner = this;
            pickup.hasSpawnedReplacement = false;
        }
    }
}
