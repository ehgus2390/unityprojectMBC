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

        // 일정 시간 후 자동 제거
        if (photonView.IsMine)
        {
            Invoke(nameof(DestroyBullet), lifetime);
        }
    }

    void Update()
    {
        if (!isInitialized) return;

        // 총알 이동
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

        // 자신을 쏘지 않도록 체크
        PhotonPlayerController playerController = other.GetComponent<PhotonPlayerController>();
        if (playerController != null && playerController.photonView.Owner.ActorNumber == shooterID)
        {
            return;
        }

        // 플레이어에게 데미지
        if (other.CompareTag("Player"))
        {
            PhotonPlayerController targetPlayer = other.GetComponent<PhotonPlayerController>();
            if (targetPlayer != null)
            {
                targetPlayer.photonView.RPC("TakeDamage", RpcTarget.All, damage, shooterID);
            }
        }

        // AI에게 데미지
        AlienAI alien = other.GetComponent<AlienAI>();
        if (alien != null)
        {
            // AI 데미지 처리
            Debug.Log($"Alien hit for {damage} damage");
        }

        // 충돌 효과 생성
        photonView.RPC("CreateHitEffect", RpcTarget.All, transform.position, transform.rotation);

        // 총알 제거
        DestroyBullet();
    }

    [PunRPC]
    private void CreateHitEffect(Vector3 position, Quaternion rotation)
    {
        // 충돌 효과 생성
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, position, rotation);
            Destroy(effect, 2f);
        }

        // 충돌 사운드 재생
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

    // 총알 정보 반환
    public float GetDamage() => damage;
    public int GetShooterID() => shooterID;
}
