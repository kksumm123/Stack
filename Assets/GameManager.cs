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

        currentCube = movingCube;
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
        else if (Input.anyKeyDown)
        {
            // 큐브 자르기
            BreakCube();

            if (GameState == GameState.Play)
                CreateCube();
        }
    }

    GameObject currentCube;
    GameObject prevCube;
    Vector3 multFactor;
    [SerializeField] int cubeCount = 0;
    private void CreateCube()
    {
        prevCube = currentCube;

        cubeCount++;
        currentCube = Instantiate(prevCube);
        if (IsMoveX())
        {
            currentCube.transform.position = new Vector3(positionValue, cubeCount * cubeHeight, prevCube.transform.position.z);
            multFactor = new Vector3(-1, 1, 1);
        }
        else
        {
            currentCube.transform.position = new Vector3(prevCube.transform.position.x, cubeCount * cubeHeight, positionValue);
            multFactor = new Vector3(1, 1, -1);
        }
        currentCube.GetComponent<MovingCube>().DesPos = Vector3.Scale(currentCube.transform.position, multFactor);

        Color.RGBToHSV(cubeColor, out float h, out float s, out float v);
        cubeColor = Color.HSVToRGB(h + (1f / 256) * colorStep, s, v);
        currentCube.GetComponent<Renderer>().material.SetColor("_BaseColor", cubeColor);

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
        currentCube.GetComponent<MovingCube>().Disable();

        var prevCubeTr = prevCube.transform;
        var currentCubeTr = currentCube.transform;

        //prevCube의 영역을 벗어나면 부수기
        Vector3 posCenter;
        if (IsCombo(prevCubeTr.position, currentCubeTr.position))
        {
            comboCount++;
            comboText.text = $"Combo {comboCount}!!";

            posCenter = new Vector3(prevCubeTr.position.x, currentCubeTr.position.y, prevCubeTr.position.z);
            currentCubeTr.position = posCenter;
            currentCubeTr.localScale = prevCubeTr.localScale;

            if (comboCount > 4)
                currentCubeTr.localScale *= comboScalePoint;
        }
        else
        {
            comboCount = 0;
            comboText.text = $"";

            posCenter = new Vector3((prevCubeTr.position.x + currentCubeTr.position.x) * 0.5f,
                                    currentCubeTr.position.y,
                                    (prevCubeTr.position.z + currentCubeTr.position.z) * 0.5f);


            float dropCubeScaleX = currentCubeTr.position.x - prevCubeTr.position.x;
            float dropCubeScaleZ = currentCubeTr.position.z - prevCubeTr.position.z;

            var newCubeLocalScale = new Vector3(currentCubeTr.localScale.x - Mathf.Abs(dropCubeScaleX),
                                                currentCubeTr.localScale.y,
                                                currentCubeTr.localScale.z - Mathf.Abs(dropCubeScaleZ));

            if (IsOutofPrevCube(newCubeLocalScale))
            {
                levelText.text = $"GameOver\nLevel {cubeCount}\nTab to Continue";
                GameState = GameState.GameOver;
                return;
            }
            else
            {
                currentCubeTr.position = posCenter;
                currentCubeTr.localScale = newCubeLocalScale;

                CreateDropCube(currentCubeTr, dropCubeScaleX, dropCubeScaleZ);
            }
        }
    }

    private void CreateDropCube(Transform currentCubeTr, float dropCubeScaleX, float dropCubeScaleZ)
    {
        var dropCube = Instantiate(currentCube);
        Destroy(dropCube.GetComponent<MovingCube>());
        dropCube.AddComponent<Rigidbody>();

        Vector3 dropCubePosFactor = new Vector3(prevCube.transform.localScale.x, 0, prevCube.transform.localScale.z);
        dropCubePosFactor = CompensationCubePos(dropCubeScaleX, dropCubeScaleZ, dropCubePosFactor);
        Vector3 dropCubePos = SetDropCubePos(currentCubeTr, dropCubeScaleX, dropCubeScaleZ, dropCubePosFactor);
        Vector3 dropCubeScale = SetDropCubeScale(currentCubeTr, ref dropCubeScaleX, ref dropCubeScaleZ);

        dropCube.transform.position = dropCubePos;
        dropCube.transform.localScale = dropCubeScale;

        Vector3 CompensationCubePos(float dropCubeScaleX, float dropCubeScaleZ, Vector3 dropCubePosFactor)
        {
            if (IsMoveX())
                dropCubePosFactor.Scale(new Vector3(1, 0, 0));
            else if (IsMoveZ())
                dropCubePosFactor.Scale(new Vector3(0, 0, 1));

            if (dropCubeScaleX < 0 || dropCubeScaleZ < 0)
                dropCubePosFactor *= -1;
            return dropCubePosFactor;
        }
        Vector3 SetDropCubePos(Transform currentCubeTr, float dropCubeScaleX, float dropCubeScaleZ, Vector3 dropCubePosFactor)
        {
            return new Vector3((dropCubeScaleX + dropCubePosFactor.x) * 0.5f,
                               currentCubeTr.position.y,
                               (dropCubeScaleZ + dropCubePosFactor.z) * 0.5f);
        }
        Vector3 SetDropCubeScale(Transform currentCubeTr, ref float dropCubeScaleX, ref float dropCubeScaleZ)
        {
            dropCubeScaleX = dropCubeScaleX == 0 ? currentCubeTr.localScale.x : Mathf.Abs(dropCubeScaleX);
            dropCubeScaleZ = dropCubeScaleZ == 0 ? currentCubeTr.localScale.z : Mathf.Abs(dropCubeScaleZ);
            return new Vector3(dropCubeScaleX, currentCubeTr.localScale.y, dropCubeScaleZ);
        }
    }

    bool IsOutofPrevCube(Vector3 currentCubeLocalScale)
    {
        return currentCubeLocalScale.x < 0 || currentCubeLocalScale.z < 0;
    }

    [SerializeField] float comboDistance = 0.05f;
    bool IsCombo(Vector3 prevCubePos, Vector3 currentCubePos)
    {
        if (IsMoveX())
        {
            if (Mathf.Abs(prevCubePos.x - currentCubePos.x) < comboDistance)
                return true;
            else
                return false;
        }
        else if (IsMoveZ())
        {
            if (Mathf.Abs(prevCubePos.z - currentCubePos.z) < comboDistance)
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
