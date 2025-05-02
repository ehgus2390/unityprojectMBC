using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatForm : MonoBehaviour
{
    public GameObject[] obstacles;
    private bool stepped = false;
    private ObjectPool<PlatForm> pool;

    public void SetPool(ObjectPool<PlatForm> pool)
    {
        this.pool = pool;
    }

    private void OnEnable()
    {
        stepped = false;
        // Randomly activate obstacles
        for (int i = 0; i < obstacles.Length; i++)
        {
            if (Random.Range(0, 3) == 0)
            {
                obstacles[i].SetActive(true);
            }
            else
            {
                obstacles[i].SetActive(false);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") && !stepped)
        {
            stepped = true;
            GameManager.Instance.AddScore(1);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Return to pool when platform goes off screen
            if (transform.position.x < -20f)
            {
                if (pool != null)
                {
                    pool.ReturnPool(this);
                }
                else
                {
                    Debug.LogWarning("Pool is null");
                }
            }
        }
    }
}