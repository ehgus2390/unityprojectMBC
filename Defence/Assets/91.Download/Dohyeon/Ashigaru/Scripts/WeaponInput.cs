using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;

public class WeaponInput : MonoBehaviour
{
    [Header("Weapon Controller")]
    [SerializeField] private WeaponController weaponController;

    [Header("Input Keys")]
    [SerializeField] private KeyCode katanaKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode crossSpearKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode unequipKey = KeyCode.Alpha0;

    private void Update()
    {
        if (weaponController == null) return;

        // 키보드 입력 처리
        if (Input.GetKeyDown(katanaKey))
        {
            weaponController.EquipKatana();
            Debug.Log("키보드 입력: 카타나 장착");
        }

        if (Input.GetKeyDown(crossSpearKey))
        {
            weaponController.EquipCrossSpear();
            Debug.Log("키보드 입력: 크로스 스피어 장착");
        }

        if (Input.GetKeyDown(unequipKey))
        {
            weaponController.UnequipWeapon();
            Debug.Log("키보드 입력: 무기 제거");
        }
    }
}
