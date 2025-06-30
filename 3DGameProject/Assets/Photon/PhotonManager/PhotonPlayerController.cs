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

    // ��Ʈ��ũ ����ȭ ����
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private float networkYRotation;
    private bool networkIsGrounded;
    private float networkVelocityY;

    // ���� ����
    private float lag;
    private Vector3 networkPositionVelocity;
    private float networkRotationVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (photonView.IsMine)
        {
            // ���� �÷��̾� ����
            SetupLocalPlayer();
        }
        else
        {
            // ���� �÷��̾� ����
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
        // ī�޶� Ȱ��ȭ
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
        }

        // ���� ��Ʈ�ѷ� Ȱ��ȭ
        if (weaponController != null)
        {
            weaponController.enabled = true;
        }

        // Ŀ�� ���
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetupRemotePlayer()
    {
        // ī�޶� ��Ȱ��ȭ
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(false);
        }

        // ���� ��Ʈ�ѷ� ��Ȱ��ȭ
        if (weaponController != null)
        {
            weaponController.enabled = false;
        }

        // ���� �÷��̾�� ���� ó�� ����
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
        // ���� üũ
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // �̵� �Է�
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // ������Ʈ
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // �߷�
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // ���� ȸ�� (ī�޶�)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        // ���� ȸ�� (�÷��̾�)
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
        // ��Ʈ��ũ ��ġ�� ����
        transform.position = Vector3.SmoothDamp(transform.position, networkPosition,
            ref networkPositionVelocity, lag);

        // ��Ʈ��ũ ȸ������ ����
        float newYRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, networkYRotation,
            ref networkRotationVelocity, lag);
        transform.rotation = Quaternion.Euler(0, newYRotation, 0);

        // ī�޶� ȸ�� ����ȭ
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = networkRotation;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // ������ ����
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(playerCamera != null ? playerCamera.transform.localRotation : Quaternion.identity);
            stream.SendNext(isGrounded);
            stream.SendNext(velocity.y);
        }
        else
        {
            // ������ ����
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkYRotation = networkRotation.eulerAngles.y;
            networkIsGrounded = (bool)stream.ReceiveNext();
            networkVelocityY = (float)stream.ReceiveNext();

            // ���� �ð� ���
            lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
        }
    }

    // RPC �޼����
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

        // ��� ó��
        Debug.Log("�÷��̾ ����߽��ϴ�.");

        // ������ ����
        StartCoroutine(RespawnAfterDelay(3f));
    }

    private System.Collections.IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // ������ ��ġ ����
        Vector3 respawnPosition = GetRandomSpawnPosition();
        transform.position = respawnPosition;

        // ü�� ȸ��
        currentHealth = maxHealth;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // ���� ����Ʈ���� ���� ����
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnPoints.Length > 0)
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
        }

        return Vector3.zero;
    }

    // ���� �޼����
    public bool IsLocalPlayer => photonView.IsMine;
    public Camera GetPlayerCamera() => playerCamera;
    public PhotonPlayerWeaponController GetWeaponController() => weaponController;
}
