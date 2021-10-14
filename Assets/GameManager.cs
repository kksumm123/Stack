using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum GameState
{
    None,
    Play,
    GameOver,
}
public class GameManager : MonoBehaviour
{
    GameState gameState = GameState.None;
    public GameState GameState
    {
        get => gameState;
        set
        {
            if (gameState == value)
                return;
            gameState = value;
            switch (gameState)
            {
                case GameState.Play:
                    Time.timeScale = 1;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 0;
                    break;
            }
        }
    }


    public const string movingCubeStr = "MovingCube";
    GameObject movingCube;
    [SerializeField] Text text;
    [SerializeField] float cubeHeight = 0.2f;
    [SerializeField] float positionValue = 2;
    Color cubeColor = new Color(214, 255, 107, 255);
    [SerializeField] float colorStep = 5f;
    void Start()
    {
        GameState = GameState.Play;
        text = GameObject.Find("Canvas").transform.Find("Text").GetComponent<Text>();
        movingCube = (GameObject)Resources.Load(movingCubeStr);
        cubeColor = movingCube.GetComponent<Renderer>().sharedMaterial
                              .GetColor("_BaseColor");
        newCube = movingCube;
        CreateCube();
    }

    void Update()
    {
        if (GameState == GameState.GameOver)
            return;

        if (Input.anyKeyDown)
        {
            // 큐브 자르기
            BreakCube();

            if (GameState == GameState.Play)
                CreateCube();
        }
    }

    GameObject newCube;
    GameObject prevCube;
        Vector3 multFactor;
    [SerializeField] int cubeCount = 0;
    private void CreateCube()
    {
        prevCube = newCube;

        cubeCount++;
        newCube = Instantiate(prevCube);
        if (cubeCount % 2 == 1)
        {
            newCube.transform.position = new Vector3(positionValue, cubeCount * cubeHeight, prevCube.transform.position.z);
            multFactor = new Vector3(-1, 1, 1);
        }
        else
        {
            newCube.transform.position = new Vector3(prevCube.transform.position.x, cubeCount * cubeHeight, positionValue);
            multFactor = new Vector3(1, 1, -1);
        }
        newCube.GetComponent<MovingCube>().DesPos = Vector3.Scale(newCube.transform.position, multFactor);

        Color.RGBToHSV(cubeColor, out float h, out float s, out float v);
        cubeColor = Color.HSVToRGB(h + (1f / 256) * colorStep, s, v);
        newCube.GetComponent<Renderer>().material.SetColor("_BaseColor", cubeColor);

        // 카메라 위로 이동
        Camera.main.transform.Translate(0, cubeHeight, 0, Space.World);

        text.text = $"Level {cubeCount}";
    }

    void BreakCube()
    {
        if (prevCube == null)
            return;

        var prevCubeTr = prevCube.transform;
        var newCubeTr = newCube.transform;
        //prevCube의 영역을 벗어나면 부수기
        var posCenter = new Vector3((prevCubeTr.position.x + newCubeTr.position.x) * 0.5f,
                                    newCubeTr.position.y,
                                    (prevCubeTr.position.z + newCubeTr.position.z) * 0.5f);
        var localScale = new Vector3(newCubeTr.localScale.x - Mathf.Abs(newCubeTr.position.x - prevCubeTr.position.x),
                                           newCubeTr.localScale.y,
                                           newCubeTr.localScale.z - Mathf.Abs(newCubeTr.position.z - prevCubeTr.position.z));

        if (localScale.x < 0 || localScale.z < 0)
        {
            text.text = $"GameOver\nLevel {cubeCount}";
            GameState = GameState.GameOver;
        }
        else
            newCubeTr.localScale = localScale;
    }
}
