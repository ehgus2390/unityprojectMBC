using System.Collections;
using System.Collections.Generic;
using EzySlice;
using UnityEngine;

public class SliceCube : MonoBehaviour
{
    // �ڸ��� ������ �Ǵ� ������Ʈ Quad (�ּ� ó��: �� ������ CubeController���� �������Ƿ� ���� ������� �ʽ��ϴ�.)
    // private Transform Slice_Obj; 

    // �߸��� ������Ʈ (StartCoroutine���� target�� ���� �ѱ�� ���� public���� ����)
    public GameObject Target;

    // �߸��� �ܸ鿡 ���� ���׸��� 
    public Material Cross_M;

    // �߸� �� ����ϰ� ���̱� ���� ���Ǵ� force (�߰�: ���� ���� ����)
    public float cutForce = 500f; // ������ ������ �����ϼ���

    // ����� ���õ� ����
    private Vector3 previous_pos; // ���� ��ġ

    // �����ɽ�Ʈ�� �浹�� ��� ���̾� ����
    public LayerMask layer;

    // Start is called before the first frame update
    void Start()
    {
        // �ʱ� previous_pos ���� (������ ù �����ӿ��� �̻��� ���� �� �� ����)
        previous_pos = transform.position;
        StartCoroutine(Update_co());
    }

