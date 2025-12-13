using UnityEngine;

public class BatterSpawner : MonoBehaviour
{
    public GameObject batterPrefab; // assign in Inspector
    private GameObject currentBatter;

    void Start()
    {
        SpawnNewBatter();
    }

    public void SpawnNewBatter()
    {
        // Only spawn if there isn’t a batter already
        if (batterPrefab == null) return;
        if (currentBatter != null) return; // <-- prevents multiple spawning

        // Instantiate the batter prefab
        currentBatter = Instantiate(batterPrefab, transform.position, transform.rotation);

        // Assign this spawner to the pickup script
        PickUpBatter pickup = currentBatter.GetComponent<PickUpBatter>();
        if (pickup != null)
        {
            pickup.spawner = this;
        }
    }

    // Optional: Call this when batter is picked up/destroyed
    public void ClearCurrentBatter()
    {
        currentBatter = null;
    }
}
