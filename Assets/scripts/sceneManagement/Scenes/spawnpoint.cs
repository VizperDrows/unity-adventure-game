using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnpoint : MonoBehaviour
{

    public GameObject ch;

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(ch, transform.position, Quaternion.Euler(45,90,0));
        SantaController.Instance.SetPlayer(ch.transform);
    }

}
