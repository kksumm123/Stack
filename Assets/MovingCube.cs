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
        isMoveing = true;
        startPos = transform.position;
        startTime = Time.time;
    }

    float elapsedTime;
    float calcTimeResult;
    Vector3 pos;

    bool isMoveing = false;
    void Update()
    {
        if (isMoveing)
            Move();
    }
    private void Move()
    {
        elapsedTime = Time.time - startTime;
        calcTimeResult = Mathf.Abs(elapsedTime % 2 - 1f);
        pos = Vector3.Lerp(desPos, startPos, calcTimeResult);
        transform.position = pos;
    }

    internal void Disable()
    {
        isMoveing = false;
    }
}
