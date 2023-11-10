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
    float shootTime = 5f;
    float shootStart = 100f;
    RaycastHit objHit;
    float crouchSpeed;
    float findCoverUpdate = 0.3f;
    float payloadAngle;
    float destAngle;
    bool revolveDir;
    Quaternion startRot;
    Quaternion endRot;
    float rotTime = 0f;
    bool rotDone = false;

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
        distFromCover = patrol.len + GetComponent<CapsuleCollider>().radius;
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
                enemyAngle = Mathf.Atan2(enemy.position.z - patrol.transform.position.z, enemy.position.x - patrol.transform.position.x);
                destination.x = -distFromCover * Mathf.Cos(enemyAngle);
                destination.z = -distFromCover * Mathf.Sin(enemyAngle);
                //4.5f is the raidus of the circle that approximates the line (0.25x)^4 + (0.25z)^4 = 1 (SWITCH TO THIS CIRCLE?)
                destination.x = (destination.x * 5f) / Mathf.Sqrt(Mathf.Pow(destination.x, 2) + Mathf.Pow(destination.z, 2)); //assumes payload is at origin
                destination.z = (destination.z * 5f) / Mathf.Sqrt(Mathf.Pow(destination.x, 2) + Mathf.Pow(destination.z, 2));
                currentTime = 0f;

                //get revolve direction around payload
                destAngle = Mathf.Atan2(destination.z - transform.position.z, destination.x - transform.position.x);
                payloadAngle = Mathf.Atan2(patrol.transform.position.z - transform.position.z, patrol.transform.position.x - transform.position.x);
                if (payloadAngle > destAngle)
                    revolveDir = false; //counterclockwise
                else
                    revolveDir = true; //clockwise
                revolveDir = false; //OVERRIDE

                rotTime = 0f;
                rotDone = false;
            }

            dTime += Time.deltaTime;

            if (Vector3.Distance(transform.position, destination) <= 1.5f)
            {
                //behind cover crouch, turn to enemy, and start next action
                if (rotTime == 0f)
                {
                    startRot = transform.rotation;
                    endRot = Quaternion.LookRotation(enemy.position - transform.position);
                }
                else if (rotTime >= 1f) //standard lerp time of 1s
                {
                    rotDone = true;
                }
                
                if (!rotDone)
                {
                    transform.rotation = Quaternion.Lerp(startRot, endRot, rotTime);
                    rotTime += 2 * Time.deltaTime;
                }
                else
                {
                    defenseActions[0].color = new Color(defenseActions[0].color.r, defenseActions[0].color.g, defenseActions[0].color.b, 1f); //enter mock crouch
                    shoot = true;
                    dTime = 0f;
                }
            }
            else
            {
                //move behind cover
                currentTime += Time.deltaTime;
                if (currentTime >= findCoverUpdate)
                {
                    nextDirection = patrol.getDirection(transform.position.x, transform.position.z, revolveDir);
                    currentTime = 0f;
                }
                direction += (nextDirection - direction) * Time.deltaTime;
                transform.position += fleeSpeed * Time.deltaTime * direction;
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        else //pop out (up, left, or right) and shoot enemy
        {
            if (dTime == 0f)
            {
                shootDir = Random.Range(0, 3);
                dTime += Time.deltaTime;
                shootStart = 100f;
                Debug.Log(shootDir);
            }
            else if (dTime >= shootStart + shootTime)
            {
                shoot = false;
                dTime = 0f;
                defenseActions[0].color = new Color(defenseActions[0].color.r, defenseActions[0].color.g, defenseActions[0].color.b, 0f);
                defenseActions[1].color = new Color(defenseActions[1].color.r, defenseActions[1].color.g, defenseActions[1].color.b, 0f);
            }
            else
            {
                switch (shootDir) //problems in the substates
                {
                    case 0: //up
                        defenseActions[0].color = new Color(defenseActions[0].color.r, defenseActions[0].color.g, defenseActions[0].color.b, 0f);
                        defenseActions[1].color = new Color(defenseActions[1].color.r, defenseActions[1].color.g, defenseActions[1].color.b, 1f);
                        if (dTime == 0f)
                            shootStart = dTime;
                        break;

                    case 1: //left
                        if (Physics.Raycast(transform.position, enemy.position - transform.position, out objHit))
                        {
                            if (shootStart == 100f)
                            {
                                if (objHit.collider.gameObject.tag == "Enemy") //start shooting
                                {
                                    defenseActions[0].color = new Color(defenseActions[0].color.r, defenseActions[0].color.g, defenseActions[0].color.b, 0f);
                                    defenseActions[1].color = new Color(defenseActions[1].color.r, defenseActions[1].color.g, defenseActions[1].color.b, 1f);
                                    shootStart = dTime;
                                }
                                else //find target left
                                {
                                    defenseActions[0].color = new Color(defenseActions[0].color.r, defenseActions[0].color.g, defenseActions[0].color.b, 1f);
                                    defenseActions[1].color = new Color(defenseActions[1].color.r, defenseActions[1].color.g, defenseActions[1].color.b, 0f);
                                    transform.position += crouchSpeed * -transform.right * Time.deltaTime;
                                    transform.rotation = Quaternion.LookRotation(enemy.position - transform.position);
                                }
                            }
                            else if (dTime > shootStart) 
                            {
                                if (objHit.collider.gameObject.tag == "Enemy") //keep shooting
                                {
                                    defenseActions[0].color = new Color(defenseActions[0].color.r, defenseActions[0].color.g, defenseActions[0].color.b, 0f);
                                    defenseActions[1].color = new Color(defenseActions[1].color.r, defenseActions[1].color.g, defenseActions[1].color.b, 1f);
                                }
                                else //find target left
                                {
                                    defenseActions[0].color = new Color(defenseActions[0].color.r, defenseActions[0].color.g, defenseActions[0].color.b, 1f);
                                    defenseActions[1].color = new Color(defenseActions[1].color.r, defenseActions[1].color.g, defenseActions[1].color.b, 0f);
                                    transform.position += crouchSpeed * -transform.right * Time.deltaTime;
                                    transform.rotation = Quaternion.LookRotation(enemy.position - transform.position);
                                }
                            }
                        }
                        else //find target left
                        {
                            defenseActions[0].color = new Color(defenseActions[0].color.r, defenseActions[0].color.g, defenseActions[0].color.b, 1f);
                            defenseActions[1].color = new Color(defenseActions[1].color.r, defenseActions[1].color.g, defenseActions[1].color.b, 0f);
                            transform.position += crouchSpeed * -transform.right * Time.deltaTime;
                            transform.rotation = Quaternion.LookRotation(enemy.position - transform.position);
                        }
                        break;

                    case 2: //right
                        if (Physics.Raycast(transform.position, enemy.position - transform.position, out objHit))
                        {
                            if (shootStart == 100f)
                            {
                                if (objHit.collider.gameObject.tag == "Enemy") //start shooting
                                {
                                    defenseActions[0].color = new Color(defenseActions[0].color.r, defenseActions[0].color.g, defenseActions[0].color.b, 0f);
                                    defenseActions[1].color = new Color(defenseActions[1].color.r, defenseActions[1].color.g, defenseActions[1].color.b, 1f);
                                    shootStart = dTime;
                                }
                                else //find target right
                                {
                                    defenseActions[0].color = new Color(defenseActions[0].color.r, defenseActions[0].color.g, defenseActions[0].color.b, 1f);
                                    defenseActions[1].color = new Color(defenseActions[1].color.r, defenseActions[1].color.g, defenseActions[1].color.b, 0f);
                                    transform.position += crouchSpeed * transform.right * Time.deltaTime;
                                    transform.rotation = Quaternion.LookRotation(enemy.position - transform.position);
                                }
                            }
                            else if (dTime > shootStart)
                            {
                                if (objHit.collider.gameObject.tag == "Enemy") //keep shooting
                                {
                                    defenseActions[0].color = new Color(defenseActions[0].color.r, defenseActions[0].color.g, defenseActions[0].color.b, 0f);
                                    defenseActions[1].color = new Color(defenseActions[1].color.r, defenseActions[1].color.g, defenseActions[1].color.b, 1f);
                                }
                                else //find target right
                                {
                                    defenseActions[0].color = new Color(defenseActions[0].color.r, defenseActions[0].color.g, defenseActions[0].color.b, 1f);
                                    defenseActions[1].color = new Color(defenseActions[1].color.r, defenseActions[1].color.g, defenseActions[1].color.b, 0f);
                                    transform.position += crouchSpeed * transform.right * Time.deltaTime;
                                    transform.rotation = Quaternion.LookRotation(enemy.position - transform.position);
                                }
                            }
                        }
                        else //find target right
                        {
                            defenseActions[0].color = new Color(defenseActions[0].color.r, defenseActions[0].color.g, defenseActions[0].color.b, 1f);
                            defenseActions[1].color = new Color(defenseActions[1].color.r, defenseActions[1].color.g, defenseActions[1].color.b, 0f);
                            transform.position += crouchSpeed * transform.right * Time.deltaTime;
                            transform.rotation = Quaternion.LookRotation(enemy.position - transform.position);
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
            enemyVecPerp = new Vector3(enemyVec.z, 0f, enemyVec.x);
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
                enemyVecPerp = new Vector3(enemyVec.z, 0f, enemyVec.x);
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
