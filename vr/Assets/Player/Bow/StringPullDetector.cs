using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class StringPullDetector : MonoBehaviour
{
    public BowController bowController; // BowController�� �ν����Ϳ��� �Ҵ�

    private bool isPulling = false;

    void OnTriggerEnter(Collider other)
    {
        // StringAttachPoint�� ����� ��
        if (other.CompareTag("StringAttachPoint") && !isPulling)
        {
            isPulling = true;
            bowController.StartPull(this.transform); // ������ Transform ����
        }
    }

    void OnTriggerExit(Collider other)
    {
        // StringAttachPoint���� ���� �������� ��
        if (other.CompareTag("StringAttachPoint") && isPulling)
        {
            isPulling = false;
            bowController.ReleasePull();
        }
    }
}
