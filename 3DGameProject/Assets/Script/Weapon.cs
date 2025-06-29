using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    public string weaponName;
    public int maxAmmo = 30;
    public int currentAmmo;
    public float reloadTime = 2f;

    public Animator animator; // 무기 또는 플레이어의 Animator

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
        // 재장전 애니메이션이 없다면 바로 코루틴 실행
        // StartCoroutine(ReloadRoutine());
    }

    // 애니메이션 이벤트에서 호출하거나, 코루틴으로 직접 호출 가능
    public void FinishReload()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log($"{weaponName} 재장전 완료!");
    }
}