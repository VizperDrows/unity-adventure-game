#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class CandyTreeSpawner : MonoBehaviour
{
    public GameObject candyTreePrefab; // Assign your candy tree prefab here
    public string candyTreeTag = "CandyTree"; // Tag your old candy trees
    public Terrain terrain;

    [Header("Randomization Settings")]
    public Vector2 rotationRange = new Vector2(0f, 360f); // random Y rotation
    public Vector2 scaleRange = new Vector2(0.6f, 1.25f);  // random scale multiplier

    [ContextMenu("Replace Existing Candy Trees")]
    public void ReplaceCandyTrees()
    {
        if (candyTreePrefab == null || terrain == null)
        {
            Debug.LogWarning("Assign candyTreePrefab and terrain first!");
            return;
        }

         // Find all old candy trees by tag
        GameObject[] oldTrees = GameObject.FindGameObjectsWithTag(candyTreeTag);
        if (oldTrees.Length == 0)
        {
            Debug.LogWarning("No candy trees found with tag: " + candyTreeTag);
            return;
        }

        foreach (GameObject oldTree in oldTrees)
        {
            Vector3 pos = oldTree.transform.position;

            // Stick to terrain height
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;

            // Random rotation
            float rotY = Random.Range(rotationRange.x, rotationRange.y);
            Quaternion rotation = Quaternion.Euler(0, rotY, 0);

            // Random scale
            float scale = Random.Range(scaleRange.x, scaleRange.y);

            // Instantiate in editor
            GameObject tree = (GameObject)PrefabUtility.InstantiatePrefab(candyTreePrefab);
            tree.transform.position = pos;
            tree.transform.rotation = rotation;
            tree.transform.localScale = new Vector3(scale, scale, scale);

            // Optionally parent under this spawner for organization
            tree.transform.parent = this.transform;

            // Destroy the pickup permanently
            DestroyImmediate(oldTree);
        }

        Debug.Log($"Replaced {oldTrees.Length} candy trees with new scaled candy trees.");
    }
}
#endif