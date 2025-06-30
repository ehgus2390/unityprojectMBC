using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.UI;
using Photon.Realtime;

public class WeaponController : MonoBehaviour
{
    [System.Serializable]
    public class Weapon
    {
        public string name;
        public GameObject weaponModel;
        public Transform firePoint;
        public float fireRate = 0.1f;
        public float bulletSpeed = 50f;
        public float damage = 10f;
        public int maxAmmo = 30;
        public int currentAmmo;
        public float reloadTime = 2f;
        public AudioClip shootSound;
        public AudioClip reloadSound;
        public AudioClip emptySound;
        public ParticleSystem muzzleFlash;

        [Header("Recoil Settings")]
        public float recoilForce = 2f;
        public float recoilRecoverySpeed = 5f;
        public Vector3 recoilRotation = new Vector3(-5f, 0f, 0f);
    }

    [Header("Weapon Settings")]
    [SerializeField] private Weapon[] weapons;            // 무기 배열
    [SerializeField] private Transform weaponHolder;      // 무기를 들 위치
    [SerializeField] private float weaponSwayAmount = 0.02f;
    [SerializeField] private float maxSwayAmount = 0.06f;
    [SerializeField] private float swaySmoothness = 3f;
    [SerializeField] private float switchSpeed = 0.3f;    // 무기 교체 속도

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;     // 총알 프리팹
    [SerializeField] private LayerMask shootableLayers = -1; // 발사 가능한 레이어

    [Header("UI Settings")]
    [SerializeField] private Image crosshair;             // 조준선
    [SerializeField] private Text ammoText;               // 탄약 표시 텍스트
    [SerializeField] private Text weaponNameText;         // 무기 이름 텍스트
    [SerializeField] private Slider reloadProgressBar;    // 재장전 진행바

    [Header("Weapon Pickup Settings")]
    [SerializeField] private float pickupRange = 3f;      // 줍기 범위
    [SerializeField] private LayerMask pickupLayer = 1;   // 줍기 가능한 레이어

    private Camera playerCamera;
    private float nextFireTime;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private int currentWeaponIndex = 0;
    private bool isSwitching = false;
    private bool isReloading = false;
    private AudioSource audioSource;

    // 반동 관련 변수
    private Vector3 currentRecoil;
    private Vector3 targetRecoil;

    // Alien AI 관련 변수
    public NavMeshAgent navAgent;
    public enum AlienState { Idle, Chase, Attack, Dead }
    public AlienState currentState = AlienState.Idle;
    public Transform player;
    public float chaseDistance = 10f;
    public float attackDistance = 2f;
    public float attackCooldown = 2f;
    public float attackDelay = 1.5f;
    private bool isAttacking = false;
    private float lastAttackTime;

    // 추가 변수
    private float reloadTimer;
    private bool isAiming = false;
    private float originalFOV;
    private float aimFOV = 40f;

    private void Start()
    {
        playerCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 무기 홀더를 카메라의 자식으로 설정
        if (weaponHolder != null && playerCamera != null)
        {
            weaponHolder.SetParent(playerCamera.transform);
            weaponHolder.localPosition = Vector3.zero;
            weaponHolder.localRotation = Quaternion.identity;
        }

        InitializeWeapons();
        navAgent = GetComponent<NavMeshAgent>();
        lastAttackTime = -attackCooldown;

        // FOV 초기화
        if (playerCamera != null)
        {
            originalFOV = playerCamera.fieldOfView;
        }

        // UI 초기화
        UpdateUI();
    }

    private void InitializeWeapons()
    {
        // 모든 무기 비활성화
        foreach (Weapon weapon in weapons)
        {
            if (weapon.weaponModel != null)
            {
                weapon.weaponModel.SetActive(false);
                weapon.currentAmmo = weapon.maxAmmo;
            }
        }

        // 첫 번째 무기 활성화
        if (weapons.Length > 0 && weapons[0].weaponModel != null)
        {
            weapons[0].weaponModel.SetActive(true);
            initialPosition = weapons[0].weaponModel.transform.localPosition;
            initialRotation = weapons[0].weaponModel.transform.localRotation;
        }
    }

    private void Update()
    {
        if (!isSwitching)
        {
            HandleWeaponSwitch();
            HandleWeaponSway();
            HandleShooting();
            HandleReload();
            HandleAiming();
            HandleWeaponPickup();
            HandleRecoil();
        }

        // Alien AI 업데이트
        UpdateAlienAI();

        // UI 업데이트
        UpdateUI();
    }

