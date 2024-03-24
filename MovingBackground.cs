using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBackground : MonoBehaviour
{
    public Vector3 startPoint; // 출발 지점 Transform
    public Transform endPoint; // 목적지 Transform

    public float moveSpeed = 5f; // 이동 속도
    public float rotationSpeed = 5f; // 회전 속도

    void Start()
    {
        startPoint = GetComponent<RectTransform>().position;
    }

    void Update()
    {
        // 목표 위치에 도착했는지 확인
        if (Vector3.Distance(transform.position, endPoint.position) < 0.1f)
            transform.position = startPoint;
        else
            MoveToTarget();

    }

    // 목표 위치로 이동하는 함수
    void MoveToTarget()
    {
        // 목표 위치까지 일정한 속도로 이동
        transform.position = Vector3.MoveTowards(transform.position, endPoint.position, moveSpeed * Time.deltaTime);
        transform.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime));
    }
}
