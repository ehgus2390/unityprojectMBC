using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class StringPullDetector : MonoBehaviour
{
    public BowController bowController; // BowController를 인스펙터에서 할당

    private bool isPulling = false;

    void OnTriggerEnter(Collider other)
    {
        // StringAttachPoint에 닿았을 때
        if (other.CompareTag("StringAttachPoint") && !isPulling)
        {
            isPulling = true;
            bowController.StartPull(this.transform); // 오른손 Transform 전달
        }
    }

    void OnTriggerExit(Collider other)
    {
        // StringAttachPoint에서 손이 떨어졌을 때
        if (other.CompareTag("StringAttachPoint") && isPulling)
        {
            isPulling = false;
            bowController.ReleasePull();
        }
    }
}
