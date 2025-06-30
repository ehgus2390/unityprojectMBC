//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class DoorInteraction1 : MonoBehaviour
//{
//    public float holdTime = 7f;           // EŰ�� ������ �ϴ� �ð�(��)
//    public Slider gaugeBar;               // UI �������� (Canvas�� ��ġ)
//    public Animator doorAnimator;         // �� �ִϸ����� (���� �ִϸ��̼�)
//    private float holdTimer = 0f;
//    private bool isPlayerNear = false;
//    private bool isDoorOpen = false;

//    void Update()
//    {
//        if (isPlayerNear && !isDoorOpen)
//            if (Input.GetKey(KeyCode.E))
//            {
//                holdTimer += Time.deltaTime;
//                if (gaugeBar != null)
//                {
//                    gaugeBar.gameObject.SetActive(true);
//                    gaugeBar.value = holdTimer / holdTime;
//                }
//                if (holdTimer >= holdTime)
//                {
//                    OpenDoor();
//                }
//            }
//            else
//            {
//                holdTimer = 0f;
//                if (gaugeBar != null)
//                {
//                    gaugeBar.value = 0f;
//                    gaugeBar.gameObject.SetActive(false);
//                }
//            }
//        }
//        else
//        {
//            holdTimer = 0f;
//            if (gaugeBar != null)
//            {
//                gaugeBar.value = 0f;
//                gaugeBar.gameObject.SetActive(false);
//            }
//        }
//    }

//    void OpenDoor()
//    {
//        isDoorOpen = true;
//        if (doorAnimator != null)
//            doorAnimator.SetTrigger("Open");
//        if (gaugeBar != null)
//            gaugeBar.gameObject.SetActive(false);
//        // �ʿ��ϴٸ� �� �ݶ��̴� ��Ȱ��ȭ �� �߰�
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))
//            isPlayerNear = true;
//    }

//    private void OnTriggerExit(Collider other)
//    {
//        if (other.CompareTag("Player"))
//            isPlayerNear = false;
//    }
//}
