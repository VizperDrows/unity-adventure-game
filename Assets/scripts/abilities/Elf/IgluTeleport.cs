using UnityEngine;

public class IgluTeleport : MonoBehaviour
{
    public int igluID;           // assign manually in Inspector
    public Transform spawnPoint;

    private void Awake()
    {
        // automatically find the spawn point inside this Iglu prefab
        spawnPoint = transform.Find("SpawnPoint");
    }
    private void OnTriggerEnter(Collider other)
    {
        playermovement player = other.GetComponent<playermovement>();

        if(player != null  && player.getIsElf())
        {
            IgluManager.Instance.OpenMap(igluID, player);
        }
    }
}
