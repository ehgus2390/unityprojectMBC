using Photon.Pun;
using UnityEngine;

public class PhotonBullet : MonoBehaviourPun
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 50f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private LayerMask hitLayers = -1;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private AudioClip hitSound;

    private int shooterID;
    private Vector3 direction;
    private bool isInitialized = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // ���� �ð� �� �ڵ� ����
        if (photonView.IsMine)
        {
            Invoke(nameof(DestroyBullet), lifetime);
        }
    }

    void Update()
    {
        if (!isInitialized) return;

        // �Ѿ� �̵�
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    public void Initialize(float bulletSpeed, float bulletDamage, int shooterActorID)
    {
        speed = bulletSpeed;
        damage = bulletDamage;
        shooterID = shooterActorID;
        direction = transform.forward;
        isInitialized = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        // �ڽ��� ���� �ʵ��� üũ
        PhotonPlayerController playerController = other.GetComponent<PhotonPlayerController>();
        if (playerController != null && playerController.photonView.Owner.ActorNumber == shooterID)
        {
            return;
        }

        // �÷��̾�� ������
        if (other.CompareTag("Player"))
        {
            PhotonPlayerController targetPlayer = other.GetComponent<PhotonPlayerController>();
            if (targetPlayer != null)
            {
                targetPlayer.photonView.RPC("TakeDamage", RpcTarget.All, damage, shooterID);
            }
        }

        // AI���� ������
        AlienAI alien = other.GetComponent<AlienAI>();
        if (alien != null)
        {
            // AI ������ ó��
            Debug.Log($"Alien hit for {damage} damage");
        }

        // �浹 ȿ�� ����
        photonView.RPC("CreateHitEffect", RpcTarget.All, transform.position, transform.rotation);

        // �Ѿ� ����
        DestroyBullet();
    }

    [PunRPC]
    private void CreateHitEffect(Vector3 position, Quaternion rotation)
    {
        // �浹 ȿ�� ����
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, position, rotation);
            Destroy(effect, 2f);
        }

        // �浹 ���� ���
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }

    private void DestroyBullet()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    // �Ѿ� ���� ��ȯ
    public float GetDamage() => damage;
    public int GetShooterID() => shooterID;
}
