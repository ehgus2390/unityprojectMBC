//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class GTFOPlayerController : MonoBehaviour
//{
//    [Header("Movement Settings")]
//    [SerializeField] private float walkSpeed = 3f;
//    [SerializeField] private float sprintSpeed = 6f;
//    [SerializeField] private float crouchSpeed = 1.5f;
//    [SerializeField] private float jumpForce = 5f;
//    [SerializeField] private float mouseSensitivity = 2f;

//    [Header("Stealth Settings")]
//    [SerializeField] private float noiseRadius = 5f;
//    [SerializeField] private float crouchNoiseRadius = 2f;
//    [SerializeField] private float sprintNoiseRadius = 8f;

//    private CharacterController controller;
//    private Camera playerCamera;
//    private float verticalRotation = 0f;
//    private bool isCrouching = false;
//    private bool isSprinting = false;
//    private Vector3 moveDirection = Vector3.zero;

//    private void Start()
//    {
//        controller = GetComponent<CharacterController>();
//        playerCamera = GetComponentInChildren<Camera>();
//        Cursor.lockState = CursorLockMode.Locked;
//    }

//    private void Update()
//    {
//        HandleMovement();
//        HandleRotation();
//        HandleActions();
//    }

//    private void HandleMovement()
//    {
//        float currentSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);

//        float moveX = Input.GetAxis("Horizontal");
//        float moveZ = Input.GetAxis("Vertical");

//        Vector3 move = transform.right * moveX + transform.forward * moveZ;
//        controller.Move(move * currentSpeed * Time.deltaTime);
//        // �߷� ����
//        if (controller.isGrounded)
//        {
//            moveDirection.y = -0.5f; // �ణ�� ��������� ���鿡 �ٵ���
//        }
//        else
//        {
//            moveDirection.y += Physics.gravity.y * Time.deltaTime;
//        }
//        // ����
//        if (Input.GetButtonDown("Jump") && controller.isGrounded)
//        {
//            moveDirection.y = jumpForce;
//        }

//        // �߷� ����
//        moveDirection.y += Physics.gravity.y * Time.deltaTime;
//        controller.Move(moveDirection * Time.deltaTime);
//    }

//    private void HandleRotation()
//    {
//        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
//        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

//        verticalRotation -= mouseY;
//        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

//        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
//        transform.Rotate(Vector3.up * mouseX);
//    }

//    private void HandleActions()
//    {
//        // �ɱ�
//        if (Input.GetKeyDown(KeyCode.LeftControl))
//        {
//            isCrouching = !isCrouching;
//            controller.height = isCrouching ? 1f : 2f;
//        }

//        // �޸���
//        isSprinting = Input.GetKey(KeyCode.LeftShift) && !isCrouching;

//        // ���� �ݰ� ���
//        float currentNoiseRadius = isCrouching ? crouchNoiseRadius :
//                                 (isSprinting ? sprintNoiseRadius : noiseRadius);

//        // ���� �̺�Ʈ �߻�
//        if (moveDirection.magnitude > 0.1f)
//        {
//            EmitNoise(currentNoiseRadius);
//        }
//    }

//    private void EmitNoise(float radius)
//    {
//        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
//        foreach (Collider collider in colliders)
//        {
//            if (collider.CompareTag("Enemy"))
//            {
//                EnemyAI enemy = collider.GetComponent<EnemyAI>();
//                if (enemy != null)
//                {
//                    enemy.DetectNoise(transform.position);
//                }
//            }
//        }
//    }
//}
