using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WeaponInventory : MonoBehaviour
{
    [System.Serializable]
    public class WeaponSlot
    {
        public WeaponController.Weapon weapon;
        public bool isUnlocked = false;
        public int slotIndex;
        public Sprite weaponIcon;
    }

    [Header("Inventory Settings")]
    [SerializeField] private WeaponSlot[] weaponSlots = new WeaponSlot[6]; // �ִ� 6�� ���� ����
    [SerializeField] private int maxWeapons = 6;
    [SerializeField] private bool allowDuplicateWeapons = false;

    [Header("Weapon Unlock System")]
    [SerializeField] private bool useUnlockSystem = true;
    [SerializeField] private int startingWeaponIndex = 0;

    [Header("Ammo Management")]
    [SerializeField] private bool sharedAmmo = false;
    [SerializeField] private int maxAmmoCapacity = 999;
    [SerializeField] private Dictionary<string, int> ammoTypes = new Dictionary<string, int>();

    private WeaponController weaponController;
    private int currentSlotIndex = 0;

    public delegate void WeaponChangedHandler(WeaponController.Weapon newWeapon, int slotIndex);
    public event WeaponChangedHandler OnWeaponChanged;

    public delegate void WeaponUnlockedHandler(WeaponController.Weapon weapon, int slotIndex);
    public event WeaponUnlockedHandler OnWeaponUnlocked;

    private void Start()
    {
        weaponController = GetComponent<WeaponController>();
        if (weaponController == null)
        {
            weaponController = FindObjectOfType<WeaponController>();
        }

        InitializeInventory();
        SetupAmmoTypes();
    }

    private void InitializeInventory()
    {
        // ���� ���� �ر�
        if (useUnlockSystem && startingWeaponIndex < weaponSlots.Length)
        {
            weaponSlots[startingWeaponIndex].isUnlocked = true;
        }

        // ��� ���� ���� �ʱ�ȭ
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == null)
            {
                weaponSlots[i] = new WeaponSlot();
            }
            weaponSlots[i].slotIndex = i;
        }

        // ù ��° �رݵ� ����� ����
        SetCurrentWeapon(GetFirstUnlockedWeaponIndex());
    }

    private void SetupAmmoTypes()
    {
        if (!sharedAmmo) return;

        // ���� Ÿ�Ժ� ź�� ����
        ammoTypes.Clear();
        ammoTypes.Add("Pistol", maxAmmoCapacity);
        ammoTypes.Add("Rifle", maxAmmoCapacity);
        ammoTypes.Add("Shotgun", maxAmmoCapacity);
        ammoTypes.Add("Sniper", maxAmmoCapacity);
        ammoTypes.Add("SMG", maxAmmoCapacity);
        ammoTypes.Add("LMG", maxAmmoCapacity);
    }

    public bool AddWeapon(WeaponController.Weapon weapon, int slotIndex = -1)
    {
        if (weapon == null) return false;

        // ���� �ε����� �������� ���� ��� �� ���� ã��
        if (slotIndex == -1)
        {
            slotIndex = FindEmptySlot();
            if (slotIndex == -1) return false; // �� ������ ����
        }

        // ���� ���� üũ
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) return false;

        // �ߺ� ���� üũ
        if (!allowDuplicateWeapons && HasWeapon(weapon.name))
        {
            Debug.LogWarning($"Weapon {weapon.name} already exists in inventory!");
            return false;
        }

        // ���� �߰�
        weaponSlots[slotIndex].weapon = weapon;
        weaponSlots[slotIndex].isUnlocked = true;

        // WeaponController�� ���� �߰�
        if (weaponController != null)
        {
            weaponController.AddWeapon(weapon);
        }

        // �̺�Ʈ �߻�
        OnWeaponUnlocked?.Invoke(weapon, slotIndex);

        Debug.Log($"Weapon {weapon.name} added to slot {slotIndex}");
        return true;
    }

    public bool RemoveWeapon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) return false;

        var weapon = weaponSlots[slotIndex].weapon;
        if (weapon == null) return false;

        weaponSlots[slotIndex].weapon = null;
        weaponSlots[slotIndex].isUnlocked = false;

        Debug.Log($"Weapon {weapon.name} removed from slot {slotIndex}");
        return true;
    }

    public void SwitchToWeapon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) return;
        if (!weaponSlots[slotIndex].isUnlocked) return;
        if (weaponSlots[slotIndex].weapon == null) return;

        currentSlotIndex = slotIndex;

        // WeaponController���� �ش� ����� ��ȯ
        if (weaponController != null)
        {
            // WeaponController�� ���� �迭���� �ش� ���� ã��
            var weapons = GetWeaponsArray();
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i].name == weaponSlots[slotIndex].weapon.name)
                {
                    // ���� ��ȯ ���� (WeaponController���� ���� �ʿ�)
                    break;
                }
            }
        }

        OnWeaponChanged?.Invoke(weaponSlots[slotIndex].weapon, slotIndex);
    }

    public void SetCurrentWeapon(int slotIndex)
    {
        SwitchToWeapon(slotIndex);
    }

    public WeaponController.Weapon GetCurrentWeapon()
    {
        if (currentSlotIndex < 0 || currentSlotIndex >= weaponSlots.Length) return null;
        return weaponSlots[currentSlotIndex].weapon;
    }

    public WeaponController.Weapon GetWeaponAtSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) return null;
        return weaponSlots[slotIndex].weapon;
    }

    public bool IsSlotUnlocked(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) return false;
        return weaponSlots[slotIndex].isUnlocked;
    }

    public void UnlockSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) return;
        weaponSlots[slotIndex].isUnlocked = true;
    }

    public void LockSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) return;
        weaponSlots[slotIndex].isUnlocked = false;
    }

    public bool HasWeapon(string weaponName)
    {
        return weaponSlots.Any(slot => slot.weapon != null && slot.weapon.name == weaponName);
    }

    public int GetWeaponCount()
    {
        return weaponSlots.Count(slot => slot.weapon != null);
    }

    public int GetUnlockedSlotCount()
    {
        return weaponSlots.Count(slot => slot.isUnlocked);
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i].weapon == null) return i;
        }
        return -1;
    }

    private int GetFirstUnlockedWeaponIndex()
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i].isUnlocked && weaponSlots[i].weapon != null)
            {
                return i;
            }
        }
        return 0;
    }

    // ź�� ����
    public void AddAmmo(string ammoType, int amount)
    {
        if (!sharedAmmo) return;

        if (ammoTypes.ContainsKey(ammoType))
        {
            ammoTypes[ammoType] = Mathf.Min(ammoTypes[ammoType] + amount, maxAmmoCapacity);
        }
    }

    public int GetAmmo(string ammoType)
    {
        if (!sharedAmmo) return 0;

        return ammoTypes.ContainsKey(ammoType) ? ammoTypes[ammoType] : 0;
    }

    public bool ConsumeAmmo(string ammoType, int amount)
    {
        if (!sharedAmmo) return true; // ���� ź�� �ý��ۿ����� �׻� true

        if (ammoTypes.ContainsKey(ammoType) && ammoTypes[ammoType] >= amount)
        {
            ammoTypes[ammoType] -= amount;
            return true;
        }
        return false;
    }

    // ���� �迭 ��ȯ (WeaponController�� ������)
    public WeaponController.Weapon[] GetWeaponsArray()
    {
        return weaponSlots.Where(slot => slot.weapon != null).Select(slot => slot.weapon).ToArray();
    }

    // �κ��丮 ���� ���
    public void PrintInventoryInfo()
    {
        Debug.Log("=== Weapon Inventory ===");
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            var slot = weaponSlots[i];
            string status = slot.isUnlocked ? "Unlocked" : "Locked";
            string weaponName = slot.weapon != null ? slot.weapon.name : "Empty";
            Debug.Log($"Slot {i}: {weaponName} ({status})");
        }

        if (sharedAmmo)
        {
            Debug.Log("=== Ammo Status ===");
            foreach (var ammo in ammoTypes)
            {
                Debug.Log($"{ammo.Key}: {ammo.Value}");
            }
        }
    }

    // ����/�ε� �ý���
    public void SaveInventory()
    {
        // PlayerPrefs�� ����� ������ ���� �ý���
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            var slot = weaponSlots[i];
            PlayerPrefs.SetInt($"WeaponSlot_{i}_Unlocked", slot.isUnlocked ? 1 : 0);

            if (slot.weapon != null)
            {
                PlayerPrefs.SetString($"WeaponSlot_{i}_Name", slot.weapon.name);
                PlayerPrefs.SetInt($"WeaponSlot_{i}_Ammo", slot.weapon.currentAmmo);
            }
        }

        PlayerPrefs.SetInt("CurrentWeaponSlot", currentSlotIndex);
        PlayerPrefs.Save();
    }

    public void LoadInventory()
    {
        // ����� ������ �ε�
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            weaponSlots[i].isUnlocked = PlayerPrefs.GetInt($"WeaponSlot_{i}_Unlocked", 0) == 1;

            string weaponName = PlayerPrefs.GetString($"WeaponSlot_{i}_Name", "");
            if (!string.IsNullOrEmpty(weaponName))
            {
                // ���� ������ ���� (���� ���������� WeaponData ScriptableObject ���)
                // ���⼭�� ������ ����
            }
        }

        currentSlotIndex = PlayerPrefs.GetInt("CurrentWeaponSlot", 0);
        SetCurrentWeapon(currentSlotIndex);
    }
}
