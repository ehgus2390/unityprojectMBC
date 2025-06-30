using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    public float damage = 10f; // 인스펙터에서 데미지 조절

    private void OnTriggerEnter(Collider other)
    {
        // AlienAI에 데미지
        AlienAI alien = other.GetComponent<AlienAI>();
        if (alien != null)
        {
            alien.TakeDamage((int)damage);
        }

        // PlayerHealth에 데미지 (플레이어도 맞을 수 있다면)
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage((int)damage);
        }

        // 총알 파괴 (필요하다면)
        Destroy(gameObject);
    }
}