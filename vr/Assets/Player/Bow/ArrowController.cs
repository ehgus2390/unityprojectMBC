using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    private bool hasHit = false;

    void OnCollisionEnter(Collision collision)
    {
        if (!hasHit)
        {
            hasHit = true;
            // ȭ���� ���� ������ ����
            GetComponent<Rigidbody>().isKinematic = true;
            // �ʿ��ϴٸ� �ı�, ����Ʈ, ���� ó�� �� �߰�
            // ��: Destroy(gameObject, 5f); // 5�� �� ȭ�� ����
        }
    }
}
