using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyTestBehavior : MonoBehaviour
{
    float speed = 1f;
    float amp = 10f;
    float freq = 1f;
    float t = 0f;
    Vector3 direction;
    Vector3 crossAxis;

    void Start()
    {
        crossAxis.x = transform.position.z;
        crossAxis.z = transform.position.x;
        crossAxis = crossAxis.normalized;
    }

    void Update()
    {
        direction = amp * Mathf.Cos(freq * t) * crossAxis;
        transform.position += speed * Time.deltaTime * direction;
        t += Time.deltaTime;
    }
}
