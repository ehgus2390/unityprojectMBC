using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet1 : MonoBehaviour
{
    private float speed;
    private float damage;
    private float lifetime = 5f;
    [SerializeField] private float explosionDelay = 2f;
    [SerializeField] private GameObject explosionEffect;

    private bool isMoving = true;
    private Animator animator;

    public void Initialize(float bulletSpeed, float bulletDamage)
    {
        speed = bulletSpeed;
        damage = bulletDamage;
        Destroy(gameObject, lifetime);
    }
}
