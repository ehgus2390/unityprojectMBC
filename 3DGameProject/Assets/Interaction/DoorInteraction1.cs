//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class DoorInteraction1 : MonoBehaviour
//{
//    public float holdTime = 7f;           // E키를 눌러야 하는 시간(초)
//    public Slider gaugeBar;               // UI 게이지바 (Canvas에 배치)
//    public Animator doorAnimator;         // 문 애니메이터 (열림 애니메이션)
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
//        // 필요하다면 문 콜라이더 비활성화 등 추가
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
