using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Image fillImage;

    void Update()
    {
        if (health != null && fillImage != null)
        {
            fillImage.fillAmount = health.GetHealthPercentage();
        }
    }

    public void SetHealth(Health targetHealth)
    {
        health = targetHealth;
    }
}
