using UnityEngine;

public class GTFOPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    [Header("Stealth Settings")]
    [SerializeField] private float noiseRadius = 5f;
    [SerializeField] private float crouchNoiseRadius = 2f;
    [SerializeField] private float sprintNoiseRadius = 8f;

    [Header("Weapon Settings")]
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private float reloadTime = 2f;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptySound;

    private CharacterController controller;
    private Camera playerCamera;
    private AudioSource audioSource;
    private float verticalRotation = 0f;
    private bool isCrouching = false;
    private bool isSprinting = false;
    private bool isReloading = false;
    private Vector3 moveDirection = Vector3.zero;
    private float currentHeight;
    private float targetHeight;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        Cursor.lockState = CursorLockMode.Locked;
        currentHeight = standHeight;
        targetHeight = standHeight;
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleActions();
        UpdateCrouchHeight();
    }

    private void HandleMovement()
    {
        float currentSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // 이동 방향 정규화
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        // 중력 적용
        if (controller.isGrounded)
        {
            moveDirection.y = -0.5f;
        }
        else
        {
            moveDirection.y += Physics.gravity.y * Time.deltaTime;
        }

        // 점프
        if (Input.GetButtonDown("Jump") && controller.isGrounded && !isCrouching)
        {
            moveDirection.y = jumpForce;
            // 점프 소리 재생
            if (audioSource != null)
            {
                audioSource.PlayOneShot(emptySound);
            }
        }

        controller.Move(moveDirection * Time.deltaTime);
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleActions()
    {
        // 앉기
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
            targetHeight = isCrouching ? crouchHeight : standHeight;
        }

        // 달리기
        isSprinting = Input.GetKey(KeyCode.LeftShift) && !isCrouching;

        // 재장전
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && weaponController != null)
        {
            StartReload();
        }

        // 소음 반경 계산
        float currentNoiseRadius = isCrouching ? crouchNoiseRadius :
                                 (isSprinting ? sprintNoiseRadius : noiseRadius);

        // 소음 이벤트 발생
        if (moveDirection.magnitude > 0.1f)
        {
            EmitNoise(currentNoiseRadius);
        }
    }

    private void UpdateCrouchHeight()
    {
        // 부드러운 앉기/일어서기 전환
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        controller.height = currentHeight;

        // 카메라 위치 조정
        Vector3 cameraPos = playerCamera.transform.localPosition;
        cameraPos.y = currentHeight - 0.5f;
        playerCamera.transform.localPosition = cameraPos;
    }

    private void StartReload()
    {
        if (isReloading) return;

        isReloading = true;
        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        // 재장전 애니메이션 및 효과
        if (weaponController != null)
        {
            weaponController.StartReload();
        }

        // 재장전 시간 후 완료
        Invoke(nameof(FinishReload), reloadTime);
    }

    private void FinishReload()
    {
        isReloading = false;
        if (weaponController != null)
        {
            weaponController.FinishReload();
        }
    }

    private void EmitNoise(float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                EnemyAI enemy = collider.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.DetectNoise(transform.position);
                }
            }
        }
    }

    // 현재 상태 확인을 위한 public 메서드들
    public bool IsCrouching() => isCrouching;
    public bool IsSprinting() => isSprinting;
    public bool IsReloading() => isReloading;
}