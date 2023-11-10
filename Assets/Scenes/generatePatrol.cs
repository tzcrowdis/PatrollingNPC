using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generatePatrol : MonoBehaviour
{
    //Script assumes the object is a square in the (x, z) plane
    [HideInInspector] public float length;
    [HideInInspector] public bool far = false;
    [HideInInspector] public float len;

    Vector3 direction = new Vector3(0f, 0f, 0f);

    void Start()
    {
        length = gameObject.transform.localScale.x;
        len = length / 2;
    }

    //returns the direction vector given the position
    public Vector3 getDirection(float x, float z, bool clockwise)
    {
        //find the quadrant to determine counterclockwise
        if (x >= length)
        {
            if (-2 < len & z < len) //Q1
                direction = new Vector3(0f, 0f, 1f);
            else if (z >= len) //Q1.5
                direction = new Vector3(-1f, 0f, 1f);
            else if (z <= -len) //Q4.5
                direction = new Vector3(1f, 0f, 1f);
        }
        else if (-len < x & x < len)
        {
            if (-len < z & z < len) //No go zone (aka inside patrolling object)
                direction = new Vector3(0f, 0f, 0f);
            else if (z >= len) //Q2
                direction = new Vector3(-1f, 0f, 0f);
            else if (z <= -len) //Q4
                direction = new Vector3(1f, 0f, 0f);
        }
        else if (x <= -len)
        {
            if (-len < z & z < len) //Q3
            {
                direction = new Vector3(0f, 0f, -1f);
            }
            else if (z >= len) //Q2.5
            {
                direction = new Vector3(-1f, 0f, -1f);
            }
            else if (z <= -len) //Q3.5
            {
                direction = new Vector3(1f, 0f, -1f);
            }
        }
        if (clockwise)
            direction *= -1;
        direction = direction.normalized;

        //desired path is on the line (0.25x)^4 + (0.25z)^4 = 1
        //determine if we are too close or far from origin relative to the line
        //then move towards line
        if (far)
        {
            if (Mathf.Pow((x / length), 4) + Mathf.Pow((z / length), 4) < 1)
                direction += new Vector3(x, 0f, z);
            else
                direction += new Vector3(-x, 0f, -z);
        }
        else
        {
            if (Mathf.Pow((x / length), 4) + Mathf.Pow((z / length), 4) < 1)
                direction += new Vector3(x, 0f, z).normalized;
            else
                direction += new Vector3(-x, 0f, -z).normalized;
        }

        return direction.normalized;
    }
}
