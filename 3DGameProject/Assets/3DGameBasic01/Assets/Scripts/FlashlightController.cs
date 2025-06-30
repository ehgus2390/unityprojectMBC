using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [SerializeField] private Light flashlight;
    [SerializeField] private float maxIntensity = 2f;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float flickerSpeed = 0.1f;
    [SerializeField] private float batteryLife = 100f;
    [SerializeField] private float batteryDrainRate = 5f;
    [SerializeField] private float batteryRechargeRate = 10f;
    [SerializeField] private float flickerChance = 0.1f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip toggleSound;
    [SerializeField] private AudioClip flickerSound;

    private bool isOn = false;
    private float currentBattery;
    private float nextFlickerTime;
    private bool isFlickering = false;

    private void Start()
    {
        if (flashlight == null)
        {
            flashlight = GetComponentInChildren<Light>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        currentBattery = batteryLife;
        flashlight.enabled = false;
    }

    private void Update()
    {
        // �÷��ö���Ʈ ���
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
        }

        if (isOn)
        {
            // ���͸� �Ҹ�
            currentBattery -= batteryDrainRate * Time.deltaTime;

            // ���͸��� �����ϸ� �ڵ����� ����
            if (currentBattery <= 0)
            {
                currentBattery = 0;
                TurnOff();
            }

            // �ø�Ŀ ȿ��
            if (Time.time >= nextFlickerTime && Random.value < flickerChance)
            {
                StartFlicker();
            }
        }
        else
        {
            // ���͸� ����
            currentBattery = Mathf.Min(currentBattery + batteryRechargeRate * Time.deltaTime, batteryLife);
        }

        // �ø�Ŀ ȿ�� ������Ʈ
        if (isFlickering)
        {
            flashlight.intensity = Random.Range(minIntensity, maxIntensity);
        }
    }

    public void ToggleFlashlight()
    {
        if (currentBattery > 0)
        {
            isOn = !isOn;
            flashlight.enabled = isOn;

            if (audioSource != null && toggleSound != null)
            {
                audioSource.PlayOneShot(toggleSound);
            }
        }
    }

    private void TurnOff()
    {
        isOn = false;
        flashlight.enabled = false;
        isFlickering = false;
    }

    private void StartFlicker()
    {
        isFlickering = true;
        nextFlickerTime = Time.time + flickerSpeed;

        if (audioSource != null && flickerSound != null)
        {
            audioSource.PlayOneShot(flickerSound);
        }

        Invoke("StopFlicker", flickerSpeed);
    }

    private void StopFlicker()
    {
        isFlickering = false;
        if (isOn)
        {
            flashlight.intensity = maxIntensity;
        }
    }

    public float GetBatteryPercentage()
    {
        return (currentBattery / batteryLife) * 100f;
    }
}
