using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    public float damage = 10f; // �ν����Ϳ��� ������ ����

    private void OnTriggerEnter(Collider other)
    {
        // AlienAI�� ������
        AlienAI alien = other.GetComponent<AlienAI>();
        if (alien != null)
        {
            alien.TakeDamage((int)damage);
        }

        // PlayerHealth�� ������ (�÷��̾ ���� �� �ִٸ�)
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage((int)damage);
        }

        // �Ѿ� �ı� (�ʿ��ϴٸ�)
        Destroy(gameObject);
    }
}