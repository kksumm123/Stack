using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public const string movingCubeStr = "MovingCube";
    GameObject movingCube;
    [SerializeField] float cubeHeight = 0.2f;
    [SerializeField] float positionValue = 2;
    Color cubeColor = new Color(214, 255, 107, 255);
    [SerializeField] float colorStep = 5f;
    void Start()
    {
        movingCube = (GameObject)Resources.Load(movingCubeStr);
        cubeColor = movingCube.GetComponent<Renderer>().sharedMaterial
                              .GetColor("_BaseColor");
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
        var dir = cubeCount % 2 == 1 ? 1 : -1f;
        var newGo = Instantiate(movingCube,
                        new Vector3(positionValue * dir, cubeCount * cubeHeight, positionValue),
                        Quaternion.Euler(0, 45, 0));
        Color.RGBToHSV(cubeColor, out float h, out float s, out float v);
        cubeColor = Color.HSVToRGB(h + (1f / 256) * colorStep, s, v);
        newGo.GetComponent<Renderer>().material.SetColor("_BaseColor", cubeColor);

        // 카메라 위로 이동
        Camera.main.transform.Translate(0, cubeHeight, 0, Space.World);
    }
}
