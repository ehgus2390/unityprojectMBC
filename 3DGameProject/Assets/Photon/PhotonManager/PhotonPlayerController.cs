using Photon.Pun;
using UnityEngine;

public class PhotonPlayerController : MonoBehaviourPun, IPunObservable
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Components")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PhotonPlayerWeaponController weaponController;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    private CharacterController controller;
    private float xRotation = 0f;
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;

    // 네트워크 동기화 변수
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private float networkYRotation;
    private bool networkIsGrounded;
    private float networkVelocityY;

    // 보간 변수
    private float lag;
    private Vector3 networkPositionVelocity;
    private float networkRotationVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (photonView.IsMine)
        {
            // 로컬 플레이어 설정
            SetupLocalPlayer();
        }
        else
        {
            // 원격 플레이어 설정
            SetupRemotePlayer();
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            HandleLocalPlayerInput();
        }
        else
        {
            HandleRemotePlayerMovement();
        }
    }

    private void SetupLocalPlayer()
    {
        // 카메라 활성화
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
        }

        // 무기 컨트롤러 활성화
        if (weaponController != null)
        {
            weaponController.enabled = true;
        }

        // 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetupRemotePlayer()
    {
        // 카메라 비활성화
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(false);
        }

        // 무기 컨트롤러 비활성화
        if (weaponController != null)
        {
            weaponController.enabled = false;
        }

        // 원격 플레이어는 물리 처리 안함
        if (controller != null)
        {
            controller.enabled = false;
        }
    }

    private void HandleLocalPlayerInput()
    {
        HandleMovement();
        HandleMouseLook();
        HandleJump();
    }

    private void HandleMovement()
    {
        // 지면 체크
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 이동 입력
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // 스프린트
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // 중력
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 수직 회전 (카메라)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        // 수평 회전 (플레이어)
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    private void HandleRemotePlayerMovement()
    {
        // 네트워크 위치로 보간
        transform.position = Vector3.SmoothDamp(transform.position, networkPosition,
            ref networkPositionVelocity, lag);

        // 네트워크 회전으로 보간
        float newYRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, networkYRotation,
            ref networkRotationVelocity, lag);
        transform.rotation = Quaternion.Euler(0, newYRotation, 0);

        // 카메라 회전 동기화
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = networkRotation;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 데이터 전송
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(playerCamera != null ? playerCamera.transform.localRotation : Quaternion.identity);
            stream.SendNext(isGrounded);
            stream.SendNext(velocity.y);
        }
        else
        {
            // 데이터 수신
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkYRotation = networkRotation.eulerAngles.y;
            networkIsGrounded = (bool)stream.ReceiveNext();
            networkVelocityY = (float)stream.ReceiveNext();

            // 지연 시간 계산
            lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
        }
    }

    // RPC 메서드들
    [PunRPC]
    public void TakeDamage(float damage, int attackerID)
    {
        if (!photonView.IsMine) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    [PunRPC]
    public void Die()
    {
        if (!photonView.IsMine) return;

        // 사망 처리
        Debug.Log("플레이어가 사망했습니다.");

        // 리스폰 로직
        StartCoroutine(RespawnAfterDelay(3f));
    }

    private System.Collections.IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 리스폰 위치 설정
        Vector3 respawnPosition = GetRandomSpawnPosition();
        transform.position = respawnPosition;

        // 체력 회복
        currentHealth = maxHealth;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // 스폰 포인트에서 랜덤 선택
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnPoints.Length > 0)
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
        }

        return Vector3.zero;
    }

    // 공개 메서드들
    public bool IsLocalPlayer => photonView.IsMine;
    public Camera GetPlayerCamera() => playerCamera;
    public PhotonPlayerWeaponController GetWeaponController() => weaponController;
}
