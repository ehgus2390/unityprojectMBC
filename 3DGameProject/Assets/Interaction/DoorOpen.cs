using UnityEngine;
using UnityEngine.UI; // �������ٸ� ������� �ʴ��� �ʿ��� �� �ֽ��ϴ�.

public class DoorOpen : MonoBehaviour
{
    public float holdTime = 7f;             // EŰ�� ������ �־�� �ϴ� �ð�(��)
    public Animator doorAnimator;           // (���� ������) �� Root Animator

    // (���� ����) �������ٸ� ����Ѵٸ� �ּ��� Ǯ�� ����Ƽ �����Ϳ��� �����ϼ���.
    public Slider gaugeBar; 

    private float holdTimer = 0f;
    private bool isPlayerNear = false;
    private bool isDoorOpen = false;

    void Start()
    {
        // (���� ����) �������� �ʱ�ȭ
        
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
        // �÷��̾ �� ��ó�� �ְ� ���� ���� ������ �ʾҴٸ�
        if (isPlayerNear && !isDoorOpen)
        {
            // (���� ����) �������� Ȱ��ȭ
            
            if (gaugeBar != null)
                gaugeBar.gameObject.SetActive(true);
            

            // E Ű�� ������ �ִ� ����
            if (Input.GetKey(KeyCode.E))
            {
                holdTimer += Time.deltaTime; // Ÿ�̸� ����

                // (���� ����) �������� �� ������Ʈ (�پ��� ���)
                
                if (gaugeBar != null)
                    gaugeBar.value = Mathf.Clamp01(1f - (holdTimer / holdTime));
                

                // ������ �ð��� �� ä���ٸ� �� ����
                if (holdTimer >= holdTime)
                {
                    OpenDoor(); // ���� �� ���� �Լ� ȣ��
                }
            }
            // E Ű�� ������ ���� �ʴٸ� Ÿ�̸� �ʱ�ȭ �� �������� �������
            else
            {
                holdTimer = 0f;
                // (���� ����) �������� �ʱ�ȭ
                
                if (gaugeBar != null)
                    gaugeBar.value = 1f;
                
            }
        }
        // �÷��̾ �� ��ó�� ���ų� ���� �̹� ���ȴٸ�
        else
        {
            holdTimer = 0f;
            // (���� ����) �������� ��Ȱ��ȭ �� �ʱ�ȭ
            
            if (gaugeBar != null)
            {
                gaugeBar.value = 1f;
                gaugeBar.gameObject.SetActive(false);
            }
            
        }
    }

    // ���� ���� �Լ� (�� ���� �����ϴ� �ϳ��� Animator Ʈ���� �ߵ�)
    void OpenDoor()
    {
        isDoorOpen = true; // ���� �������� ǥ��
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open"); // "Open" �ִϸ��̼� Ʈ���� �ߵ�
            Debug.Log("���� ���Ƚ��ϴ�!"); // �ֿܼ� �޽��� ���
        }

        // (���� ����) �������� ��Ȱ��ȭ
        
        if (gaugeBar != null)
            gaugeBar.gameObject.SetActive(false);
        

        // ���� ���� �� �� �̻� ��ȣ�ۿ��� �ʿ� ���ٸ� �� ��ũ��Ʈ�� ��Ȱ��ȭ�� ���� �ֽ��ϴ�.
        this.enabled = false; 
    }

    // �÷��̾ �ݶ��̴� ������ ������ ��
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            Debug.Log("�÷��̾� ���� - �� ���� ����");
        }
    }

    // �÷��̾ �ݶ��̴� �������� ������ ��
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            holdTimer = 0f; // ���� ������ ������ Ÿ�̸� �ʱ�ȭ
            // (���� ����) �������� ��Ȱ��ȭ �� �ʱ�ȭ
            
            if (gaugeBar != null)
            {
                gaugeBar.value = 1f;
                gaugeBar.gameObject.SetActive(false);
            }
            
            Debug.Log("�÷��̾� ��Ż - �� ���� ���");
        }
    }
}