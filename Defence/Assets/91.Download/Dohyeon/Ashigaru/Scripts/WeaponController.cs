using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform weaponSocket; // 무기를 장착할 위치
    [SerializeField] private GameObject currentWeapon; // 현재 장착된 무기

    [Header("Available Weapons")]
    [SerializeField] private GameObject katanaPrefab;
    [SerializeField] private GameObject crossSpearPrefab;

    private void Start()
    {
        // 기본 무기로 카타나 장착
        if (katanaPrefab != null)
        {
            EquipWeapon(katanaPrefab);
        }
    }

    /// <summary>
    /// 무기를 장착합니다
    /// </summary>
    /// <param name="weaponPrefab">장착할 무기 프리팹</param>
    public void EquipWeapon(GameObject weaponPrefab)
    {
        // 기존 무기 제거
        if (currentWeapon != null)
        {
            DestroyImmediate(currentWeapon);
        }

        // 새 무기 생성 및 장착
        if (weaponPrefab != null && weaponSocket != null)
        {
            currentWeapon = Instantiate(weaponPrefab, weaponSocket);
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;
        }
    }

    /// <summary>
    /// 카타나 장착
    /// </summary>
    public void EquipKatana()
    {
        EquipWeapon(katanaPrefab);
    }

    /// <summary>
    /// 크로스 스피어 장착
    /// </summary>
    public void EquipCrossSpear()
    {
        EquipWeapon(crossSpearPrefab);
    }

    /// <summary>
    /// 무기 제거
    /// </summary>
    public void UnequipWeapon()
    {
        if (currentWeapon != null)
        {
            DestroyImmediate(currentWeapon);
            currentWeapon = null;
        }
    }

    /// <summary>
    /// 현재 장착된 무기 반환
    /// </summary>
    public GameObject GetCurrentWeapon()
    {
        return currentWeapon;
    }
}
