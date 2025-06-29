using Photon.Pun;
using UnityEngine;

public class PhotonWeaponPickup : MonoBehaviourPun
{
    [Header("Weapon Data")]
    [SerializeField] private PhotonPlayerWeaponController.Weapon weaponData;
    [SerializeField] private string pickupMessage = "Press E to pickup weapon";
    [SerializeField] private float pickupRange = 3f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;

    private Vector3 startPosition;
    private AudioSource audioSource;
    private bool isPickedUp = false;

    void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (isPickedUp) return;

        // 회전 효과
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        // 위아래 움직임 효과
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    public void PickupWeapon(PhotonPlayerWeaponController weaponController)
    {
        if (isPickedUp) return;

        // 로컬 플레이어만 줍기 처리
        if (photonView.IsMine)
        {
            // 무기 추가
            weaponController.AddWeapon(weaponData);

            // 줍기 효과 생성
            photonView.RPC("CreatePickupEffect", RpcTarget.All);

            // 오브젝트 제거
            PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    private void CreatePickupEffect()
    {
        // 줍기 효과 생성
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, transform.position, transform.rotation);
            Destroy(effect, 2f);
        }

        // 줍기 사운드 재생
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp) return;

        if (other.CompareTag("Player"))
        {
            // UI에 줍기 메시지 표시
            Debug.Log(pickupMessage);

            // 플레이어가 E키를 누르면 줍기
            PhotonPlayerController playerController = other.GetComponent<PhotonPlayerController>();
            if (playerController != null && playerController.IsLocalPlayer)
            {
                // 입력 처리
                if (Input.GetKeyDown(KeyCode.E))
                {
                    PhotonPlayerWeaponController weaponController = playerController.GetWeaponController();
                    if (weaponController != null)
                    {
                        PickupWeapon(weaponController);
                    }
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isPickedUp) return;

        if (other.CompareTag("Player"))
        {
            PhotonPlayerController playerController = other.GetComponent<PhotonPlayerController>();
            if (playerController != null && playerController.IsLocalPlayer)
            {
                // 입력 처리
                if (Input.GetKeyDown(KeyCode.E))
                {
                    PhotonPlayerWeaponController weaponController = playerController.GetWeaponController();
                    if (weaponController != null)
                    {
                        PickupWeapon(weaponController);
                    }
                }
            }
        }
    }

    // 무기 데이터 설정
    public void SetWeaponData(PhotonPlayerWeaponController.Weapon data)
    {
        weaponData = data;
    }

    // 무기 데이터 반환
    public PhotonPlayerWeaponController.Weapon GetWeaponData() => weaponData;
}
