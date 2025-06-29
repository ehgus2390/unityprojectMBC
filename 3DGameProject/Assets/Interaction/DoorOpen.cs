using UnityEngine;
using UnityEngine.UI; // 게이지바를 사용하지 않더라도 필요할 수 있습니다.

public class DoorOpen : MonoBehaviour
{
    public float holdTime = 7f;             // E키를 누르고 있어야 하는 시간(초)
    public Animator doorAnimator;           // (새로 생성한) 문 Root Animator

    // (선택 사항) 게이지바를 사용한다면 주석을 풀고 유니티 에디터에서 연결하세요.
    public Slider gaugeBar; 

    private float holdTimer = 0f;
    private bool isPlayerNear = false;
    private bool isDoorOpen = false;

    void Start()
    {
        // (선택 사항) 게이지바 초기화
        
        if (gaugeBar != null)
        {
            gaugeBar.minValue = 0f;
            gaugeBar.maxValue = 1f;
            gaugeBar.value = 1f;
            gaugeBar.gameObject.SetActive(false);
        }
        
    }

    void Update()
    {
        // 플레이어가 문 근처에 있고 문이 아직 열리지 않았다면
        if (isPlayerNear && !isDoorOpen)
        {
            // (선택 사항) 게이지바 활성화
            
            if (gaugeBar != null)
                gaugeBar.gameObject.SetActive(true);
            

            // E 키를 누르고 있는 동안
            if (Input.GetKey(KeyCode.E))
            {
                holdTimer += Time.deltaTime; // 타이머 증가

                // (선택 사항) 게이지바 값 업데이트 (줄어드는 방식)
                
                if (gaugeBar != null)
                    gaugeBar.value = Mathf.Clamp01(1f - (holdTimer / holdTime));
                

                // 설정된 시간을 다 채웠다면 문 열기
                if (holdTimer >= holdTime)
                {
                    OpenDoor(); // 단일 문 열림 함수 호출
                }
            }
            // E 키를 누르고 있지 않다면 타이머 초기화 및 게이지바 원래대로
            else
            {
                holdTimer = 0f;
                // (선택 사항) 게이지바 초기화
                
                if (gaugeBar != null)
                    gaugeBar.value = 1f;
                
            }
        }
        // 플레이어가 문 근처에 없거나 문이 이미 열렸다면
        else
        {
            holdTimer = 0f;
            // (선택 사항) 게이지바 비활성화 및 초기화
            
            if (gaugeBar != null)
            {
                gaugeBar.value = 1f;
                gaugeBar.gameObject.SetActive(false);
            }
            
        }
    }

    // 문을 여는 함수 (두 문을 제어하는 하나의 Animator 트리거 발동)
    void OpenDoor()
    {
        isDoorOpen = true; // 문이 열렸음을 표시
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open"); // "Open" 애니메이션 트리거 발동
            Debug.Log("문이 열렸습니다!"); // 콘솔에 메시지 출력
        }

        // (선택 사항) 게이지바 비활성화
        
        if (gaugeBar != null)
            gaugeBar.gameObject.SetActive(false);
        

        // 문이 열린 후 더 이상 상호작용이 필요 없다면 이 스크립트를 비활성화할 수도 있습니다.
        this.enabled = false; 
    }

    // 플레이어가 콜라이더 범위에 들어왔을 때
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            Debug.Log("플레이어 접근 - 문 열기 가능");
        }
    }

    // 플레이어가 콜라이더 범위에서 나갔을 때
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            holdTimer = 0f; // 범위 밖으로 나가면 타이머 초기화
            // (선택 사항) 게이지바 비활성화 및 초기화
            
            if (gaugeBar != null)
            {
                gaugeBar.value = 1f;
                gaugeBar.gameObject.SetActive(false);
            }
            
            Debug.Log("플레이어 이탈 - 문 열기 취소");
        }
    }
}