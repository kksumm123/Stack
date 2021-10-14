using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingCube : MonoBehaviour
{
    [SerializeField] float posValue = 2;
    Vector3 desPos;
    Vector3 startPos;
    float startTime;
    RaycastHit hitInfo;
    LayerMask cubeLayer;
    void Start()
    {
        cubeLayer = 1 << LayerMask.NameToLayer("Cube");
        Physics.Raycast(new Vector3(0, 1000, 0), Vector3.down, out hitInfo, 1000, cubeLayer);
        
        var randValue = Random.Range(0, 2) == 0 ? 1 : -1f;
        var posY = 0.5f * transform.localScale.y + hitInfo.point.y;
        startPos = new Vector3(posValue * randValue, posY, posValue);
        desPos = startPos;
        desPos.x *= -1f;
        desPos.z *= -1f;
        startTime = Time.time;
    }

    float elapsedTime;
    float calcTimeResult;
    Vector3 pos;
    void Update()
    {
        elapsedTime = Time.time - startTime;
        calcTimeResult = Mathf.Abs(elapsedTime % 2 - 1f);
        pos = Vector3.Lerp(desPos, startPos, calcTimeResult);
        transform.position = pos;
    }
}
