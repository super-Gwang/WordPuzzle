using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBackground : MonoBehaviour
{
    public Vector3 startPoint; // ��� ���� Transform
    public Transform endPoint; // ������ Transform

    public float moveSpeed = 5f; // �̵� �ӵ�
    public float rotationSpeed = 5f; // ȸ�� �ӵ�

    void Start()
    {
        startPoint = GetComponent<RectTransform>().position;
    }

    void Update()
    {
        // ��ǥ ��ġ�� �����ߴ��� Ȯ��
        if (Vector3.Distance(transform.position, endPoint.position) < 0.1f)
            transform.position = startPoint;
        else
            MoveToTarget();

    }

    // ��ǥ ��ġ�� �̵��ϴ� �Լ�
    void MoveToTarget()
    {
        // ��ǥ ��ġ���� ������ �ӵ��� �̵�
        transform.position = Vector3.MoveTowards(transform.position, endPoint.position, moveSpeed * Time.deltaTime);
        transform.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime));
    }
}
