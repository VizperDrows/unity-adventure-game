using UnityEngine;
using UnityEngine.UI;

public class IgluMapButton : MonoBehaviour
{
    public int igluID; // the iglu this button represents

    private void Awake()
    {
        // Automatically hook into the button component
        GetComponent<Button>().onClick.AddListener(() =>
        {
            IgluManager.Instance.SelectIglu(igluID);
        });
    }
}
