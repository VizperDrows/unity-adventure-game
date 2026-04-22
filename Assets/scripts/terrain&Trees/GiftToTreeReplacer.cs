#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class GiftToTreeReplacer : MonoBehaviour
{
    [Header("Tree Prefabs")]
    public GameObject treeB_Prefab;
    public GameObject treeM_Prefab;
    public GameObject treeS_Prefab;

    [Header("Setup")]
    public string giftTag = "GiftPickup"; // Tag for your gift pickups
    public Terrain terrain;

    [ContextMenu("Replace Gifts With Random Trees")]
    public void ReplaceGifts()
    {
        if (terrain == null || treeB_Prefab == null || treeM_Prefab == null || treeS_Prefab == null)
        {
            Debug.LogWarning("Assign terrain and all tree prefabs!");
            return;
        }

        GameObject[] gifts = GameObject.FindGameObjectsWithTag(giftTag);

        if (gifts.Length == 0)
        {
            Debug.LogWarning("No gift pickups found with tag: " + giftTag);
            return;
        }

        foreach (GameObject gift in gifts)
        {
            Vector3 pos = gift.transform.position;

            // Snap to terrain
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;

            // Pick random tree type
            int randomIndex = Random.Range(0, 3);
            GameObject selectedPrefab = null;

            switch (randomIndex)
            {
                case 0:
                    selectedPrefab = treeB_Prefab;
                    break;
                case 1:
                    selectedPrefab = treeM_Prefab;
                    break;
                case 2:
                    selectedPrefab = treeS_Prefab;
                    break;
            }

            // Instantiate prefab (EDITOR SAFE)
            GameObject tree = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            tree.transform.position = pos;

            // Optional: organize hierarchy
            tree.transform.parent = this.transform;

            // Remove gift permanently
            DestroyImmediate(gift);
        }

        Debug.Log($"Replaced {gifts.Length} gifts with random trees.");
    }
}
#endif
