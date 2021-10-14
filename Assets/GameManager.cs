using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject movingCube;
    [SerializeField] float cubeHeight = 0.2f;
    [SerializeField] float positionValue = 2;
    void Start()
    {
        CreateCube();
    }

    void Update()
    {
        if (Input.anyKeyDown)
            CreateCube();
    }

    [SerializeField] int cubeCount = 0;
    private void CreateCube()
    {
        cubeCount++;
        var direction = cubeCount % 2 == 1 ? 1 : -1f;
        Instantiate(movingCube,
                    new Vector3(positionValue * direction, cubeCount * cubeHeight, positionValue),
                    Quaternion.Euler(0, 45, 0));
    }
}
