using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image crosshair;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private Slider reloadProgressBar;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private GameObject pickupPrompt;
    [SerializeField] private TextMeshProUGUI pickupText;

    [Header("Crosshair Settings")]
    [SerializeField] private Color normalCrosshairColor = Color.white;
    [SerializeField] private Color aimingCrosshairColor = Color.red;
    [SerializeField] private Color reloadingCrosshairColor = Color.yellow;
    [SerializeField] private float crosshairSize = 20f;
    [SerializeField] private float aimingCrosshairSize = 15f;

    [Header("Animation Settings")]
    [SerializeField] private float uiAnimationSpeed = 5f;
    [SerializeField] private AnimationCurve reloadAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private WeaponController weaponController;
    private RectTransform crosshairRect;
    private Vector3 originalCrosshairScale;
    private bool isReloading = false;

    private void Start()
    {
        weaponController = FindObjectOfType<WeaponController>();
        if (crosshair != null)
        {
            crosshairRect = crosshair.GetComponent<RectTransform>();
            originalCrosshairScale = crosshairRect.localScale;
        }

        // �ʱ� UI �����
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }

        if (reloadProgressBar != null)
        {
            reloadProgressBar.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (weaponController != null)
        {
            UpdateAmmoDisplay();
            UpdateWeaponName();
            UpdateCrosshair();
            UpdateReloadProgress();
        }
    }

    private void UpdateAmmoDisplay()
    {
        if (ammoText != null)
        {
            var currentWeapon = weaponController.GetCurrentWeapon();
            ammoText.text = $"{currentWeapon.currentAmmo} / {currentWeapon.maxAmmo}";

            // ź���� ���� �� ���� ����
            if (currentWeapon.currentAmmo <= currentWeapon.maxAmmo * 0.2f)
            {
                ammoText.color = Color.red;
            }
            else if (currentWeapon.currentAmmo <= currentWeapon.maxAmmo * 0.5f)
            {
                ammoText.color = Color.yellow;
            }
            else
            {
                ammoText.color = Color.white;
            }
        }
    }

    private void UpdateWeaponName()
    {
        if (weaponNameText != null)
        {
            var currentWeapon = weaponController.GetCurrentWeapon();
            weaponNameText.text = currentWeapon.name;
        }
    }

    private void UpdateCrosshair()
    {
        if (crosshair == null || crosshairRect == null) return;

        bool isAiming = weaponController.IsAiming();
        bool isReloading = weaponController.IsReloading();

        // ���ؼ� ���� ����
        if (isReloading)
        {
            crosshair.color = reloadingCrosshairColor;
        }
        else if (isAiming)
        {
            crosshair.color = aimingCrosshairColor;
        }
        else
        {
            crosshair.color = normalCrosshairColor;
        }

        // ���ؼ� ũ�� ����
        float targetSize = isAiming ? aimingCrosshairSize : crosshairSize;
        Vector3 targetScale = originalCrosshairScale * (targetSize / crosshairSize);

        crosshairRect.localScale = Vector3.Lerp(
            crosshairRect.localScale,
            targetScale,
            Time.deltaTime * uiAnimationSpeed
        );
    }

    private void UpdateReloadProgress()
    {
        if (reloadProgressBar == null) return;

        bool isReloading = weaponController.IsReloading();

        if (isReloading && !this.isReloading)
        {
            // ������ ����
            this.isReloading = true;
            reloadProgressBar.gameObject.SetActive(true);
            reloadProgressBar.value = 0f;
        }
        else if (!isReloading && this.isReloading)
        {
            // ������ �Ϸ�
            this.isReloading = false;
            reloadProgressBar.gameObject.SetActive(false);
        }
    }

    public void ShowPickupPrompt(string weaponName)
    {
        if (pickupPrompt != null && pickupText != null)
        {
            pickupText.text = $"Press E to pickup {weaponName}";
            pickupPrompt.SetActive(true);
        }
    }

    public void HidePickupPrompt()
    {
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }
    }

    public void UpdateWeaponIcon(Sprite icon)
    {
        if (weaponIcon != null && icon != null)
        {
            weaponIcon.sprite = icon;
        }
    }

    public void ShowReloadProgress(float progress)
    {
        if (reloadProgressBar != null)
        {
            reloadProgressBar.value = reloadAnimationCurve.Evaluate(progress);
        }
    }

    // ���� ��ȯ �� �ִϸ��̼�
    public void PlayWeaponSwitchAnimation()
    {
        StartCoroutine(WeaponSwitchAnimation());
    }

    private System.Collections.IEnumerator WeaponSwitchAnimation()
    {
        if (weaponNameText == null) yield break;

        // ���̵� �ƿ�
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / duration);
            Color color = weaponNameText.color;
            color.a = alpha;
            weaponNameText.color = color;
            yield return null;
        }

        // ���̵� ��
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = elapsed / duration;
            Color color = weaponNameText.color;
            color.a = alpha;
            weaponNameText.color = color;
            yield return null;
        }

        // ���İ� ����
        Color finalColor = weaponNameText.color;
        finalColor.a = 1f;
        weaponNameText.color = finalColor;
    }

    // ź�� ���� �� ��� ȿ��
    public void PlayLowAmmoWarning()
    {
        StartCoroutine(LowAmmoWarningAnimation());
    }

    private System.Collections.IEnumerator LowAmmoWarningAnimation()
    {
        if (ammoText == null) yield break;

        Color originalColor = ammoText.color;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Sin(elapsed * 10f) * 0.5f + 0.5f;
            ammoText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        ammoText.color = originalColor;
    }
}
