using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    public string weaponName;
    public int maxAmmo = 30;
    public int currentAmmo;
    public float reloadTime = 2f;

    public Animator animator; // ���� �Ǵ� �÷��̾��� Animator

    private bool isReloading = false;

    private void Start()
    {
        currentAmmo = maxAmmo;
    }

    public void Reload()
    {
        if (isReloading || currentAmmo == maxAmmo) return;
        isReloading = true;
        if (animator != null)
            animator.SetTrigger("isReloading");
        // ������ �ִϸ��̼��� ���ٸ� �ٷ� �ڷ�ƾ ����
        // StartCoroutine(ReloadRoutine());
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ���ϰų�, �ڷ�ƾ���� ���� ȣ�� ����
    public void FinishReload()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log($"{weaponName} ������ �Ϸ�!");
    }
}