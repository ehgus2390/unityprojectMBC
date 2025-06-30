using UnityEngine;
using UnityEngine.UI;

public class DoorInteraction : MonoBehaviour
{
    public float holdTime = 7f;           // E키를 누르고 있는 시간(초)
    public Slider gaugeBar;               // UI 게이지바 (Canvas의 위치)
    public Animator doorAnimator;         // 문 애니메이터
    public GameObject clearText;          // "Clear" 텍스트 (Canvas의 위치)

    private float holdTimer = 0f;
    private bool isPlayerNear = false;
    private bool isDoorOpen = false;

    void Start()
    {
        if (gaugeBar != null)
        {
            gaugeBar.minValue = 0f;
            gaugeBar.maxValue = 1f;
            gaugeBar.value = 1f;
            gaugeBar.gameObject.SetActive(false);
        }
        if (clearText != null)
        {
            clearText.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerNear && !isDoorOpen)
        {
            if (gaugeBar != null)
                gaugeBar.gameObject.SetActive(true);

            if (Input.GetKey(KeyCode.E))
            {
                holdTimer += Time.deltaTime;
                if (gaugeBar != null)
                    gaugeBar.value = Mathf.Clamp01(1f - (holdTimer / holdTime));
                if (holdTimer >= holdTime)
                {
                    OpenDoor();
                }
            }
            else
            {
                holdTimer = 0f;
                if (gaugeBar != null)
                    gaugeBar.value = 1f;
            }
        }
        else
        {
            holdTimer = 0f;
            if (gaugeBar != null)
            {
                gaugeBar.value = 1f;
                gaugeBar.gameObject.SetActive(false);
            }
        }
    }

    void OpenDoor()
    {
        isDoorOpen = true;
        if (doorAnimator != null)
            doorAnimator.SetTrigger("Open");
        if (gaugeBar != null)
            gaugeBar.gameObject.SetActive(false);

        if (clearText != null)
            clearText.SetActive(true);

        // 2초 후 게임 종료
        Invoke("QuitGame", 2f);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerNear = true;
        Debug.Log("플레이어 접근");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerNear = false;
    }
}