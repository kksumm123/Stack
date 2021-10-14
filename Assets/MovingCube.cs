using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingCube : MonoBehaviour
{
    Vector3 desPos;
    public Vector3 DesPos { get => desPos; set => desPos = value; }
    Vector3 startPos;
    float startTime;
    void Start()
    {
        startPos = transform.position;
        startTime = Time.time;
    }

    float elapsedTime;
    float calcTimeResult;
    Vector3 pos;


    void Update()
    {
        Move();

        if (Input.anyKeyDown)
            enabled = false;
    }
    private void Move()
    {
        elapsedTime = Time.time - startTime;
        calcTimeResult = Mathf.Abs(elapsedTime % 2 - 1f);
        pos = Vector3.Lerp(desPos, startPos, calcTimeResult);
        transform.position = pos;
    }
}
