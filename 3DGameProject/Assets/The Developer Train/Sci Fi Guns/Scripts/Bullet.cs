using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletspeed = 6000f;
    public float lifeTime = 3f;
    public int damage = 100;

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.velocity = transform.forward * bulletspeed;
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // AlienAI에 데미지 주기
        AlienAI alien = other.GetComponent<AlienAI>();
        if (alien != null)
        {
            alien.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}