    IEnumerator Update_co()
    {
        while (true)
        {
            // ����ĳ��Ʈ�� �ڽ�(SliceCube�� ���� ������Ʈ)�� ��ġ���� �� �������� �߻��մϴ�.
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 100f, layer)) // maxDistance �߰�
            {
                // ���� ��ġ�� ���� ��ġ�� ���Ͽ� ������ ����ϴ� ����� �������� ���� �ʽ��ϴ�.
                // ���� ��ġ�� ���� hit.transform.up�� ������ ���ϴ� ���� �ƴ϶�,
                // ���� �������� �ڽ��� ��ġ�� ���� �������� �ڽ��� ��ġ ��ȭ ������ ������� �ؾ� �մϴ�.
                // ���⼭�� Į���� ������ ����� �浹�� ������Ʈ�� ���� ������ ����Ͽ� �ڸ��� �������� �����߽��ϴ�.
                // �Ǵ� �ܼ��� �浹�� �߻����� �� �ڸ��� ������ε� ���� �����մϴ�.

                // ����� �ڸ��� ����: Į���� ���� ��ġ�� ���� ��ġ ����, �׸��� ���� ������Ʈ�� �� ���͸� ���
                // ���⼭�� Į���� ������ ����� ���� ������Ʈ�� Normal ���͸� ����Ͽ� �ڸ��� ������ �����ϴ� ������� �����߽��ϴ�.
                // hit.transform.up ��� hit.normal�� ����ϴ� ���� �� ��Ȯ�� �� �ֽ��ϴ�.
                Vector3 moveDirection = (transform.position - previous_pos).normalized;

                // Ư�� ���� �̻����� ������ �ֵѷ��� �� �ڸ����� ������ ���� (��: �а����� �ֵθ� ��)
                if (Vector3.Angle(moveDirection, hit.normal) > 90f && moveDirection.magnitude > 0.01f) // �������� ���� ����
                {
                    // Target�� ���� �����Ǿ� �ִٸ� �� Target�� �ڸ���, �ƴ϶�� Raycast hit�� ������Ʈ�� �ڸ��ϴ�.
                    // �� ��ũ��Ʈ������ Raycast�� ���� hit.collider.gameObject�� �ڸ��� ���� �� �ڿ��������ϴ�.
                    Slice_Object(hit.collider.gameObject);
                }
            }
            previous_pos = transform.position; // Raycast hit.transform.position�� �ƴ� �ڽ��� ���� ��ġ�� �����ؾ� �մϴ�.
            yield return null;
        }
    }

    // Slice_Object �Լ��� �Ű������� �߰��Ͽ� � ������Ʈ�� �ڸ��� ��Ȯ�� �մϴ�.
    private void Slice_Object(GameObject target)
    {
        // CubeController ������Ʈ�� �ִ��� Ȯ���ϰ� ������ �ڸ��� ����
        if (target.TryGetComponent(out CubeController c))
        {
            // Slice_Obj�� CubeController���� �����ɴϴ�.
            Transform Slice_Obj_From_Controller = c.SliceObj;

            // �����̽� ��� ���� ���: Į���� ������ ����(���� ��ġ���� ���� ��ġ)�� ���� ������ ����
            // Vector3.Cross(transform.position - previous_pos, transform.forward)��
            // Į���� ������ ����� Į���� ������ �������� �����̽� ����� ���� ���͸� �����մϴ�.
            // �����̽� ������ Slice_Obj_From_Controller.position�� slice_normal�� �Ǿ�� �մϴ�.
            Vector3 slice_normal = Vector3.Cross(transform.position - previous_pos, transform.forward).normalized;
            if (slice_normal == Vector3.zero) // �������� ��� normal�� 0�� �Ǵ� ��� �⺻�� ����
            {
                slice_normal = transform.forward; // �Ǵ� �ٸ� ������ �⺻ ����
            }

            // EzySlice�� ����� ��� ������Ʈ �ڸ���
            // Slice_Obj_From_Controller.position�� ���� slice_normal ���
            SlicedHull hull = target.Slice(Slice_Obj_From_Controller.position, slice_normal, Cross_M); // Cross_M�� �ٷ� �Ѱ��ּ���.

            // �ڸ��Ⱑ ����������
            if (hull != null)
            {
                // UpperHull�� LowerHull ���� �� ��� ���� ����� EzySlice ������ ������ ���� �ٸ� �� �ֽ��ϴ�.
                // �Ϲ������� CreateUpperHull, CreateLowerHull�� �Ű������� Material�� �޽��ϴ�.
                // ���⼭�� Material�� �ٷ� �Ѱ��ֵ��� �����߽��ϴ�.
                GameObject UpperHull = hull.CreateUpperHull(target, Cross_M);
                GameObject LowerHull = hull.CreateLowerHull(target, Cross_M); // CreateUpperHull�� �ƴ� CreateLowerHull�� ���

                // ������ ����鿡 ���� ȿ�� �߰�
                Setup_Slice_component(UpperHull);
                Setup_Slice_component(LowerHull);

                // �ڽ� ������Ʈ�鵵 �Բ� �ڸ���
                // ����: target.transform.childCount ��� g.transform.childCount�� �����ؾ� �մϴ�.
                // �׸��� CreateUpperHull ȣ�� �� UpperHull�� �θ�� �ѱ�� ���ڰ� �´��� Ȯ���ؾ� �մϴ�.
                // EzySlice�� ������ ���� CreateUpperHull/CreateLowerHull�� �Ϲ������� ��� ������Ʈ�� ��Ḹ �޽��ϴ�.
                // �ڽ� ������Ʈ�鵵 ������ �������� ����� ���ο� UpperHull�� LowerHull�� �ڽ����� ���̷���
                // �߰����� ������ �ʿ��ϸ�, EzySlice�� �⺻ ���۰� �ణ �ٸ� �� �ֽ��ϴ�.
                // ���⼭�� �ڽ� ������Ʈ�� ���������� �߶� �� ������ �����ϵ��� �����߽��ϴ�.

                // �ڽ� ������Ʈ�� �ݺ������� target.transform.childCount�� ����ؾ� �մϴ�.
                // transform.childCount�� �� ��ũ��Ʈ�� ���� ������Ʈ�� �ڽ��� �ǹ��մϴ�.
                // ����, �ڽ� ������Ʈ�� �ڸ� �� ���ο� ������ �����ǹǷ�, ������ �������� ���� UpperHull/LowerHull�� �ڽ����� �ٿ��ַ���
                // SetParent�� ����ؾ� �մϴ�. �� �κ��� EzySlice�� ��� ������ �ش��ϹǷ� ������ �� ������ ������ �ϵ��� �ΰڽ��ϴ�.
                if (target.transform.childCount > 0)
                {
                    // �ڽ� ������Ʈ���� ����Ʈ�� �����Ͽ� �ݺ� ���� �÷����� ����Ǵ� ���� �����մϴ�.
                    List<GameObject> childrenToSlice = new List<GameObject>();
                    for (int i = 0; i < target.transform.childCount; i++)
                    {
                        GameObject child = target.transform.GetChild(i).gameObject;
                        if (child != Slice_Obj_From_Controller.gameObject) // Slice_Obj ��ü�� �ڸ��� ����
                        {
                            childrenToSlice.Add(child);
                        }
                    }

                    foreach (GameObject g_child in childrenToSlice)
                    {
                        // �ڽ� ������Ʈ�� ��ġ�� ���� �����̽� ��� ���
                        SlicedHull hull_c = g_child.Slice(Slice_Obj_From_Controller.position, slice_normal, Cross_M);

                        if (hull_c != null)
                        {
                            GameObject upper_c = hull_c.CreateUpperHull(g_child, Cross_M);
                            GameObject lower_c = hull_c.CreateLowerHull(g_child, Cross_M);

                            Setup_Slice_component(upper_c);
                            Setup_Slice_component(lower_c);

                            // ���� �ڽ� ������Ʈ�� �ı�
                            Destroy(g_child);
                        }
                    }
                }

                // ���� ��� ������Ʈ�� �ı�
                Destroy(target);

                // �߸� �������� ���� �ð� �� �ı� (��: 1.0f �� ��)
                Destroy(UpperHull, 1.0f);
                Destroy(LowerHull, 1.0f);
            }
        }
    }

    // ���� ������Ʈ �� �� �߰� �Լ�
    private void Setup_Slice_component(GameObject g)
    {
        if (g == null) return; // null üũ �߰�

        Rigidbody rb = g.GetComponent<Rigidbody>();
        if (rb == null) // Rigidbody�� ������ �߰�
        {
            rb = g.AddComponent<Rigidbody>();
        }

        MeshCollider c = g.GetComponent<MeshCollider>();
        if (c == null) // MeshCollider�� ������ �߰�
        {
            c = g.AddComponent<MeshCollider>();
        }

        c.convex = true; // MeshCollider�� convex���� Rigidbody�� ����� �۵��մϴ�.

        // ���߷� �߰� (cutForce ���)
        // AddExplosionForce�� Ư�� �������������� ���߷��� �ùķ��̼��մϴ�.
        // ���⼭�� �߸� ������ ��ġ���� ���߷��� ���������� �����մϴ�.
        rb.AddExplosionForce(cutForce, g.transform.position, 1f); // �ݰ� �߰�
    }

    // Update�� �ڷ�ƾ���� ó���ϹǷ� ����Ӵϴ�.
    void Update()
    {

    }
}