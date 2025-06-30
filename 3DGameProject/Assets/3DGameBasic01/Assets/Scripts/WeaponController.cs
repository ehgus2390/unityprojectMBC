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
    [SerializeField] private Weapon[] weapons;            // ���� �迭
    [SerializeField] private Transform weaponHolder;      // ���⸦ �� ��ġ
    [SerializeField] private float weaponSwayAmount = 0.02f;
    [SerializeField] private float maxSwayAmount = 0.06f;
    [SerializeField] private float swaySmoothness = 3f;
    [SerializeField] private float switchSpeed = 0.3f;    // ���� ��ü �ӵ�

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;     // �Ѿ� ������
    [SerializeField] private LayerMask shootableLayers = -1; // �߻� ������ ���̾�

    [Header("UI Settings")]
    [SerializeField] private Image crosshair;             // ���ؼ�
    [SerializeField] private Text ammoText;               // ź�� ǥ�� �ؽ�Ʈ
    [SerializeField] private Text weaponNameText;         // ���� �̸� �ؽ�Ʈ
    [SerializeField] private Slider reloadProgressBar;    // ������ �����

    [Header("Weapon Pickup Settings")]
    [SerializeField] private float pickupRange = 3f;      // �ݱ� ����
    [SerializeField] private LayerMask pickupLayer = 1;   // �ݱ� ������ ���̾�

    private Camera playerCamera;
    private float nextFireTime;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private int currentWeaponIndex = 0;
    private bool isSwitching = false;
    private bool isReloading = false;
    private AudioSource audioSource;

    // �ݵ� ���� ����
    private Vector3 currentRecoil;
    private Vector3 targetRecoil;

    // Alien AI ���� ����
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

    // �߰� ����
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

        // ���� Ȧ���� ī�޶��� �ڽ����� ����
        if (weaponHolder != null && playerCamera != null)
        {
            weaponHolder.SetParent(playerCamera.transform);
            weaponHolder.localPosition = Vector3.zero;
            weaponHolder.localRotation = Quaternion.identity;
        }

        InitializeWeapons();
        navAgent = GetComponent<NavMeshAgent>();
        lastAttackTime = -attackCooldown;

        // FOV �ʱ�ȭ
        if (playerCamera != null)
        {
            originalFOV = playerCamera.fieldOfView;
        }

        // UI �ʱ�ȭ
        UpdateUI();
    }

    private void InitializeWeapons()
    {
        // ��� ���� ��Ȱ��ȭ
        foreach (Weapon weapon in weapons)
        {
            if (weapon.weaponModel != null)
            {
                weapon.weaponModel.SetActive(false);
                weapon.currentAmmo = weapon.maxAmmo;
            }
        }

        // ù ��° ���� Ȱ��ȭ
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

        // Alien AI ������Ʈ
        UpdateAlienAI();

        // UI ������Ʈ
        UpdateUI();
    }

    private void HandleWeaponSwitch()
    {
        // ���� Ű �Է� ó��
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

        // ���콺 �ٷ� ���� ��ȯ
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

        // ���� ���� �����
        if (currentWeapon.weaponModel != null)
        {
            currentWeapon.weaponModel.SetActive(false);
        }

        // �� ���� ���̱�
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
                // źâ�� ����� �� �Ҹ� ���
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

        // RŰ�� ������
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentWeapon.currentAmmo < currentWeapon.maxAmmo)
        {
            StartReload();
        }

        // ������ ����
        if (isReloading)
        {
            reloadTimer += Time.deltaTime;

            // ������ ����� ������Ʈ
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
        // ��Ŭ������ ����
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
            // �÷��̾� �ֺ��� ���� ������ �˻�
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

        // �ݵ� ����
        targetRecoil = Vector3.Lerp(targetRecoil, Vector3.zero, Time.deltaTime * currentWeapon.recoilRecoverySpeed);
        currentRecoil = Vector3.Lerp(currentRecoil, targetRecoil, Time.deltaTime * currentWeapon.recoilRecoverySpeed);

        // ���⿡ �ݵ� ����
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

        // �Ѿ� ����
        GameObject bullet = Instantiate(bulletPrefab, currentWeapon.firePoint.position, currentWeapon.firePoint.rotation);

        if (bullet.TryGetComponent<Bullet1>(out Bullet1 bulletComponent))
        {
            bulletComponent.Initialize(currentWeapon.bulletSpeed, currentWeapon.damage);
        }

        // �߻� ȿ��
        if (currentWeapon.muzzleFlash != null)
        {
            currentWeapon.muzzleFlash.Play();
        }

        // �߻� �Ҹ�
        if (audioSource != null && currentWeapon.shootSound != null)
        {
            audioSource.PlayOneShot(currentWeapon.shootSound);
        }

        // �ݵ� �߰�
        targetRecoil += currentWeapon.recoilRotation;

        // ź�� ����
        currentWeapon.currentAmmo--;
    }

    public void StartReload()
    {
        if (isReloading) return;

        Weapon currentWeapon = weapons[currentWeaponIndex];
        if (currentWeapon.currentAmmo >= currentWeapon.maxAmmo) return;

        isReloading = true;
        reloadTimer = 0f;

        // ������ �Ҹ�
        if (audioSource != null && currentWeapon.reloadSound != null)
        {
            audioSource.PlayOneShot(currentWeapon.reloadSound);
        }

        // ������ ����� ǥ��
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

        // ������ ����� �����
        if (reloadProgressBar != null)
        {
            reloadProgressBar.gameObject.SetActive(false);
        }
    }

    private void UpdateUI()
    {
        Weapon currentWeapon = weapons[currentWeaponIndex];

        // ź�� ǥ�� ������Ʈ
        if (ammoText != null)
        {
            ammoText.text = $"{currentWeapon.currentAmmo} / {currentWeapon.maxAmmo}";
        }

        // ���� �̸� ǥ�� ������Ʈ
        if (weaponNameText != null)
        {
            weaponNameText.text = currentWeapon.name;
        }

        // ���ؼ� ���� ���� (���� ���� ��)
        if (crosshair != null)
        {
            crosshair.color = isAiming ? Color.red : Color.white;
        }
    }

    // Alien AI ������Ʈ
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
                // ��� ���� ó��
                navAgent.isStopped = true;
                break;
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // ���� �ִϸ��̼� �Ǵ� ȿ��
        // animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackDelay);

        // �÷��̾�� ������ �ֱ�
        //if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        //{
        //    Player playerComponent = player.GetComponent<Player>();
        //    if (playerComponent != null)
        //    {
        //        playerComponent.TakeDamage(10f); // ������ �� ����
        //    }
        //}

        isAttacking = false;
    }

    // ���� �߰� �޼���
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

    // ���� ���� ���� ��ȯ
    public Weapon GetCurrentWeapon() => weapons[currentWeaponIndex];
    public bool IsReloading() => isReloading;
    public bool IsAiming() => isAiming;

    // Gizmos�� �ݱ� ���� ǥ��
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}

// ���� �ݱ� Ŭ����
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
            // UI�� �ݱ� �޽��� ǥ��
            Debug.Log(pickupMessage);
        }
    }
}