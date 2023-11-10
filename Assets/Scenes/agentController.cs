using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class agentController : MonoBehaviour
{
    public Transform enemy;
    Vector3 enemyVec;

    //patrol variables
    public generatePatrol patrol;
    public float patrolUpdate;
    public float speed;
    float currentTime;
    Vector3 direction;
    Vector3 nextDirection;
    float visionRadius = 25f;
    float visionAngle = 35f;

    //defend variables
    float distFromCover;
    float enemyAngle;
    Vector3 destination;
    TextMeshProUGUI[] defenseActions;
    bool shoot = false;
    float dTime = 0f;
    int shootDir;
    float shootTime = 3f;
    float shootStart = 0f;
    RaycastHit objHit;
    float crouchSpeed;
    float findCoverUpdate = 0.2f;
    float payloadAngle;
    float destAngle;
    bool revolveDir;

    //flee variables [tweak visually]
    Vector3 enemyVecPerp;
    static float AMP = 2f;
    float amp = AMP;
    float freq = 3f;
    float fleeSpeed = 4f;
    float fleeHealth = 25f; //assumes max health 100
    float switchTime;
    float[] switchRange = { 2f, 3.5f };
    bool serpentine = false;
    bool fleeTransition = false;
    float transTime = 1f;
    float startTrans;

    //variables altered by test script
    [HideInInspector] public string state;
    [HideInInspector] public bool enemySpotted;
    [HideInInspector] public float t;
    [HideInInspector] public float endFleeTime;
    [HideInInspector] public float health;
    agentTest testScript;

    void Start()
    {
        state = "patrol";
        direction = patrol.getDirection(transform.position.x, transform.position.z, false);
        nextDirection = direction;
        currentTime = 0f;

        enemySpotted = false;
        t = 0f;
        endFleeTime = 10f;
        health = 100f;

        destination = new Vector3(0f, 0f, 0f);
        //distFromCover = Mathf.Sqrt(2 * Mathf.Pow(patrol.len, 2)) + GetComponent<CapsuleCollider>().radius;
        distFromCover = 2f + 0.5f;
        defenseActions = gameObject.GetComponentsInChildren<TextMeshProUGUI>();
        defenseActions[0].color = new Color(defenseActions[0].color.r, defenseActions[0].color.g, defenseActions[0].color.b, 0f);
        defenseActions[1].color = new Color(defenseActions[1].color.r, defenseActions[1].color.g, defenseActions[1].color.b, 0f);
        crouchSpeed = 0.75f * speed;

        testScript = GameObject.Find("Agent").GetComponent<agentTest>();
    }

    void Patrol()
    {
        //updating next direction at time intervals gives the meandering effect
        currentTime += Time.deltaTime;
        if (currentTime >= patrolUpdate) 
        {
            nextDirection = patrol.getDirection(transform.position.x, transform.position.z, false);
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

        //check for enemies
        if (testScript.enemySpawned) //need to rework to implement in a game
        {
            if (!enemySpotted)
            {
                if (Mathf.Pow(enemy.position.x, 2) + Mathf.Pow(enemy.position.z, 2) <= Mathf.Pow(visionRadius, 2)) //doesn't seem scalable to multiple enemies in the scene
                {
                    if (Vector3.Angle(transform.forward, enemy.position) <= visionAngle)
                    {
                        enemySpotted = true;
                    }
                }
            }
        }
    }

    void Defend()
    {
        if (!shoot) //crouch behind cover
        {
            if (dTime == 0f)
            {
                //find cover
                enemyAngle = Mathf.Atan2(enemy.position.x - patrol.transform.position.x, enemy.position.z - patrol.transform.position.z);
                destination.x = -distFromCover * Mathf.Cos(enemyAngle);
                destination.z = -distFromCover * Mathf.Sin(enemyAngle);

                //4.5f is the raidus of the circle that approximates the line (0.25x)^4 + (0.25z)^4 = 1 (SWITCH TO THIS CIRCLE?)
                //assumes payload is at origin
                destination.x = (destination.x * 5f) / Mathf.Sqrt(Mathf.Pow(destination.x, 2) + Mathf.Pow(destination.z, 2)); 
                destination.z = (destination.z * 5f) / Mathf.Sqrt(Mathf.Pow(destination.x, 2) + Mathf.Pow(destination.z, 2));
                currentTime = 0f;

                //get revolve direction around payload
                destAngle = Mathf.Atan2(destination.x - transform.position.x, destination.z - transform.position.z);
                payloadAngle = Mathf.Atan2(patrol.transform.position.x - transform.position.x, patrol.transform.position.z - transform.position.z);
                if (payloadAngle > destAngle)
                    revolveDir = false; //counterclockwise
                else
                    revolveDir = true; //clockwise
            }
            dTime += Time.deltaTime;

            //move behind cover
            currentTime += Time.deltaTime;
            if (currentTime >= findCoverUpdate)
            {
                nextDirection = patrol.getDirection(transform.position.x, transform.position.z, revolveDir);
                currentTime = 0f;
            }
            direction += (nextDirection - direction) * Time.deltaTime;
            transform.position += 2 * speed * Time.deltaTime * direction;
            transform.rotation = Quaternion.LookRotation(direction);

            //once there crouch and start next action
            //Debug.Log(Vector3.Distance(transform.position, destination));
            if (Vector3.Distance(transform.position, destination) <= 1.5f)
            {
                defenseActions[0].color = new Color(defenseActions[0].color.r, defenseActions[0].color.g, defenseActions[0].color.b, 1f); //enter mock crouch [change speed?]
                shoot = true;
                dTime = 0f;
            }
        }
        else //pop out (up, left, or right) and shoot enemy
        {
            if (dTime == 0f)
            {
                shootDir = Random.Range(0, 3);
                dTime += Time.deltaTime;
                Debug.Log(shootDir);
            }
            else if (dTime >= shootStart + shootTime)
            {
                shoot = false;
                dTime = 0f;
            }
            else
            {
                switch (shootDir) //may be problems in the substates
                {
                    case 0: //up
                        defenseActions[0].color = new Color(defenseActions[0].color.r, defenseActions[0].color.g, defenseActions[0].color.b, 0f);
                        defenseActions[1].color = new Color(defenseActions[1].color.r, defenseActions[1].color.g, defenseActions[1].color.b, 1f);
                        transform.rotation = Quaternion.LookRotation(enemy.position - transform.position);
                        if (dTime == 0f)
                            shootStart = dTime;
                        break;
                    case 1: //left
                        if (Physics.Raycast(transform.position, enemy.position, out objHit, visionRadius) & objHit.collider.tag == "Enemy" & shootStart == 0f) //start shooting
                        {
                            defenseActions[1].color = new Color(defenseActions[1].color.r, defenseActions[1].color.g, defenseActions[1].color.b, 1f);
                            shootStart = dTime;
                        }
                        else if (Physics.Raycast(transform.position, enemy.position, out objHit, visionRadius) & objHit.collider.tag == "Enemy" & shootStart == 0f) //keep shooting
                        {
                            transform.rotation = Quaternion.LookRotation(enemy.position - transform.position);
                        }
                        else if (Physics.Raycast(transform.position, enemy.position, out objHit, visionRadius) & objHit.collider.tag != "Enemy" & shootStart > 0f) //end shooting
                        {
                            dTime += shootTime;
                        }
                        else //find target left
                        {
                            transform.position += crouchSpeed * -transform.right * Time.deltaTime;
                        }
                        break;
                    case 2: //right
                        if (Physics.Raycast(transform.position, enemy.position, out objHit, visionRadius) & objHit.collider.tag == "Enemy" & shootStart == 0f)
                        {
                            defenseActions[1].color = new Color(defenseActions[1].color.r, defenseActions[1].color.g, defenseActions[1].color.b, 1f);
                            shootStart = dTime;
                        }
                        else if (Physics.Raycast(transform.position, enemy.position, out objHit, visionRadius) & objHit.collider.tag == "Enemy" & shootStart == 0f)
                        {
                            transform.rotation = Quaternion.LookRotation(enemy.position - transform.position);
                        }
                        else if (Physics.Raycast(transform.position, enemy.position, out objHit, visionRadius) & objHit.collider.tag != "Enemy" & shootStart > 0f)
                        {
                            dTime += shootTime;
                        }
                        else
                        {
                            transform.position += crouchSpeed * transform.right * Time.deltaTime;
                        }
                        break;
                }

                dTime += Time.deltaTime;
            }
        }
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
            amp = 0f;
            switchTime = t + transTime + Random.Range(switchRange[0], switchRange[1]);
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

            //reset vars
            switchTime = t + transTime + Random.Range(switchRange[0], switchRange[1]);
            serpentine = !serpentine;
            fleeTransition = true;
            startTrans = t;
        }

        //smoothly transition between serpentine and straight fleeing
        if (fleeTransition)
        {
            if (serpentine)
                amp += Time.deltaTime;
            else
                amp -= Time.deltaTime;

            if (t - (startTrans + transTime) >= transTime)
            {
                fleeTransition = false;
                if (serpentine)
                    amp = AMP;
                else
                    amp = 0f;
            }
        }

        direction = amp * Mathf.Sin(freq * t) * enemyVecPerp + enemyVec;
        transform.position += fleeSpeed * Time.deltaTime * direction.normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        t += Time.deltaTime;
    }

    string CheckState()
    {
        //if enemy spotted then defend
        if (enemySpotted & health > fleeHealth)
        {
            state = "defend";
        }

        //if health too low then flee
        if (health <= fleeHealth & t < endFleeTime)
        {
            state = "flee";
        }

        //if fleeing for too long return to patrol
        if (t >= endFleeTime)
        {
            t = 0f;
            state = "patrol";

            testScript.flee = false;
        }

        return state;
    }

    void Update()
    {
        state = CheckState();
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
