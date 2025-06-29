using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Transform weaponHolder; // Weapon_R ������Ʈ�� ���⿡ �Ҵ�
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
        // Weapon_R�� ��� �ڽ� ������Ʈ(�����)�� �����ɴϴ�
        for (int i = 0; i < weaponHolder.childCount; i++)
        {
            Transform weaponTransform = weaponHolder.GetChild(i);
            Weapon weapon = weaponTransform.GetComponent<Weapon>();

            if (weapon != null)
            {
                weapons.Add(weapon);
                // ù ��° ���⸸ Ȱ��ȭ�ϰ� �������� ��Ȱ��ȭ
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

        // ���� Ű�� ���� ��ȯ (1-5)
        for (int i = 0; i < 5; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                SwitchWeapon(i);
                break;
            }
        }

        // ���콺 �ٷ� ���� ��ȯ
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            int direction = scroll > 0 ? -1 : 1;
            int newIndex = (currentWeaponIndex + direction + weapons.Count) % weapons.Count;
            SwitchWeapon(newIndex);
        }

        
        // ������
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

        // ���� ���� ��Ȱ��ȭ
        weapons[currentWeaponIndex].gameObject.SetActive(false);

        // �� ���� Ȱ��ȭ
        currentWeaponIndex = newIndex;
        weapons[currentWeaponIndex].gameObject.SetActive(true);
    }

    public Weapon GetCurrentWeapon()
    {
        return weapons[currentWeaponIndex];
    }
}