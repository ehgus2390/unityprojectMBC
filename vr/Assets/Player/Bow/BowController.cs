using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowController : MonoBehaviour
{
    public Transform stringAttachPoint; // ���� ������(������ �ٴ� ��ġ)
    public Transform arrowStartPoint;   // ȭ�� ���� ��ġ
    public GameObject arrowPrefab; //ȭ�� ������ 
    public int damage = 10;

    private GameObject _currentArrow; //���� �غ����� ȭ�� ������Ʈ
    private bool _isStringPulled = false;  //������ ��������� ����
    private Transform _pullingHand;  //������ ���� �ִ� ���� Transform

    void Update()
    {
        //������ ����� �����̰� ���� ����Ǿ� ������
        if (_isStringPulled && _pullingHand != null)
        {
            // ���� �������� ��(��Ʈ�ѷ�) ������ �Ÿ����
            float pullDistance = Vector3.Distance(stringAttachPoint.position, _pullingHand.position);
            // �ð��� ȿ�� �� �Ŀ� ���
        }
    }

    //���� ���� ����(�������� ���� ��ġ�� ����� �� ȣ��)
    public void StartPull(Transform hand)
    {
        _isStringPulled = true;
        _pullingHand = hand;
        // ȭ���� Ȱ�� ����(Ȥ�� ����)
        _currentArrow = Instantiate(arrowPrefab, arrowStartPoint.position, arrowStartPoint.rotation, arrowStartPoint);
    }

    //���� ����
    public void ReleasePull()
    {
        if (_currentArrow != null)
        {
            // ȭ�� �߻�(ȭ���� �θ𿡼� �и�)
            _currentArrow.transform.parent = null;
            //ȭ�쿡 Rigidbody ������Ʈ ��������
            Rigidbody rb = _currentArrow.GetComponent<Rigidbody>();
            //���� ȿ�� ������ ���� isKinetic ����
            rb.isKinematic = false;
            //ȭ�쿡 ���� ���� �߻�
            rb.AddForce((arrowStartPoint.forward) * damage, ForceMode.Impulse);
            _currentArrow = null;
        }
        _isStringPulled = false;
        _pullingHand = null;
    }
}
