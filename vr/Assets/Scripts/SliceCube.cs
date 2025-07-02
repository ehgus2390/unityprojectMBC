using System.Collections;
using System.Collections.Generic;
using EzySlice;
using UnityEngine;

public class SliceCube : MonoBehaviour
{
    // 자르는 기준이 되는 오브젝트 Quad (주석 처리: 이 변수는 CubeController에서 가져오므로 직접 사용하지 않습니다.)
    // private Transform Slice_Obj; 

    // 잘리는 오브젝트 (StartCoroutine에서 target을 직접 넘기기 위해 public으로 유지)
    public GameObject Target;

    // 잘리는 단면에 대한 머테리얼 
    public Material Cross_M;

    // 잘릴 때 우아하게 보이기 위해 사용되는 force (추가: 변수 선언 누락)
    public float cutForce = 500f; // 적절한 값으로 조절하세요

    // 막대봉 관련된 변수
    private Vector3 previous_pos; // 이전 위치

    // 레이케스트가 충돌할 대상 레이어 설정
    public LayerMask layer;

    // Start is called before the first frame update
    void Start()
    {
        // 초기 previous_pos 설정 (없으면 첫 프레임에서 이상한 값이 들어갈 수 있음)
        previous_pos = transform.position;
        StartCoroutine(Update_co());
    }

