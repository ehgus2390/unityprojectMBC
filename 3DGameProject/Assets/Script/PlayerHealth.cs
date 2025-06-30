using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    public Animator animator; // �߰�
    public GameObject gameOverUI; // �߰�

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
            healthSlider.value = (float)currentHealth / maxHealth;
        if (healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;
    }

    void Die()
    {
        Debug.Log("�÷��̾� ���!");

        if (animator != null)
            animator.SetTrigger("Die");

        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;
    }
}