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
            // 화살이 맞은 지점에 고정
            GetComponent<Rigidbody>().isKinematic = true;
            // 필요하다면 파괴, 이펙트, 점수 처리 등 추가
            // 예: Destroy(gameObject, 5f); // 5초 후 화살 삭제
        }
    }
}
