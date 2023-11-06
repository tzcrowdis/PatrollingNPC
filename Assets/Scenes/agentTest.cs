using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class agentTest : MonoBehaviour
{
    agentController agent;
    [Header("Patrol [1], Flee [2], Defend [3]")]
    public bool patrol;
    public bool flee;
    public bool defend;
    
    void Start()
    {
        agent = GameObject.Find("Agent").GetComponent<agentController>();
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
                agent.health = 10f;
        }
        else if (defend)
        {
            if (!agent.state.Equals("defend"))
            {
                agent.enemySpotted = true;
                agent.health = 100f;
            }
        }
        else
        {
            if (!agent.state.Equals("patrol"))
            {
                agent.enemySpotted = false;
                agent.health = 100f;
                agent.t += agent.endFleeTime;
            }
        }
    }
}
