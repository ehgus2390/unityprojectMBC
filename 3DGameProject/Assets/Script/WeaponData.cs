using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Basic Settings")]
    public string weaponName = "Default Weapon";
    public GameObject weaponModel;
    public Transform firePoint;

    [Header("Combat Settings")]
    public float fireRate = 0.1f;
    public float bulletSpeed = 50f;
    public float damage = 10f;
    public int maxAmmo = 30;
    public float reloadTime = 2f;

    [Header("Audio Settings")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;

    [Header("Visual Effects")]
    public ParticleSystem muzzleFlash;
    public GameObject bulletPrefab;

    [Header("Recoil Settings")]
    public float recoilForce = 2f;
    public float recoilRecoverySpeed = 5f;
    public Vector3 recoilRotation = new Vector3(-5f, 0f, 0f);

    [Header("Aiming Settings")]
    public float aimFOV = 40f;
    public float aimSpeed = 5f;

    [Header("Weapon Type")]
    public WeaponType weaponType = WeaponType.Rifle;

    public enum WeaponType
    {
        Pistol,
        Rifle,
        Shotgun,
        Sniper,
        SMG,
        LMG
    }

    // 무기 타입별 기본 설정
    public void SetDefaultValuesByType(WeaponType type)
    {
        weaponType = type;

        switch (type)
        {
            case WeaponType.Pistol:
                fireRate = 0.3f;
                damage = 15f;
                maxAmmo = 12;
                reloadTime = 1.5f;
                recoilForce = 1f;
                break;

            case WeaponType.Rifle:
                fireRate = 0.1f;
                damage = 25f;
                maxAmmo = 30;
                reloadTime = 2f;
                recoilForce = 2f;
                break;

            case WeaponType.Shotgun:
                fireRate = 0.8f;
                damage = 80f;
                maxAmmo = 8;
                reloadTime = 3f;
                recoilForce = 4f;
                break;

            case WeaponType.Sniper:
                fireRate = 1.5f;
                damage = 100f;
                maxAmmo = 5;
                reloadTime = 2.5f;
                recoilForce = 6f;
                break;

            case WeaponType.SMG:
                fireRate = 0.05f;
                damage = 15f;
                maxAmmo = 25;
                reloadTime = 1.8f;
                recoilForce = 1.5f;
                break;

            case WeaponType.LMG:
                fireRate = 0.08f;
                damage = 30f;
                maxAmmo = 100;
                reloadTime = 4f;
                recoilForce = 3f;
                break;
        }
    }
}