    IEnumerator Update_co()
    {
        while (true)
        {
            // 레이캐스트는 자신(SliceCube를 가진 오브젝트)의 위치에서 앞 방향으로 발사합니다.
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 100f, layer)) // maxDistance 추가
            {
                // 이전 위치와 현재 위치를 비교하여 각도를 계산하는 방식이 논리적으로 맞지 않습니다.
                // 이전 위치와 현재 hit.transform.up의 각도를 비교하는 것이 아니라,
                // 이전 프레임의 자신의 위치와 현재 프레임의 자신의 위치 변화 방향을 기반으로 해야 합니다.
                // 여기서는 칼날의 움직임 방향과 충돌한 오브젝트의 수직 방향을 고려하여 자르는 조건으로 변경했습니다.
                // 또는 단순히 충돌이 발생했을 때 자르는 방식으로도 구현 가능합니다.

                // 변경된 자르기 조건: 칼날의 이전 위치와 현재 위치 벡터, 그리고 맞은 오브젝트의 업 벡터를 사용
                // 여기서는 칼날의 움직임 방향과 맞은 오브젝트의 Normal 벡터를 사용하여 자르는 조건을 검토하는 방식으로 변경했습니다.
                // hit.transform.up 대신 hit.normal을 사용하는 것이 더 정확할 수 있습니다.
                Vector3 moveDirection = (transform.position - previous_pos).normalized;

                // 특정 각도 이상으로 빠르게 휘둘렀을 때 자르도록 조건을 변경 (예: 둔각으로 휘두를 때)
                if (Vector3.Angle(moveDirection, hit.normal) > 90f && moveDirection.magnitude > 0.01f) // 움직임이 있을 때만
                {
                    // Target이 직접 설정되어 있다면 그 Target을 자르고, 아니라면 Raycast hit된 오브젝트를 자릅니다.
                    // 이 스크립트에서는 Raycast로 맞은 hit.collider.gameObject를 자르는 것이 더 자연스럽습니다.
                    Slice_Object(hit.collider.gameObject);
                }
            }
            previous_pos = transform.position; // Raycast hit.transform.position이 아닌 자신의 현재 위치를 저장해야 합니다.
            yield return null;
        }
    }

    // Slice_Object 함수에 매개변수를 추가하여 어떤 오브젝트를 자를지 명확히 합니다.
    private void Slice_Object(GameObject target)
    {
        // CubeController 컴포넌트가 있는지 확인하고 없으면 자르지 않음
        if (target.TryGetComponent(out CubeController c))
        {
            // Slice_Obj는 CubeController에서 가져옵니다.
            Transform Slice_Obj_From_Controller = c.SliceObj;

            // 슬라이스 노멀 벡터 계산: 칼날의 움직임 방향(이전 위치에서 현재 위치)과 전방 벡터의 외적
            // Vector3.Cross(transform.position - previous_pos, transform.forward)는
            // 칼날의 움직임 방향과 칼날의 전방을 기준으로 슬라이스 평면의 수직 벡터를 생성합니다.
            // 슬라이스 기준은 Slice_Obj_From_Controller.position과 slice_normal이 되어야 합니다.
            Vector3 slice_normal = Vector3.Cross(transform.position - previous_pos, transform.forward).normalized;
            if (slice_normal == Vector3.zero) // 움직임이 없어서 normal이 0이 되는 경우 기본값 설정
            {
                slice_normal = transform.forward; // 또는 다른 적절한 기본 방향
            }

            // EzySlice를 사용해 대상 오브젝트 자르기
            // Slice_Obj_From_Controller.position과 계산된 slice_normal 사용
            SlicedHull hull = target.Slice(Slice_Obj_From_Controller.position, slice_normal, Cross_M); // Cross_M을 바로 넘겨주세요.

            // 자르기가 성공했으면
            if (hull != null)
            {
                // UpperHull과 LowerHull 생성 시 재료 전달 방식이 EzySlice 버전과 사용법에 따라 다를 수 있습니다.
                // 일반적으로 CreateUpperHull, CreateLowerHull은 매개변수로 Material을 받습니다.
                // 여기서는 Material을 바로 넘겨주도록 수정했습니다.
                GameObject UpperHull = hull.CreateUpperHull(target, Cross_M);
                GameObject LowerHull = hull.CreateLowerHull(target, Cross_M); // CreateUpperHull이 아닌 CreateLowerHull을 사용

                // 생성된 파편들에 물리 효과 추가
                Setup_Slice_component(UpperHull);
                Setup_Slice_component(LowerHull);

                // 자식 오브젝트들도 함께 자르기
                // 주의: target.transform.childCount 대신 g.transform.childCount로 수정해야 합니다.
                // 그리고 CreateUpperHull 호출 시 UpperHull을 부모로 넘기는 인자가 맞는지 확인해야 합니다.
                // EzySlice의 사용법에 따라 CreateUpperHull/CreateLowerHull은 일반적으로 대상 오브젝트와 재료만 받습니다.
                // 자식 오브젝트들도 각각의 조각으로 만들어 새로운 UpperHull과 LowerHull의 자식으로 붙이려면
                // 추가적인 로직이 필요하며, EzySlice의 기본 동작과 약간 다를 수 있습니다.
                // 여기서는 자식 오브젝트를 개별적으로 잘라 새 조각을 생성하도록 수정했습니다.

                // 자식 오브젝트의 반복문에서 target.transform.childCount를 사용해야 합니다.
                // transform.childCount는 이 스크립트가 붙은 오브젝트의 자식을 의미합니다.
                // 또한, 자식 오브젝트를 자를 때 새로운 조각이 생성되므로, 생성된 조각들을 기존 UpperHull/LowerHull의 자식으로 붙여주려면
                // SetParent를 사용해야 합니다. 이 부분은 EzySlice의 고급 사용법에 해당하므로 간단히 각 조각을 생성만 하도록 두겠습니다.
                if (target.transform.childCount > 0)
                {
                    // 자식 오브젝트들을 리스트로 복사하여 반복 도중 컬렉션이 변경되는 것을 방지합니다.
                    List<GameObject> childrenToSlice = new List<GameObject>();
                    for (int i = 0; i < target.transform.childCount; i++)
                    {
                        GameObject child = target.transform.GetChild(i).gameObject;
                        if (child != Slice_Obj_From_Controller.gameObject) // Slice_Obj 자체는 자르지 않음
                        {
                            childrenToSlice.Add(child);
                        }
                    }

                    foreach (GameObject g_child in childrenToSlice)
                    {
                        // 자식 오브젝트의 위치와 같은 슬라이스 평면 사용
                        SlicedHull hull_c = g_child.Slice(Slice_Obj_From_Controller.position, slice_normal, Cross_M);

                        if (hull_c != null)
                        {
                            GameObject upper_c = hull_c.CreateUpperHull(g_child, Cross_M);
                            GameObject lower_c = hull_c.CreateLowerHull(g_child, Cross_M);

                            Setup_Slice_component(upper_c);
                            Setup_Slice_component(lower_c);

                            // 원본 자식 오브젝트는 파괴
                            Destroy(g_child);
                        }
                    }
                }

                // 원본 대상 오브젝트는 파괴
                Destroy(target);

                // 잘린 조각들은 일정 시간 후 파괴 (예: 1.0f 초 후)
                Destroy(UpperHull, 1.0f);
                Destroy(LowerHull, 1.0f);
            }
        }
    }

    // 물리 컴포넌트 및 힘 추가 함수
    private void Setup_Slice_component(GameObject g)
    {
        if (g == null) return; // null 체크 추가

        Rigidbody rb = g.GetComponent<Rigidbody>();
        if (rb == null) // Rigidbody가 없으면 추가
        {
            rb = g.AddComponent<Rigidbody>();
        }

        MeshCollider c = g.GetComponent<MeshCollider>();
        if (c == null) // MeshCollider가 없으면 추가
        {
            c = g.AddComponent<MeshCollider>();
        }

        c.convex = true; // MeshCollider는 convex여야 Rigidbody와 제대로 작동합니다.

        // 폭발력 추가 (cutForce 사용)
        // AddExplosionForce는 특정 지점에서부터의 폭발력을 시뮬레이션합니다.
        // 여기서는 잘린 조각의 위치에서 폭발력이 가해지도록 설정합니다.
        rb.AddExplosionForce(cutForce, g.transform.position, 1f); // 반경 추가
    }

    // Update는 코루틴으로 처리하므로 비워둡니다.
    void Update()
    {

    }
}