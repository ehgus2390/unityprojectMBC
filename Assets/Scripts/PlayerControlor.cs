using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlor : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip deathClip;

    [Header("Jump Setting")]
    [SerializeField] float jumpPower = 200.0f;

    private Rigidbody2D rb;
    //애니메이터 컴포넌트
    private Animator animator;
    private AudioSource audioSource;

    private int jumpCount = 0;
    private bool isGrounded = false;
    private bool isDead = false;

    private const int MAX_JUMP_COUNT = 2;

    private bool jumpRequested = false;
    private bool jumpReleasedEarly = false;
  

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }
    

    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            return;
        }
        MouseInput();
        animator.SetBool("Grounded", isGrounded);
    }
    private void FixedUpdate()
    {
        if (jumpRequested)
        {
            Jump();
            jumpRequested = false;
        }
        if (jumpReleasedEarly)
        {
            ShortJump();
            jumpReleasedEarly = false;
        }
    }
    private void MouseInput()
    {
        if (Input.GetMouseButtonDown(0) && jumpCount<MAX_JUMP_COUNT)
        {
            jumpRequested = true;
            jumpCount++;
        }
        if (Input.GetMouseButtonUp(0) &&rb.velocity.y>0)
        {
            jumpReleasedEarly = true;
        }
    }
    private void Jump()
    {
        rb.velocity = Vector3.zero;
        rb.AddForce(Vector2.up * jumpPower);
        audioSource.Play();
    }
    private void ShortJump()
    {
        rb.velocity *= 0.5f;
    }
    private void Die()
    {
        if(isDead) return;
        isDead = true;
        rb.velocity = Vector3.zero;
        animator.SetTrigger("Die");
        audioSource.clip = deathClip;
        audioSource.Play();
        GameManager.Instance.OnPlayerDead();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Dead"))
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.7f)
        {
            //어떤 콜라이더와 닿았으며 충돌 표면이 위쪽을 보고있으면
            isGrounded = true;
            jumpCount = 0;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}
