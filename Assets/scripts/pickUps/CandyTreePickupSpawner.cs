using UnityEngine;

public class CandyTreePickupSpawner : MonoBehaviour
{
    public GameObject candyPickupPrefab;  // Assign your candy pickup prefab here
    public LayerMask groundLayer;         // Assign terrain or ground layer(s)
    public float floatHeight = 1f;        // How much above ground it should float


    void Start()
    {
        if (candyPickupPrefab == null)
        {
            Debug.LogWarning("CandyPickupPrefab not assigned on " + gameObject.name);
            return;
        }

        // Find the child named "pickUpSpawn"
        Transform spawnPoint = transform.Find("pickUpSpawn");
        if (spawnPoint == null)
        {
            Debug.LogWarning("No child named 'pickUpSpawn' found in " + gameObject.name);
            return;
        }

        // Calculate spawn position
        Vector3 spawnPos = spawnPoint.position;
        // Raycast downward to find the ground
        RaycastHit hit;
        if (Physics.Raycast(spawnPos + Vector3.up * 10f, Vector3.down, out hit, 100f, groundLayer))
        {
            spawnPos.y = hit.point.y + floatHeight;
        }
        else
        {
            Debug.LogWarning("No ground hit found for " + gameObject.name + " pickup. Using original Y.");
        }

        // Instantiate candy pickup at the spawn point
        GameObject pickup = Instantiate(candyPickupPrefab, spawnPos, Quaternion.identity);

        // Optional: make it a child of the tree for organization
        pickup.transform.parent = transform;
    }
}
