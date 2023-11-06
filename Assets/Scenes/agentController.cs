using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class agentController : MonoBehaviour
{
    public generatePatrol patrol;
    public float patrolUpdate;
    public float speed;

    Vector3 direction;
    Vector3 nextDirection;
    float currentTime;

    public Transform enemy; //need a way to check for enemies
    Vector3 enemyVec;

    //flee variables [tweaked visually]
    Vector3 enemyVecPerp;
    float amp = 2f;
    float freq = 3f;
    float fleeSpeed = 4f;
    float fleeHealth = 25f; //assumes max health 100
    float switchTime;
    float[] switchRange = { 2.5f, 4f };
    bool serpentine = false;
    bool fleeTransition = false;
    bool nextDone = true;
    float fleeTrans = 1f;
    float startTrans;

    //variables altered by test script
    [HideInInspector] public string state;
    [HideInInspector] public bool enemySpotted = false;
    [HideInInspector] public float t = 0f;
    [HideInInspector] public float endFleeTime = 10f;
    [HideInInspector] public float health = 100f;
    agentTest testScript;

    void Start()
    {
        state = "patrol";
        direction = patrol.getDirection(transform.position.x, transform.position.z);
        nextDirection = direction;
        currentTime = 0f;

        testScript = GameObject.Find("Agent").GetComponent<agentTest>();
    }

    void Patrol()
    {
        //updating next direction at time intervals gives the meandering effect
        currentTime += Time.deltaTime;
        if (currentTime >= patrolUpdate) 
        {
            nextDirection = patrol.getDirection(transform.position.x, transform.position.z);
            currentTime = 0f;
        }
        direction += (nextDirection - direction) * Time.deltaTime; //along with slowly updating direction vector with time

        transform.rotation = Quaternion.LookRotation(direction);

        //if very far from object run back
        if (Vector3.Distance(patrol.transform.position, transform.position) > 2 * patrol.length)
        {
            transform.position += 2 * speed * Time.deltaTime * direction;
            patrol.far = true;
        }
        else
        {
            transform.position += speed * Time.deltaTime * direction;
            patrol.far = false;
        }
    }

    void Defend()
    {
        //find and crouch behind cover
        //randomly switch between actions
    }

    void Flee()
    {
        //flee in direction opposite of enemy
        if (t == 0f)
        {
            enemyVec = transform.position - enemy.position;
            enemyVec.y = 0f;
            enemyVec = enemyVec.normalized;
            enemyVecPerp = new Vector3(-enemyVec.z, 0f, enemyVec.x);

            switchTime = t + Random.Range(switchRange[0], switchRange[1]);
        }

        if (t >= switchTime)
        {
            if (serpentine) //simulate look back during serpentine
            {
                enemyVec = transform.position - enemy.position;
                enemyVec.y = 0f;
                enemyVec = enemyVec.normalized;
                enemyVecPerp = new Vector3(-enemyVec.z, 0f, enemyVec.x);
            }
            //reset
            switchTime = t + Random.Range(switchRange[0], switchRange[1]);
            serpentine = !serpentine;
            fleeTransition = true;
            nextDone = false;
        }

        //smoothly transition between serpentine and straight BROKEN
        if (fleeTransition)
        {
            if (!nextDone) //get the next direction
            {
                if (serpentine)
                {
                    nextDirection = amp * Mathf.Sin(freq * (t + fleeTrans)) * enemyVecPerp + enemyVec; //sinusoidal fleeing (aka serpentine)
                }
                else
                {
                    //how do we predict this fleeTime in the future???
                    nextDirection = enemyVec; //straight away from enemy
                }
                startTrans = t;
                nextDone = true;
            }

            direction += (nextDirection - direction) * Time.deltaTime; //smoothly move towards it
            if (t >= startTrans + fleeTrans) { fleeTransition = false; }
        }
        else
        {
            if (serpentine)
            {
                direction = amp * Mathf.Sin(freq * t) * enemyVecPerp + enemyVec; //sinusoidal fleeing (aka serpentine)
            }
            else
            {
                direction = enemyVec; //straight away from enemy
            }
        }
        
        transform.position += fleeSpeed * Time.deltaTime * direction.normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        t += Time.deltaTime;
    }

    string checkState()
    {
        //if enemy spotted then defend
        if (enemySpotted & health > fleeHealth)
        {
            state = "defend";
        }

        //if health too low then flee
        if (health <= fleeHealth & t <= endFleeTime)
        {
            state = "flee";
        }

        //if fleeing for too long return to patrol
        if (t > endFleeTime)
        {
            t = 0f;
            state = "patrol";

            testScript.flee = false;
        }

        return state;
    }

    void Update()
    {
        state = checkState();
        switch (state)
        {
            case "patrol":
                Patrol();
                break;
            case "defend":
                Defend();
                break;
            case "flee":
                Flee();
                break;
        }
    }
}
