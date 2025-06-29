using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    // ���� ������Ʈ �迭
    public GameObject[] weapons; 
    private int currentWeapon = 0;

    void Start()
    {
        SelectWeapon(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectWeapon(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectWeapon(4);
        
    }

    void SelectWeapon(int index)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(i == index);
        }
        currentWeapon = index;
    }
}