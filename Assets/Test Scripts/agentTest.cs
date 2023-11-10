using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class agentTest : MonoBehaviour
{
    agentController agent;
    public GameObject enemy;
    GameObject enemyInst;
    [HideInInspector] public bool enemySpawned;
    [Header("Patrol [1], Flee [2], Defend [3]")]
    public bool patrol;
    public bool flee;
    public bool defend;
    
    void Start()
    {
        agent = GameObject.Find("Agent").GetComponent<agentController>();
        if (flee)
        {
            SpawnEnemy(Random.Range(0, 4));
            enemySpawned = true;
        }
        else if (defend)
        {
            SpawnEnemy(Random.Range(0, 4));
            enemySpawned = true;
        }
        else
        {
            enemySpawned = false;
        }
    }

    void SpawnEnemy(int location)
    {
        switch (location)
        {
            case 0:
                enemyInst = Instantiate(enemy, new Vector3(10f, 1f, 0f), Quaternion.identity);
                agent.enemy = enemyInst.transform;
                break;
            case 1:
                enemyInst = Instantiate(enemy, new Vector3(0f, 1f, 10f), Quaternion.identity);
                agent.enemy = enemyInst.transform;
                break;
            case 2:
                enemyInst = Instantiate(enemy, new Vector3(-10f, 1f, 0f), Quaternion.identity);
                agent.enemy = enemyInst.transform;
                break;
            case 3:
                enemyInst = Instantiate(enemy, new Vector3(0f, 1f, -10f), Quaternion.identity);
                agent.enemy = enemyInst.transform;
                break;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            flee = false;
            defend = false;
            patrol = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            defend = false;
            patrol = false;
            flee = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            flee = false;
            patrol = false;
            defend = true;
        }


        if (flee)
        {
            if (!agent.state.Equals("flee"))
            {
                if (!enemySpawned)
                {
                    SpawnEnemy(Random.Range(0, 4));
                    enemySpawned = true;
                }
                agent.health = 10f;
            }
        }
        else if (defend)
        {
            if (!agent.state.Equals("defend"))
            {
                agent.health = 100f;
                if (!enemySpawned)
                {
                    SpawnEnemy(Random.Range(0, 4));
                    enemySpawned = true;
                }
            }
        }
        else
        {
            if (!agent.state.Equals("patrol"))
            {
                agent.enemySpotted = false;
                agent.health = 100f;
                agent.t += agent.endFleeTime;
                if (enemySpawned)
                {
                    Destroy(enemyInst);
                    enemySpawned = false;
                }
            }
        }
    }
}
