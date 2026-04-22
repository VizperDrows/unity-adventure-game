using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterScript : MonoBehaviour
{
    CharacterController myCharacter;
    [SerializeField] Vector3 move = new Vector3(0, 0, 0);
    //[SerializeField] float time = 0f;
    [SerializeField] Vector3 attackedDir = new Vector3(0, 0, 0);
    float horizontalIn = 0f;
    float verticalIn = 0f;
    float ySpeed = 0f;
    //bool hasGravity = false;
    //bool hadGravity = false;
    bool attacked = false;
    //bool resetAtk = false;

    public float movementSpeed = 5;
    public float jumpSpeed = 5f;
    public float timeDelayYReset;
    public float ySpeedReset = -.1f;
    public bool isDead = false;


    // Start is called before the first frame update
    void Start()
    {
        myCharacter = GetComponent<CharacterController>();
        movementSpeed = PlayerPrefs.GetFloat("speed");
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead) {
            if (Input.GetButton("Vertical"))
            {
                Debug.Log(1);
                verticalIn = movementSpeed;
            }
            else verticalIn = 0;
            horizontalIn = Input.GetAxis("Horizontal");
            //verticalIn = Input.GetAxis("Vertical");
        }
        else
        {
            horizontalIn = 0f;
            verticalIn = 0f;
        }

        if (!attacked)
        {
            move = new Vector3(verticalIn, ySpeed, -horizontalIn);
        }
        else
        {
            move = new Vector3(attackedDir.x, ySpeed, attackedDir.z);
        }
        myCharacter.Move(move * Time.deltaTime*movementSpeed);
    }
}
