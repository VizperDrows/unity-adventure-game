using UnityEngine;

//This script is attached to the tree prefab 
//It spawns a gift pickup at one of the two spawn points (left or right)
// when the game starts.
// It also has a method to convert the tree into a decorated one with lights (called from the trunk's lightbulb ability)
public class TreeBehavior : MonoBehaviour
{
    
    [Header("Spawn Gift Settings")]
    public GameObject giftPickupPrefab;
    public float floatHeight = 0.5f;
    public LayerMask groundLayer;
    private GameObject leftGift;
    private GameObject rightGift;
    private Transform leftSpawnPoint;
    private Transform rightSpawnPoint;

    [Header("Decoration")]
    public GameObject lightbulbStrip;
    private bool converted = false;
    public Mesh decoratedMesh;
    public Material[] decoratedMaterials;
    private MeshFilter mf;
    private MeshRenderer mr;

    void Start()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();

        if (giftPickupPrefab == null)
        {
            Debug.LogWarning("GiftPickupPrefab not assigned on " + gameObject.name);
            return;
        }

        // Find spawn points
        leftSpawnPoint = transform.Find("spawnGiftLeft");
        rightSpawnPoint = transform.Find("spawnGiftRight");

        if (leftSpawnPoint == null || rightSpawnPoint == null)
        {
            Debug.LogWarning("Missing spawnGiftLeft or spawnGiftRight on " + gameObject.name);
            return;
        }

        // Pick one randomly
        if (Random.value < 0.5f)
        {
            leftGift = SpawnGiftAt(leftSpawnPoint);
        }
        else
        {
            rightGift = SpawnGiftAt(rightSpawnPoint);
        }
    }

    GameObject SpawnGiftAt(Transform spawnPoint)
    {
        Vector3 spawnPos = spawnPoint.position;

        // Raycast down to find ground
        RaycastHit hit;
        if (Physics.Raycast(spawnPos + Vector3.up * 10f, Vector3.down, out hit, 100f, groundLayer))
        {
            spawnPos.y = hit.point.y + floatHeight;
        }

        // Spawn gift
        GameObject gift = Instantiate(giftPickupPrefab, spawnPos, Quaternion.identity);

        // Optional: parent to tree
        gift.transform.parent = transform;

        // ASSIGN TREE + SLOT INFO
        GiftScript gs = gift.GetComponent<GiftScript>();
        if (gs != null)
        {
            gs.parentTree = this;
            gs.isLeftSlot = (spawnPoint == leftSpawnPoint);
        }

        return gift;
    }

    public void ConvertTree()
    {
        if (converted) return;

        // Enable lightbulb strip
        if (lightbulbStrip != null)
            lightbulbStrip.SetActive(true);

        // Swap actual tree mesh
        if (mf != null && decoratedMesh != null)
        {
            mf.mesh = decoratedMesh;
        }

        // Swap actual tree materials
        if (mr != null && decoratedMaterials != null && decoratedMaterials.Length > 0)
        {
            mr.materials = decoratedMaterials;
        }


        converted = true;

        // Fill missing gift slots
        if (leftGift == null)
        {
            leftGift = SpawnGiftAt(leftSpawnPoint);
        }

        if (rightGift == null)
        {
            rightGift = SpawnGiftAt(rightSpawnPoint);
        }
    }

    public void ClearLeft()
    {
        leftGift = null;
    }

    public void ClearRight()
    {
        rightGift = null;
    }
}
