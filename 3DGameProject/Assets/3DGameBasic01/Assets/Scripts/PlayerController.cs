using TheDeveloperTrain.SciFiGuns;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    public Animator animator;
    public Weapon weapon; // ���� ���� ����
    private bool isBusy = false; // �ൿ �� ����
    public Gun gun;

    void Update()
    {
        // ��� ���¿����� �ƹ��͵� ����
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Death_A"))
            return;

        // �⺻ �̵�(���⼭�� Grounded ���¸� �׻� true�� ����)
        animator.SetBool("isGrounded", true);

        // ���� (������ ���콺 ��ư)
        animator.SetBool("isAiming", Input.GetMouseButton(1));

        // ���� (���콺 ���� Ŭ��)
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("isAttacking");
            if (gun != null)
                gun.Shoot(); // �� �� �� �߰�!
            StartCoroutine(DoAction(animator, "Attack"));
        }
        
        // �ൿ �߿��� �ٸ� �Է� ����
        if (isBusy) return;

        // ������ (�����̽���)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("isRolling");
            StartCoroutine(DoAction(animator, "Roll"));
        }

        // ��ȣ�ۿ� (EŰ)
        animator.SetBool("isUsing", Input.GetKey(KeyCode.E));

        // ������ (RŰ)
        if (Input.GetKeyDown(KeyCode.R))
        {
            weapon.Reload();
        }
        // ������Ʈ �ݱ� (FŰ)
        if (Input.GetKeyDown(KeyCode.F))
        {
            animator.SetTrigger("isPickingUp");
            StartCoroutine(DoAction(animator, "PickupObject"));
        }

        

        // ��� (�׽�Ʈ��: KŰ)
        if (Input.GetKeyDown(KeyCode.K))
        {
            animator.SetTrigger("isDead");
        }
    }
    
    // �ൿ �ִϸ��̼��� ���� ������ ��� �� ���� ����
    private System.Collections.IEnumerator DoAction(Animator anim, string stateName)
    {
        isBusy = true;
        // ���� ���°� stateName�� �� ������ ���
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            yield return null;
        // �ִϸ��̼��� ���� ������ ���
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            yield return null;
        isBusy = false;
    }
}

