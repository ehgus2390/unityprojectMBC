using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator weaponAnimator;
    [SerializeField] private float shootCooldown = 0.1f;

    [Header("Weapon Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Accuracy Settings")]
    [SerializeField] private float standingAccuracy = 0.1f;
    [SerializeField] private float crouchingAccuracy = 0.05f;
    [SerializeField] private float proneAccuracy = 0.02f;
    [SerializeField] private float runningAccuracy = 0.3f;

    private bool isCrouching = false;
    private bool isProne = false;
    private bool isRunning = false;
    private float nextShootTime;

    private void Update()
    {
        // 자세 상태 체크
        isCrouching = Input.GetKey(KeyCode.C);
        isProne = Input.GetKey(KeyCode.Z);
        isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching && !isProne;

        // 애니메이터 파라미터 업데이트
        weaponAnimator.SetBool("IsCrouching", isCrouching);
        weaponAnimator.SetBool("IsProne", isProne);
        weaponAnimator.SetBool("IsRunning", isRunning);

        // 발사 처리
        if (Input.GetMouseButton(0) && Time.time >= nextShootTime)
        {
            Shoot();
            nextShootTime = Time.time + shootCooldown;
        }
    }
    
    private float GetCurrentAccuracy()
    {
        if (isProne) return proneAccuracy;
        if (isCrouching) return crouchingAccuracy;
        if (isRunning) return runningAccuracy;
        return standingAccuracy;
    }

    private void Shoot()
    {
        float accuracy = GetCurrentAccuracy();
        Vector3 spread = new Vector3(
            Random.Range(-accuracy, accuracy),
            Random.Range(-accuracy, accuracy),
            0
        );

        // 발사 방향에 정확도 적용
        Vector3 shootDirection = firePoint.forward + spread;

        // 총알 생성 및 발사
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.transform.forward = shootDirection;
    }
}
