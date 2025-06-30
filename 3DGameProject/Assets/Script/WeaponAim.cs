using UnityEngine;

public class WeaponAim : MonoBehaviour
{
    public Camera mainCamera; // �÷��̾� ī�޶�

    void Update()
    {
        // ȭ�� �߾��� Ray ����
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 lookPoint = ray.GetPoint(10f); // 10 ���� �� ����

        // ����(�ѱ�)�� lookPoint�� �ٶ󺸰� ȸ��
        transform.LookAt(lookPoint);
    }
}
