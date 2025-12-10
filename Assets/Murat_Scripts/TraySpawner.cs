using UnityEngine;

public class TraySpawner : MonoBehaviour
{
    public GameObject trayPrefab; // assign the correct prefab in Inspector
    private GameObject currentTray;

    void Start()
    {
        SpawnNewTray();
    }

    public void SpawnNewTray()
    {
        if (trayPrefab == null) return;

        currentTray = Instantiate(trayPrefab, transform.position, transform.rotation);

        // tell the tray who spawned it
        PickUpTray pickup = currentTray.GetComponent<PickUpTray>();
        if (pickup != null)
        {
            pickup.spawner = this;
        }
    }
}
