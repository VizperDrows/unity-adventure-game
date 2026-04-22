using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class npcmove : MonoBehaviour
{
    public NavMeshAgent agent;

    public float walkpointrange;
    public Vector3 walkpoint;

    //[SerializeField] private Transform movePositionTransform;

    float time = 0f;
    public float timedelay; //param that sets the time delay before to start searching for another location
    float randomZ;
    float randomX;
    public Vector3 initpos;
    bool walkpointset = false;

    //variables para anim 
    Vector3 lastpos;
    Vector3 dif;
    SpriteRenderer sprite;
    Animator anim;
    //

    private void Awake()
    {
        //referencia para navmeshagent
        agent = GetComponent<NavMeshAgent>();
        //

        initpos = transform.position;

        //para anim
        lastpos = transform.position;
        sprite = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        //
    }



    // Update is called once per frame
    void FixedUpdate()
    {   
        //agent.destination = movePositionTransform.position;

        //buscar walkpoint
        if (!walkpointset)
        {
            randomZ = Random.Range(-walkpointrange, walkpointrange);
            randomX = Random.Range(-walkpointrange, walkpointrange);

            walkpoint = new Vector3(initpos.x + randomX, initpos.y, initpos.z + randomZ);
            agent.destination = walkpoint; //this line is to move agent
            walkpointset = true;
        }
        //

        /*
        if (sprite == null) Debug.Log("Sprite doesn't exists");
        else Debug.Log("Sprite exists");
        if (anim == null) Debug.Log("Anim doesn't exists");
        else Debug.Log("Anim exists");*/

        //para anim
        dif = transform.position - lastpos;

        if (dif.z > 0)
        {
            lastpos = transform.position;
            anim.SetFloat("blend", 1);
            sprite.flipX = true;
        }
        else if (dif.z < 0)
        {
            lastpos = transform.position;
            anim.SetFloat("blend", 1);
            sprite.flipX = false;
        }
        else if (dif.z == 0)
        { 
            anim.SetFloat("blend", 0);
            //

            //wait for delay to start searching for walkpoint again
            if (time <= timedelay)
            {
                time = time + Time.deltaTime; 
            } else
            {
                walkpointset = false;
                time = 0f;
            }
            //
        } 

    }
}