    private void HandleWeaponSwitch()
    {
        // 숫자 키 입력 처리
        if (Input.GetKeyDown(KeyCode.Alpha1) && currentWeaponIndex != 0)
        {
            StartCoroutine(SwitchWeapon(0));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && currentWeaponIndex != 1)
        {
            StartCoroutine(SwitchWeapon(1));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && currentWeaponIndex != 2)
        {
            StartCoroutine(SwitchWeapon(2));
        }

        // 마우스 휠로 무기 전환
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0.1f)
        {
            int nextIndex = (currentWeaponIndex + 1) % weapons.Length;
            if (nextIndex != currentWeaponIndex)
            {
                StartCoroutine(SwitchWeapon(nextIndex));
            }
        }
        else if (scroll < -0.1f)
        {
            int prevIndex = (currentWeaponIndex - 1 + weapons.Length) % weapons.Length;
            if (prevIndex != currentWeaponIndex)
            {
                StartCoroutine(SwitchWeapon(prevIndex));
            }
        }
    }

    private System.Collections.IEnumerator SwitchWeapon(int newIndex)
    {
        isSwitching = true;
        Weapon currentWeapon = weapons[currentWeaponIndex];
        Weapon newWeapon = weapons[newIndex];

        // 현재 무기 숨기기
        if (currentWeapon.weaponModel != null)
        {
            currentWeapon.weaponModel.SetActive(false);
        }

        // 새 무기 보이기
        if (newWeapon.weaponModel != null)
        {
            newWeapon.weaponModel.SetActive(true);
            initialPosition = newWeapon.weaponModel.transform.localPosition;
            initialRotation = newWeapon.weaponModel.transform.localRotation;
        }

        currentWeaponIndex = newIndex;
        yield return new WaitForSeconds(switchSpeed);
        isSwitching = false;
    }

    private void HandleWeaponSway()
    {
        if (weapons[currentWeaponIndex].weaponModel == null) return;

        float mouseX = Input.GetAxis("Mouse X") * weaponSwayAmount;
        float mouseY = Input.GetAxis("Mouse Y") * weaponSwayAmount;

        mouseX = Mathf.Clamp(mouseX, -maxSwayAmount, maxSwayAmount);
        mouseY = Mathf.Clamp(mouseY, -maxSwayAmount, maxSwayAmount);

        Vector3 targetPosition = new Vector3(
            initialPosition.x + mouseX,
            initialPosition.y + mouseY,
            initialPosition.z
        );

        weapons[currentWeaponIndex].weaponModel.transform.localPosition = Vector3.Lerp(
            weapons[currentWeaponIndex].weaponModel.transform.localPosition,
            targetPosition,
            Time.deltaTime * swaySmoothness
        );
    }

    private void HandleShooting()
    {
        Weapon currentWeapon = weapons[currentWeaponIndex];

        if (Input.GetMouseButton(0) && Time.time >= nextFireTime && !isReloading)
        {
            if (currentWeapon.currentAmmo > 0)
            {
                Shoot();
                nextFireTime = Time.time + currentWeapon.fireRate;
            }
            else
            {
                // 탄창이 비었을 때 소리 재생
                if (audioSource != null && currentWeapon.emptySound != null)
                {
                    audioSource.PlayOneShot(currentWeapon.emptySound);
                }
            }
        }
    }

    private void HandleReload()
    {
        Weapon currentWeapon = weapons[currentWeaponIndex];

        // R키로 재장전
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentWeapon.currentAmmo < currentWeapon.maxAmmo)
        {
            StartReload();
        }

        // 재장전 진행
        if (isReloading)
        {
            reloadTimer += Time.deltaTime;

            // 재장전 진행바 업데이트
            if (reloadProgressBar != null)
            {
                reloadProgressBar.value = reloadTimer / currentWeapon.reloadTime;
            }

            if (reloadTimer >= currentWeapon.reloadTime)
            {
                FinishReload();
            }
        }
    }

    private void HandleAiming()
    {
        // 우클릭으로 조준
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = aimFOV;
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isAiming = false;
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = originalFOV;
            }
        }
    }

    private void HandleWeaponPickup()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 플레이어 주변의 무기 아이템 검색
            Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRange, pickupLayer);

            foreach (Collider col in colliders)
            {
                WeaponPickup pickup = col.GetComponent<WeaponPickup>();
                if (pickup != null)
                {
                    pickup.PickupWeapon(this);
                    break;
                }
            }
        }
    }

    private void HandleRecoil()
    {
        Weapon currentWeapon = weapons[currentWeaponIndex];

        // 반동 복구
        targetRecoil = Vector3.Lerp(targetRecoil, Vector3.zero, Time.deltaTime * currentWeapon.recoilRecoverySpeed);
        currentRecoil = Vector3.Lerp(currentRecoil, targetRecoil, Time.deltaTime * currentWeapon.recoilRecoverySpeed);

        // 무기에 반동 적용
        if (weapons[currentWeaponIndex].weaponModel != null)
        {
            weapons[currentWeaponIndex].weaponModel.transform.localRotation =
                initialRotation * Quaternion.Euler(currentRecoil);
        }
    }

    private void Shoot()
    {
        Weapon currentWeapon = weapons[currentWeaponIndex];
        if (currentWeapon.firePoint == null) return;

        // 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, currentWeapon.firePoint.position, currentWeapon.firePoint.rotation);

        if (bullet.TryGetComponent<Bullet1>(out Bullet1 bulletComponent))
        {
            bulletComponent.Initialize(currentWeapon.bulletSpeed, currentWeapon.damage);
        }

        // 발사 효과
        if (currentWeapon.muzzleFlash != null)
        {
            currentWeapon.muzzleFlash.Play();
        }

        // 발사 소리
        if (audioSource != null && currentWeapon.shootSound != null)
        {
            audioSource.PlayOneShot(currentWeapon.shootSound);
        }

        // 반동 추가
        targetRecoil += currentWeapon.recoilRotation;

        // 탄약 감소
        currentWeapon.currentAmmo--;
    }

    public void StartReload()
    {
        if (isReloading) return;

        Weapon currentWeapon = weapons[currentWeaponIndex];
        if (currentWeapon.currentAmmo >= currentWeapon.maxAmmo) return;

        isReloading = true;
        reloadTimer = 0f;

        // 재장전 소리
        if (audioSource != null && currentWeapon.reloadSound != null)
        {
            audioSource.PlayOneShot(currentWeapon.reloadSound);
        }

        // 재장전 진행바 표시
        if (reloadProgressBar != null)
        {
            reloadProgressBar.gameObject.SetActive(true);
            reloadProgressBar.value = 0f;
        }
    }

    public void FinishReload()
    {
        if (!isReloading) return;

        Weapon currentWeapon = weapons[currentWeaponIndex];
        currentWeapon.currentAmmo = currentWeapon.maxAmmo;
        isReloading = false;
        reloadTimer = 0f;

        // 재장전 진행바 숨기기
        if (reloadProgressBar != null)
        {
            reloadProgressBar.gameObject.SetActive(false);
        }
    }

    private void UpdateUI()
    {
        Weapon currentWeapon = weapons[currentWeaponIndex];

        // 탄약 표시 업데이트
        if (ammoText != null)
        {
            ammoText.text = $"{currentWeapon.currentAmmo} / {currentWeapon.maxAmmo}";
        }

        // 무기 이름 표시 업데이트
        if (weaponNameText != null)
        {
            weaponNameText.text = currentWeapon.name;
        }

        // 조준선 색상 변경 (조준 중일 때)
        if (crosshair != null)
        {
            crosshair.color = isAiming ? Color.red : Color.white;
        }
    }

    // Alien AI 업데이트
    private void UpdateAlienAI()
    {
        if (player == null || navAgent == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case AlienState.Idle:
                if (distanceToPlayer <= chaseDistance)
                {
                    currentState = AlienState.Chase;
                }
                break;

            case AlienState.Chase:
                navAgent.SetDestination(player.position);

                if (distanceToPlayer <= attackDistance)
                {
                    currentState = AlienState.Attack;
                }
                else if (distanceToPlayer > chaseDistance)
                {
                    currentState = AlienState.Idle;
                }
                break;

            case AlienState.Attack:
                if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
                {
                    StartCoroutine(PerformAttack());
                }

                if (distanceToPlayer > attackDistance)
                {
                    currentState = AlienState.Chase;
                }
                break;

            case AlienState.Dead:
                // 사망 상태 처리
                navAgent.isStopped = true;
                break;
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // 공격 애니메이션 또는 효과
        // animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackDelay);

        // 플레이어에게 데미지 주기
        //if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        //{
        //    Player playerComponent = player.GetComponent<Player>();
        //    if (playerComponent != null)
        //    {
        //        playerComponent.TakeDamage(10f); // 데미지 값 조정
        //    }
        //}

        isAttacking = false;
    }

    // 무기 추가 메서드
    public void AddWeapon(Weapon newWeapon)
    {
        System.Array.Resize(ref weapons, weapons.Length + 1);
        weapons[weapons.Length - 1] = newWeapon;

        if (newWeapon.weaponModel != null)
        {
            newWeapon.weaponModel.SetActive(false);
            newWeapon.currentAmmo = newWeapon.maxAmmo;
        }
    }

    // 현재 무기 정보 반환
    public Weapon GetCurrentWeapon() => weapons[currentWeaponIndex];
    public bool IsReloading() => isReloading;
    public bool IsAiming() => isAiming;

    // Gizmos로 줍기 범위 표시
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}

// 무기 줍기 클래스
public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private WeaponController.Weapon weaponData;
    [SerializeField] private string pickupMessage = "Press E to pickup weapon";

    public void PickupWeapon(WeaponController weaponController)
    {
        weaponController.AddWeapon(weaponData);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // UI에 줍기 메시지 표시
            Debug.Log(pickupMessage);
        }
    }
}