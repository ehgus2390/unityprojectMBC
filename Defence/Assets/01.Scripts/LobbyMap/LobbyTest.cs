using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyTest : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float mouseSensitivity = 3f;

    private Rigidbody rb;
    private bool isGrounded;
    private float yRotation;

    public LayerMask groundLayerMask;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 마우스 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //  마우스 입력 → 회전 각도 누적
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        yRotation += mouseX;
        transform.rotation = Quaternion.Euler(0, yRotation, 0);

        // 이동 입력 → 바라보는 방향 기준
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S

        Vector3 move = (transform.right * h + transform.forward * v).normalized;
        Vector3 velocity = move * moveSpeed;
        velocity.y = rb.velocity.y; // 기존 Y속도 유지
        rb.velocity = velocity;

        //  점프
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayerMask.value) != 0)
        {
            isGrounded = true;
        }
    }
}
