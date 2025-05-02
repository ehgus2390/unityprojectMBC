using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayetController : MonoBehaviour
{


    [Header("Audio")]
    [SerializeField] private AudioClip deathClip;

    [Header("Jump Setting")]
    [SerializeField] float jumpPower = 500.0f;


    private Rigidbody2D rb;
    //�ִϸ����� ������Ʈ
    private Animator animator;
    private AudioSource audioSource;

    private int jumpCount = 0;
    private bool isGrounded = false;
    private bool isDead = false;

    private const int MAX_JUMP_COUNT = 2;

    //�Է� ���� ����
    private bool jumpRequested = false;
    private bool jumpReleasedEarly = false;

    /// <summary>
    /// ������Ʈ ������
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
     
    }


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
        if(jumpRequested)
        {
            Jump();
            jumpRequested = false;
        }
        if(jumpReleasedEarly)
        {
            ShortJump();
            jumpReleasedEarly = false;
        }
    }
    private void MouseInput()
    {
        if(Input.GetMouseButtonDown(0)&& jumpCount<MAX_JUMP_COUNT)
        {
            jumpRequested = true;
            jumpCount++;
        }
        if(Input.GetMouseButtonUp(0)&&rb.velocity.y>0)
        {
            jumpReleasedEarly = true;   
        }
    }
    private void Jump()
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * jumpPower);
        audioSource.Play();
    }
    private void ShortJump()
    {
        rb.velocity *= 0.5f;
    }
    private void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.velocity = Vector2.zero; 

        animator.SetTrigger("Die");
        audioSource.clip = deathClip;
        audioSource.Play();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Dead"))
        {
            Die();
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //� �ݶ��̴��� ������� �浹ǥ���� ������ ���� ������
        if (collision.contacts[0].normal.y > 0.7f)
        {

            isGrounded = true;
            jumpCount = 0;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}
