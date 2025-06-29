using UnityEngine;

public class WeaponAim : MonoBehaviour
{
    public Camera mainCamera; // 플레이어 카메라

    void Update()
    {
        // 화면 중앙의 Ray 생성
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 lookPoint = ray.GetPoint(10f); // 10 유닛 앞 지점

        // 무기(총구)가 lookPoint를 바라보게 회전
        transform.LookAt(lookPoint);
    }
}
