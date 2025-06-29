using Photon.Pun;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PhotonPlayerWeaponController : MonoBehaviourPun, IPunObservable
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
    [SerializeField] private Weapon[] weapons;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private float weaponSwayAmount = 0.02f;
    [SerializeField] private float maxSwayAmount = 0.06f;
    [SerializeField] private float swaySmoothness = 3f;
    [SerializeField] private float switchSpeed = 0.3f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private LayerMask shootableLayers = -1;

    [Header("UI Settings")]
    [SerializeField] private Image crosshair;
    [SerializeField] private Text ammoText;
    [SerializeField] private Text weaponNameText;
    [SerializeField] private Slider reloadProgressBar;

    [Header("Weapon Pickup Settings")]
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask pickupLayer = 1;

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

    // �߰� ����
    private float reloadTimer;
    private bool isAiming = false;
    private float originalFOV;
    private float aimFOV = 40f;

    // ��Ʈ��ũ ����ȭ ����
    private int networkCurrentWeaponIndex;
    private bool networkIsReloading;
    private int networkCurrentAmmo;
    private bool networkIsAiming;

    void Start()
    {
        if (!photonView.IsMine) return;

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

        // FOV �ʱ�ȭ
        if (playerCamera != null)
        {
            originalFOV = playerCamera.fieldOfView;
        }

        // UI �ʱ�ȭ
        UpdateUI();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

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

        // UI ������Ʈ
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

    private IEnumerator SwitchWeapon(int newIndex)
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
                // ��Ʈ��ũ �߻�
                photonView.RPC("Shoot", RpcTarget.All);
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

    [PunRPC]
    private void Shoot()
    {
        Weapon currentWeapon = weapons[currentWeaponIndex];
        if (currentWeapon.firePoint == null) return;

        // �Ѿ� ���� (��Ʈ��ũ ������Ʈ)
        GameObject bullet = PhotonNetwork.Instantiate(bulletPrefab.name,
            currentWeapon.firePoint.position, currentWeapon.firePoint.rotation);

        if (bullet.TryGetComponent<PhotonBullet>(out PhotonBullet bulletComponent))
        {
            bulletComponent.Initialize(currentWeapon.bulletSpeed, currentWeapon.damage, photonView.Owner.ActorNumber);
        }

        // �߻� ȿ��
        if (currentWeapon.muzzleFlash != null)
        {
            currentWeapon.muzzleFlash.Play();
        }

        // �߻� �Ҹ� (���ÿ�����)
        if (photonView.IsMine && audioSource != null && currentWeapon.shootSound != null)
        {
            audioSource.PlayOneShot(currentWeapon.shootSound);
        }

        // �ݵ� �߰� (���ÿ�����)
        if (photonView.IsMine)
        {
            targetRecoil += currentWeapon.recoilRotation;
        }

        // ź�� ����
        currentWeapon.currentAmmo--;
    }

    private void HandleReload()
    {
        Weapon currentWeapon = weapons[currentWeaponIndex];

        // RŰ�� ������
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentWeapon.currentAmmo < currentWeapon.maxAmmo)
        {
            photonView.RPC("StartReload", RpcTarget.All);
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
                photonView.RPC("FinishReload", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    private void StartReload()
    {
        if (isReloading) return;

        Weapon currentWeapon = weapons[currentWeaponIndex];
        if (currentWeapon.currentAmmo >= currentWeapon.maxAmmo) return;

        isReloading = true;
        reloadTimer = 0f;

        // ������ �Ҹ� (���ÿ�����)
        if (photonView.IsMine && audioSource != null && currentWeapon.reloadSound != null)
        {
            audioSource.PlayOneShot(currentWeapon.reloadSound);
        }

        // ������ ����� ǥ��
        if (photonView.IsMine && reloadProgressBar != null)
        {
            reloadProgressBar.gameObject.SetActive(true);
            reloadProgressBar.value = 0f;
        }
    }

    [PunRPC]
    private void FinishReload()
    {
        if (!isReloading) return;

        Weapon currentWeapon = weapons[currentWeaponIndex];
        currentWeapon.currentAmmo = currentWeapon.maxAmmo;
        isReloading = false;
        reloadTimer = 0f;

        // ������ ����� �����
        if (photonView.IsMine && reloadProgressBar != null)
        {
            reloadProgressBar.gameObject.SetActive(false);
        }
    }

    private void HandleAiming()
    {
        // ��Ŭ������ ����
        if (Input.GetMouseButtonDown(1))
        {
            photonView.RPC("SetAiming", RpcTarget.All, true);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            photonView.RPC("SetAiming", RpcTarget.All, false);
        }
    }

    [PunRPC]
    private void SetAiming(bool aiming)
    {
        isAiming = aiming;

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = aiming ? aimFOV : originalFOV;
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
                PhotonWeaponPickup pickup = col.GetComponent<PhotonWeaponPickup>();
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

    private void UpdateUI()
    {
        if (!photonView.IsMine) return;

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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // ������ ����
            stream.SendNext(currentWeaponIndex);
            stream.SendNext(isReloading);
            stream.SendNext(weapons[currentWeaponIndex].currentAmmo);
            stream.SendNext(isAiming);
        }
        else
        {
            // ������ ����
            networkCurrentWeaponIndex = (int)stream.ReceiveNext();
            networkIsReloading = (bool)stream.ReceiveNext();
            networkCurrentAmmo = (int)stream.ReceiveNext();
            networkIsAiming = (bool)stream.ReceiveNext();
        }
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
}