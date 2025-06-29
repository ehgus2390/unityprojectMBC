using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Transform weaponHolder; // Weapon_R 오브젝트를 여기에 할당
    [SerializeField] private float switchTime = 0.5f;

    private List<Weapon> weapons = new List<Weapon>();
    private int currentWeaponIndex = 0;
    private bool isSwitching = false;
    private float switchTimer = 0f;

    private void Start()
    {
        InitializeWeapons();
    }

    private void InitializeWeapons()
    {
        // Weapon_R의 모든 자식 오브젝트(무기들)를 가져옵니다
        for (int i = 0; i < weaponHolder.childCount; i++)
        {
            Transform weaponTransform = weaponHolder.GetChild(i);
            Weapon weapon = weaponTransform.GetComponent<Weapon>();

            if (weapon != null)
            {
                weapons.Add(weapon);
                // 첫 번째 무기만 활성화하고 나머지는 비활성화
                weaponTransform.gameObject.SetActive(i == 0);
            }
        }
    }

    private void Update()
    {
        if (isSwitching)
        {
            switchTimer += Time.deltaTime;
            if (switchTimer >= switchTime)
            {
                isSwitching = false;
                switchTimer = 0f;
            }
            return;
        }

        // 숫자 키로 무기 전환 (1-5)
        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                SwitchWeapon(i);
                break;
            }
        }

        // 마우스 휠로 무기 전환
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            int direction = scroll > 0 ? -1 : 1;
            int newIndex = (currentWeaponIndex + direction + weapons.Count) % weapons.Count;
            SwitchWeapon(newIndex);
        }

        
        // 재장전
        if (Input.GetKeyDown(KeyCode.R))
        {
            weapons[currentWeaponIndex].Reload();
        }
    }

    private void SwitchWeapon(int newIndex)
    {
        if (newIndex >= weapons.Count || newIndex == currentWeaponIndex || isSwitching)
            return;

        isSwitching = true;
        switchTimer = 0f;

        // 현재 무기 비활성화
        weapons[currentWeaponIndex].gameObject.SetActive(false);

        // 새 무기 활성화
        currentWeaponIndex = newIndex;
        weapons[currentWeaponIndex].gameObject.SetActive(true);
    }

    public Weapon GetCurrentWeapon()
    {
        return weapons[currentWeaponIndex];
    }
}