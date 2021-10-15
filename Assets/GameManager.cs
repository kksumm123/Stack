using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
                    reStartRealTime = Time.realtimeSinceStartup + 1;
                    Time.timeScale = 0;
                    break;
            }
        }
    }


    public const string movingCubeStr = "MovingCube";
    GameObject movingCube;
    Text levelText;
    Text comboText;
    [SerializeField] float cubeHeight = 0.2f;
    [SerializeField] float positionValue = 2;
    Color cubeColor = new Color(214, 255, 107, 255);
    [SerializeField] float colorStep = 5f;
    void Start()
    {
        GameState = GameState.Play;
        levelText = GameObject.Find("Canvas").transform.Find("LevelText").GetComponent<Text>();
        comboText = GameObject.Find("Canvas").transform.Find("ComboText").GetComponent<Text>();
        movingCube = (GameObject)Resources.Load(movingCubeStr);
        cubeColor = movingCube.GetComponent<Renderer>().sharedMaterial
                              .GetColor("_BaseColor");

        newCube = movingCube;
        comboCount = 0;
        comboText.text = "";
        CreateCube();
    }

    float reStartRealTime;
    void Update()
    {
        if (GameState == GameState.GameOver)
        {
            if (Input.anyKeyDown && Time.realtimeSinceStartup > reStartRealTime)
            {
                SceneManager.LoadScene(0);
            }
        }

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
        if (IsMoveX())
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

        levelText.text = $"Level {cubeCount}";
    }

    int comboCount = 0;
    [SerializeField] float comboScalePoint = 1.2f;
    void BreakCube()
    {
        if (prevCube == null)
            return;
        newCube.GetComponent<MovingCube>().Disable();

        var prevCubeTr = prevCube.transform;
        var newCubeTr = newCube.transform;

        //prevCube의 영역을 벗어나면 부수기
        Vector3 posCenter;
        if (IsCombo(prevCubeTr.position, newCubeTr.position))
        {
            comboCount++;
            comboText.text = $"Combo {comboCount}!!";

            posCenter = new Vector3(prevCubeTr.position.x, newCubeTr.position.y, prevCubeTr.position.z);
            newCubeTr.position = posCenter;
            newCubeTr.localScale = prevCubeTr.localScale;

            if (comboCount > 4)
                newCubeTr.localScale *= comboScalePoint;
        }
        else
        {
            comboCount = 0;
            comboText.text = $"";

            posCenter = new Vector3((prevCubeTr.position.x + newCubeTr.position.x) * 0.5f,
                                    newCubeTr.position.y,
                                    (prevCubeTr.position.z + newCubeTr.position.z) * 0.5f);

            
            float dropCubeScaleX = newCubeTr.position.x - prevCubeTr.position.x;
            float dropCubeScaleZ = newCubeTr.position.z - prevCubeTr.position.z;

            var newCubeLocalScale = new Vector3(newCubeTr.localScale.x - Mathf.Abs(dropCubeScaleX),
                                                newCubeTr.localScale.y,
                                                newCubeTr.localScale.z - Mathf.Abs(dropCubeScaleZ));

            if (IsOutofPrevCube(newCubeLocalScale))
            {
                levelText.text = $"GameOver\nLevel {cubeCount}\nTab to Continue";
                GameState = GameState.GameOver;
                return;
            }
            else
            {
                newCubeTr.position = posCenter;
                newCubeTr.localScale = newCubeLocalScale;

                CreateDropCube(newCubeTr, dropCubeScaleX, dropCubeScaleZ);
            }
        }
    }

    private void CreateDropCube(Transform newCubeTr, float dropCubeScaleX, float dropCubeScaleZ)
    {

        var dropCube = Instantiate(newCube);
        Destroy(dropCube.GetComponent<MovingCube>());
        dropCube.AddComponent<Rigidbody>();

        Vector3 dropCubePosFactor = new Vector3(prevCube.transform.localScale.x, 0, prevCube.transform.localScale.z);
        if (IsMoveX())
            dropCubePosFactor.Scale(new Vector3(1, 0, 0));
        else if (IsMoveZ())
            dropCubePosFactor.Scale(new Vector3(0, 0, 1));

        if (dropCubeScaleX < 0 || dropCubeScaleZ < 0)
            dropCubePosFactor *= -1;

        var dropCubePos = new Vector3(dropCubeScaleX * 0.5f + dropCubePosFactor.x,
                                      newCubeTr.position.y,
                                      dropCubeScaleZ * 0.5f + dropCubePosFactor.z);

        dropCubeScaleX = dropCubeScaleX == 0 ? newCubeTr.localScale.x : Mathf.Abs(dropCubeScaleX);
        dropCubeScaleZ = dropCubeScaleZ == 0 ? newCubeTr.localScale.z : Mathf.Abs(dropCubeScaleZ);
        var dropCubeScale = new Vector3(dropCubeScaleX, newCubeTr.localScale.y, dropCubeScaleZ);

        dropCube.transform.position = dropCubePos;
        dropCube.transform.localScale = dropCubeScale;
    }

    private static bool IsOutofPrevCube(Vector3 newCubeLocalScale)
    {
        return newCubeLocalScale.x < 0 || newCubeLocalScale.z < 0;
    }

    [SerializeField] float comboDistance = 0.05f;
    bool IsCombo(Vector3 prevCubePos, Vector3 newCubePos)
    {
        if (IsMoveX())
        {
            if (Mathf.Abs(prevCubePos.x - newCubePos.x) < comboDistance)
                return true;
            else
                return false;
        }
        else if (IsMoveZ())
        {
            if (Mathf.Abs(prevCubePos.z - newCubePos.z) < comboDistance)
                return true;
            else
                return false;
        }

        return false;
    }
    bool IsMoveX()
    {
        return cubeCount % 2 == 1;
    }
    bool IsMoveZ()
    {
        return cubeCount % 2 == 0;
    }
}
