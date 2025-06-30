using UnityEngine;
using UnityEngine.UI;

public class SniperRifle : MonoBehaviour
{
    [Header("총알/발사")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public int magazineSize = 5;
    public float reloadTime = 2f;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;
    public ParticleSystem muzzleFlash;
    public GameObject fireEffect;

    [Header("조준")]
    public Camera playerCamera;
    public float normalFOV = 60f;
    public float zoomFOV = 20f;
    public float zoomSpeed = 10f;
    public GameObject scopeUI;
    public GameObject crosshairUI;

    [Header("UI")]
    public Text ammoText;

    private int currentAmmo;
    private bool isReloading = false;
    private bool isAiming = false;
    private float nextFireTime = 0f;
    private AudioSource audioSource;
    private float originalFOV;

    void Start()
    {
        currentAmmo = magazineSize;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        if (playerCamera != null)
            originalFOV = playerCamera.fieldOfView;
        UpdateUI();
    }

    void Update()
    {
        HandleAiming();
        HandleShooting();
        HandleReload();
        HandleAimingRaycast();
    }

    void HandleAiming()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
            if (scopeUI != null) scopeUI.SetActive(true);
            if (crosshairUI != null) crosshairUI.SetActive(false);
        }
        if (Input.GetMouseButtonUp(1))
        {
            isAiming = false;
            if (scopeUI != null) scopeUI.SetActive(false);
            if (crosshairUI != null) crosshairUI.SetActive(true);
        }

        if (playerCamera != null)
        {
            float targetFOV = isAiming ? zoomFOV : normalFOV;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        }
    }
    void HandleAimingRaycast()
    {
        if (isAiming && playerCamera != null)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f))
            {
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);
                Debug.Log($"조준 지점: {hit.point}, 오브젝트: {hit.collider.gameObject.name}");
            }
            else
            {
                Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.yellow);
                Debug.Log("조준 지점: 아무것도 맞지 않음");
            }
        }
    }
    void HandleShooting()
    {
        if (isReloading) return;
        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Shoot();
                nextFireTime = Time.time + 1f / fireRate;
            }
            else
            {
                if (audioSource != null && emptySound != null)
                    audioSource.PlayOneShot(emptySound);
                StartReload();
            }
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
        currentAmmo--;
        if (audioSource != null && fireSound != null)
            audioSource.PlayOneShot(fireSound);
        if (muzzleFlash != null)
            muzzleFlash.Play();
        if (fireEffect != null)
            fireEffect.SetActive(true);
        UpdateUI();
    }

    void HandleReload()
    {
        if (isReloading) return;
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < magazineSize)
        {
            StartReload();
        }
    }

    void StartReload()
    {
        if (!isReloading)
            StartCoroutine(ReloadRoutine());
    }

    System.Collections.IEnumerator ReloadRoutine()
    {
        isReloading = true;
        if (audioSource != null && reloadSound != null)
            audioSource.PlayOneShot(reloadSound);
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = magazineSize;
        isReloading = false;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (ammoText != null)
            ammoText.text = $"{currentAmmo} / {magazineSize}";
    }
}
