using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowController : MonoBehaviour
{
    public Transform stringAttachPoint; // 시위 시작점(시위가 붙는 위치)
    public Transform arrowStartPoint;   // 화살 생성 위치
    public GameObject arrowPrefab; //화살 프리펩 
    public int damage = 10;

    private GameObject _currentArrow; //현재 준비중인 화살 오브젝트
    private bool _isStringPulled = false;  //시위가 당겨졌는지 여부
    private Transform _pullingHand;  //시위를 당기고 있는 손의 Transform

    void Update()
    {
        //시위가 당겨진 상태이고 손이 연결되어 있을때
        if (_isStringPulled && _pullingHand != null)
        {
            // 시위 시작점과 손(컨트롤러) 사이의 거리계산
            float pullDistance = Vector3.Distance(stringAttachPoint.position, _pullingHand.position);
            // 시각적 효과 및 파워 계산
        }
    }

    //시위 당기기 시작(오른손이 시위 위치에 닿았을 때 호출)
    public void StartPull(Transform hand)
    {
        _isStringPulled = true;
        _pullingHand = hand;
        // 화살을 활에 생성(혹은 장전)
        _currentArrow = Instantiate(arrowPrefab, arrowStartPoint.position, arrowStartPoint.rotation, arrowStartPoint);
    }

    //시위 놓기
    public void ReleasePull()
    {
        if (_currentArrow != null)
        {
            // 화살 발사(화살을 부모에서 분리)
            _currentArrow.transform.parent = null;
            //화살에 Rigidbody 컴포넌트 가져오기
            Rigidbody rb = _currentArrow.GetComponent<Rigidbody>();
            //물리 효과 적용을 위해 isKinetic 해제
            rb.isKinematic = false;
            //화살에 힘을 가해 발사
            rb.AddForce((arrowStartPoint.forward) * damage, ForceMode.Impulse);
            _currentArrow = null;
        }
        _isStringPulled = false;
        _pullingHand = null;
    }
}
