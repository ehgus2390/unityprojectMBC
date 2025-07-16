using UnityEngine;
using UnityEngine.UI;

public class CastleHealthUI : MonoBehaviour
{
    public CastleHealth castleHealth;
    public Slider hpSlider;
    public Text hpText;

    void Start()
    {
        if (castleHealth != null)
        {
            castleHealth.OnHealthChanged.AddListener(UpdateUI);
            UpdateUI(castleHealth.CurrentHealth, castleHealth.MaxHealth);
        }
    }

   
    public void UpdateUI(int current, int max)
    {
        if (hpSlider != null)
            hpSlider.value = (float)current / max;
        if (hpText != null)
            hpText.text = $"HP: {current} / {max}";
    }
}
