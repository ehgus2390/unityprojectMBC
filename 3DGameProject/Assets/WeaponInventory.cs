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
    [SerializeField] private WeaponSlot[] weaponSlots = new WeaponSlot[6]; // 최대 6개 무기 슬롯
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
        // 시작 무기 해금
        if (useUnlockSystem && startingWeaponIndex < weaponSlots.Length)
        {
            weaponSlots[startingWeaponIndex].isUnlocked = true;
        }

        // 모든 무기 슬롯 초기화
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == null)
            {
                weaponSlots[i] = new WeaponSlot();
            }
            weaponSlots[i].slotIndex = i;
        }

        // 첫 번째 해금된 무기로 설정
        SetCurrentWeapon(GetFirstUnlockedWeaponIndex());
    }

    private void SetupAmmoTypes()
    {
        if (!sharedAmmo) return;

        // 무기 타입별 탄약 설정
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

        // 슬롯 인덱스가 지정되지 않은 경우 빈 슬롯 찾기
        if (slotIndex == -1)
        {
            slotIndex = FindEmptySlot();
            if (slotIndex == -1) return false; // 빈 슬롯이 없음
        }

        // 슬롯 범위 체크
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) return false;

        // 중복 무기 체크
        if (!allowDuplicateWeapons && HasWeapon(weapon.name))
        {
            Debug.LogWarning($"Weapon {weapon.name} already exists in inventory!");
            return false;
        }

        // 무기 추가
        weaponSlots[slotIndex].weapon = weapon;
        weaponSlots[slotIndex].isUnlocked = true;

        // WeaponController에 무기 추가
        if (weaponController != null)
        {
            weaponController.AddWeapon(weapon);
        }

        // 이벤트 발생
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

        // WeaponController에서 해당 무기로 전환
        if (weaponController != null)
        {
            // WeaponController의 무기 배열에서 해당 무기 찾기
            var weapons = GetWeaponsArray();
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i].name == weaponSlots[slotIndex].weapon.name)
                {
                    // 무기 전환 로직 (WeaponController에서 구현 필요)
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

    // 탄약 관리
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
        if (!sharedAmmo) return true; // 개별 탄약 시스템에서는 항상 true

        if (ammoTypes.ContainsKey(ammoType) && ammoTypes[ammoType] >= amount)
        {
            ammoTypes[ammoType] -= amount;
            return true;
        }
        return false;
    }

    // 무기 배열 반환 (WeaponController와 연동용)
    public WeaponController.Weapon[] GetWeaponsArray()
    {
        return weaponSlots.Where(slot => slot.weapon != null).Select(slot => slot.weapon).ToArray();
    }

    // 인벤토리 정보 출력
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

    // 저장/로드 시스템
    public void SaveInventory()
    {
        // PlayerPrefs를 사용한 간단한 저장 시스템
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
        // 저장된 데이터 로드
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            weaponSlots[i].isUnlocked = PlayerPrefs.GetInt($"WeaponSlot_{i}_Unlocked", 0) == 1;

            string weaponName = PlayerPrefs.GetString($"WeaponSlot_{i}_Name", "");
            if (!string.IsNullOrEmpty(weaponName))
            {
                // 무기 데이터 복원 (실제 구현에서는 WeaponData ScriptableObject 사용)
                // 여기서는 간단한 예시
            }
        }

        currentSlotIndex = PlayerPrefs.GetInt("CurrentWeaponSlot", 0);
        SetCurrentWeapon(currentSlotIndex);
    }
}
