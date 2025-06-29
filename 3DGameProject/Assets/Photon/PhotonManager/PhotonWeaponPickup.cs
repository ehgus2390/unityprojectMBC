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

        // ȸ�� ȿ��
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        // ���Ʒ� ������ ȿ��
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    public void PickupWeapon(PhotonPlayerWeaponController weaponController)
    {
        if (isPickedUp) return;

        // ���� �÷��̾ �ݱ� ó��
        if (photonView.IsMine)
        {
            // ���� �߰�
            weaponController.AddWeapon(weaponData);

            // �ݱ� ȿ�� ����
            photonView.RPC("CreatePickupEffect", RpcTarget.All);

            // ������Ʈ ����
            PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    private void CreatePickupEffect()
    {
        // �ݱ� ȿ�� ����
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, transform.position, transform.rotation);
            Destroy(effect, 2f);
        }

        // �ݱ� ���� ���
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
            // UI�� �ݱ� �޽��� ǥ��
            Debug.Log(pickupMessage);

            // �÷��̾ EŰ�� ������ �ݱ�
            PhotonPlayerController playerController = other.GetComponent<PhotonPlayerController>();
            if (playerController != null && playerController.IsLocalPlayer)
            {
                // �Է� ó��
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
                // �Է� ó��
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

    // ���� ������ ����
    public void SetWeaponData(PhotonPlayerWeaponController.Weapon data)
    {
        weaponData = data;
    }

    // ���� ������ ��ȯ
    public PhotonPlayerWeaponController.Weapon GetWeaponData() => weaponData;
}
